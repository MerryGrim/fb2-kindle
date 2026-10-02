using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Fb2Kindle
{
    public sealed class ConversionResult
    {
        public string Title;
        public string Author;
        public int ChapterCount;
        public List<string> Warnings = new List<string>();
    }

    // No network access, external programs, or modifications to the original book.
    public static class Fb2Converter
    {
        private const long MaxInputBytes = 96L * 1024 * 1024;
        private const long MaxArchiveBytes = 256L * 1024 * 1024;
        private const int MaxDepth = 96;
        private const int MaxNodes = 600000;
        private const int MaxImageBytes = 24 * 1024 * 1024;
        private const long MaxAllImages = 80L * 1024 * 1024;
        private static readonly XNamespace Html = "http://www.w3.org/1999/xhtml";
        private static readonly XNamespace Epub = "http://www.idpf.org/2007/ops";
        private static readonly XNamespace Dc = "http://purl.org/dc/elements/1.1/";
        private static readonly XNamespace Opf = "http://www.idpf.org/2007/opf";
        private static readonly XNamespace Ncx = "http://www.daisy.org/z3986/2005/ncx/";

        private sealed class Chapter
        {
            public string File;
            public string Title;
            public bool Notes;
            public bool Ancillary;
            public List<XElement> Sources = new List<XElement>();
            public XElement Content;
        }
        private sealed class Target
        {
            public Chapter Chapter;
            public string Id;
            public XElement Source;
        }
        private sealed class Picture
        {
            public string File;
            public string MediaType;
            public byte[] Bytes;
        }
        private sealed class TocItem
        {
            public string Title;
            public string Href;
            public List<TocItem> Children = new List<TocItem>();
        }
        private sealed class Backlink
        {
            public Chapter Chapter;
            public string Id;
        }
        private sealed class Book
        {
            public ConversionResult Result = new ConversionResult();
            public string Language;
            public string Identifier = "urn:uuid:" + Guid.NewGuid().ToString();
            public string Publisher;
            public string Series;
            public string SeriesNumber;
            public string CoverId;
            public List<string> Authors = new List<string>();
            public List<Chapter> Chapters = new List<Chapter>();
            public List<TocItem> Toc = new List<TocItem>();
            public Dictionary<string, Target> Targets = new Dictionary<string, Target>(StringComparer.Ordinal);
            public Dictionary<XElement, Target> ElementTargets = new Dictionary<XElement, Target>();
            public Dictionary<string, Picture> Pictures = new Dictionary<string, Picture>(StringComparer.Ordinal);
            public Dictionary<XElement, List<Backlink>> Backlinks = new Dictionary<XElement, List<Backlink>>();
            public Dictionary<XElement, string> NoteReferences = new Dictionary<XElement, string>();
            public HashSet<string> WarningSet = new HashSet<string>(StringComparer.Ordinal);
            public int NextId;
            public int NextReference;
            public void Warn(string text)
            {
                if (WarningSet.Add(text) && Result.Warnings.Count < 120) Result.Warnings.Add(text);
            }
        }

        public static ConversionResult Convert(string inputPath, string outputPath)
        {
            if (String.IsNullOrWhiteSpace(inputPath)) throw new ArgumentException(L10n.T("Не выбран исходный файл FB2."));
            if (String.IsNullOrWhiteSpace(outputPath)) throw new ArgumentException(L10n.T("Не выбран путь для сохранения EPUB."));
            inputPath = Path.GetFullPath(inputPath);
            outputPath = Path.GetFullPath(outputPath);
            if (!File.Exists(inputPath)) throw new FileNotFoundException(L10n.T("Исходный файл не найден."), inputPath);
            if (String.Equals(inputPath, outputPath, StringComparison.OrdinalIgnoreCase))
                throw new IOException(L10n.T("Исходный файл и готовая книга должны иметь разные имена."));
            if (File.Exists(outputPath) || Directory.Exists(outputPath))
                throw new IOException(L10n.T("Файл с таким именем уже существует. Выберите другое имя для EPUB."));
            string outputDirectory = Path.GetDirectoryName(outputPath);
            if (!Directory.Exists(outputDirectory)) throw new DirectoryNotFoundException(L10n.T("Папка для сохранения не найдена."));
            byte[] input = ReadInput(inputPath);
            XDocument document = ReadXml(input);
            if (document.Root == null || document.Root.Name.LocalName != "FictionBook")
                throw new InvalidDataException(L10n.T("В файле нет корневого элемента FictionBook. Это не книга FB2."));
            Book book = BuildBook(document, inputPath);
            // The sibling temporary file and final move are on the same volume. File.Move
            // refuses an existing destination, including one created during conversion.
            string temporary = Path.Combine(outputDirectory, ".fb2kindle-" + Guid.NewGuid().ToString("N") + ".tmp");
            try
            {
                using (FileStream output = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                using (EpubZip zip = new EpubZip(output))
                {
                    zip.Add("mimetype", Encoding.ASCII.GetBytes("application/epub+zip"), false);
                    zip.Add("META-INF/container.xml", XmlBytes(ContainerDocument()), true);
                    zip.Add("OEBPS/styles/style.css", Encoding.UTF8.GetBytes(Styles), true);
                    foreach (Chapter chapter in book.Chapters)
                        zip.Add("OEBPS/text/" + chapter.File, XmlBytes(ChapterDocument(book, chapter)), true);
                    foreach (Picture picture in book.Pictures.Values)
                        zip.Add("OEBPS/images/" + picture.File, picture.Bytes, false);
                    zip.Add("OEBPS/nav.xhtml", XmlBytes(NavigationDocument(book)), true);
                    zip.Add("OEBPS/toc.ncx", XmlBytes(NcxDocument(book)), true);
                    zip.Add("OEBPS/content.opf", XmlBytes(PackageDocument(book)), true);
                }
                File.Move(temporary, outputPath);
                return book.Result;
            }
            finally
            {
                if (File.Exists(temporary))
                {
                    try { File.Delete(temporary); } catch (IOException) { } catch (UnauthorizedAccessException) { }
                }
            }
        }

        private static byte[] ReadInput(string path)
        {
            string extension = Path.GetExtension(path).ToLowerInvariant();
            if (extension != ".fb2" && extension != ".zip")
                throw new InvalidDataException(L10n.T("Выберите файл .fb2 или ZIP-архив с одной книгой FB2."));
            using (FileStream input = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                if (extension == ".fb2") return ReadBounded(input, MaxInputBytes);
                if (input.Length > MaxArchiveBytes)
                    throw new InvalidDataException(L10n.T("ZIP-архив слишком большой: предел — 256 МБ."));
                try
                {
                    using (ZipArchive zip = new ZipArchive(input, ZipArchiveMode.Read, true))
                    {
                        if (zip.Entries.Count > 10000)
                            throw new InvalidDataException(L10n.T("В ZIP-архиве слишком много файлов."));
                        List<ZipArchiveEntry> books = zip.Entries.Where(e =>
                            e.Name.Length > 0 && e.FullName.EndsWith(".fb2", StringComparison.OrdinalIgnoreCase)).ToList();
                        if (books.Count == 0) throw new InvalidDataException(L10n.T("В ZIP-архиве не найден файл FB2."));
                        if (books.Count > 1)
                            throw new InvalidDataException(L10n.T("В ZIP-архиве несколько книг FB2. Распакуйте архив и выберите одну книгу."));
                        if (books[0].Length > MaxInputBytes)
                            throw new InvalidDataException(L10n.T("Книга в ZIP-архиве слишком большая: предел — 96 МБ."));
                        using (Stream entry = books[0].Open()) return ReadBounded(entry, MaxInputBytes);
                    }
                }
                catch (InvalidDataException exception)
                {
                    if (exception.Message.Contains("FB2") ||
                        exception.Message == L10n.T("ZIP-архив слишком большой: предел — 256 МБ.") ||
                        exception.Message == L10n.T("В ZIP-архиве слишком много файлов.") ||
                        exception.Message == L10n.T("Книга в ZIP-архиве слишком большая: предел — 96 МБ.")) throw;
                    throw new InvalidDataException(L10n.T("Не удалось прочитать ZIP-архив. Проверьте, что он не повреждён и не защищён паролем."), exception);
                }
            }
        }

        private static byte[] ReadBounded(Stream input, long limit)
        {
            if (input.CanSeek && input.Length > limit)
                throw new InvalidDataException(L10n.T("Книга слишком большая: предел для FB2 — 96 МБ."));
            using (MemoryStream output = new MemoryStream())
            {
                byte[] buffer = new byte[65536];
                int count;
                while ((count = input.Read(buffer, 0, buffer.Length)) != 0)
                {
                    if (output.Length + count > limit)
                        throw new InvalidDataException(L10n.T("Книга слишком большая: предел для FB2 — 96 МБ."));
                    output.Write(buffer, 0, count);
                }
                return output.ToArray();
            }
        }

        private static XDocument ReadXml(byte[] bytes)
        {
            XmlReaderSettings settings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null,
                MaxCharactersInDocument = 100L * 1024 * 1024,
                MaxCharactersFromEntities = 1024,
                IgnoreComments = true,
                IgnoreProcessingInstructions = true
            };
            try
            {
                // Check depth before creating an object tree, so deeply nested input cannot
                // overflow the renderer or XML writer stack.
                using (MemoryStream input = new MemoryStream(bytes, false))
                using (XmlReader reader = XmlReader.Create(input, settings))
                {
                    int nodes = 0;
                    while (reader.Read())
                    {
                        if (reader.Depth > MaxDepth || ++nodes > MaxNodes)
                            throw new InvalidDataException(L10n.T("Слишком сложная структура FB2: превышен предел вложенности или числа элементов."));
                    }
                }
                using (MemoryStream input = new MemoryStream(bytes, false))
                using (XmlReader reader = XmlReader.Create(input, settings))
                    return XDocument.Load(reader, LoadOptions.PreserveWhitespace);
            }
            catch (XmlException exception)
            {
                throw new InvalidDataException(L10n.T("Не удалось прочитать FB2: некорректный XML или запрещённый DTD. Строка ") +
                    exception.LineNumber.ToString(CultureInfo.InvariantCulture) + ".", exception);
            }
            catch (ArgumentException exception)
            {
                throw new InvalidDataException(L10n.T("В FB2 указана неподдерживаемая кодировка текста."), exception);
            }
        }

        private static Book BuildBook(XDocument document, string inputPath)
        {
            Book book = new Book();
            XElement root = document.Root;
            XElement description = Child(root, "description");
            XElement titleInfo = Child(description, "title-info");
            book.Result.Title = Text(Child(titleInfo, "book-title"));
            if (book.Result.Title.Length == 0)
            {
                book.Result.Title = Path.GetFileNameWithoutExtension(inputPath);
                if (book.Result.Title.EndsWith(".fb2", StringComparison.OrdinalIgnoreCase))
                    book.Result.Title = Path.GetFileNameWithoutExtension(book.Result.Title);
                book.Warn(L10n.T("В FB2 нет названия книги. Использовано имя файла."));
            }
            if (titleInfo != null)
            {
                foreach (XElement author in Children(titleInfo, "author"))
                {
                    string name = String.Join(" ", new[] { Text(Child(author, "first-name")),
                        Text(Child(author, "middle-name")), Text(Child(author, "last-name")) }.Where(s => s.Length != 0));
                    if (name.Length == 0) name = Text(Child(author, "nickname"));
                    if (name.Length != 0) book.Authors.Add(name);
                }
            }
            book.Result.Author = String.Join(", ", book.Authors);
            book.Language = Text(Child(titleInfo, "lang"));
            if (book.Language.Length == 0) book.Language = "und";
            // Invalid language values must not produce malformed XML or package metadata.
            if (book.Language.Length > 64 || book.Language.Any(c => !(Char.IsLetterOrDigit(c) || c == '-')))
            {
                book.Language = "und";
                book.Warn(L10n.T("Некорректный код языка в FB2 заменён на «und»."));
            }
            XElement sequence = Child(titleInfo, "sequence");
            book.Series = Attribute(sequence, "name");
            book.SeriesNumber = Attribute(sequence, "number");
            book.Publisher = Text(Child(Child(description, "publish-info"), "publisher"));
            XElement coverpage = Child(titleInfo, "coverpage");
            XElement coverImage = Child(coverpage, "image");
            book.CoverId = Fragment(Attribute(coverImage, "href"));
            LoadPictures(book, root);

            Picture cover;
            if (book.CoverId.Length != 0 && book.Pictures.TryGetValue(book.CoverId, out cover))
            {
                Chapter chapter = AddChapter(book, L10n.T("Обложка"), false, true);
                chapter.Content = new XElement(Html + "div", new XAttribute("class", "cover"),
                    new XElement(Html + "img", new XAttribute("src", "../images/" + cover.File),
                        new XAttribute("alt", L10n.T("Обложка: ") + book.Result.Title)));
            }
            else if (book.CoverId.Length != 0) book.Warn(L10n.T("Изображение обложки не найдено или не поддерживается."));

            XElement annotation = Child(titleInfo, "annotation");
            if (annotation != null && (Text(annotation).Length != 0 || Children(annotation, "image").Any()))
            {
                Chapter chapter = AddChapter(book, L10n.T("Аннотация"), false, true);
                chapter.Sources.Add(annotation);
            }

            List<XElement> bodies = Children(root, "body").ToList();
            if (bodies.Count == 0) throw new InvalidDataException(L10n.T("В FB2 нет содержимого книги: элемент body отсутствует."));
            int mainBodyNumber = 0;
            foreach (XElement body in bodies)
            {
                string name = Attribute(body, "name");
                bool notes = String.Equals(name, "notes", StringComparison.OrdinalIgnoreCase) ||
                    String.Equals(name, "comments", StringComparison.OrdinalIgnoreCase);
                string fallback = notes ? L10n.T("Примечания") : (++mainBodyNumber == 1 ? book.Result.Title :
                    (name.Length == 0 ? L10n.T("Часть ") + mainBodyNumber.ToString(CultureInfo.InvariantCulture) : name));
                Chapter pending = null;
                int unnamed = 0;
                foreach (XElement element in body.Elements())
                {
                    if (element.Name.LocalName == "section")
                    {
                        pending = null;
                        string title = Text(Child(element, "title"));
                        if (title.Length == 0) title = fallback + " — " + (++unnamed).ToString(CultureInfo.InvariantCulture);
                        Chapter chapter = AddChapter(book, title, notes, false);
                        chapter.Sources.Add(element);
                    }
                    else
                    {
                        if (pending == null) pending = AddChapter(book, fallback, notes, false);
                        pending.Sources.Add(element);
                    }
                }
            }
            if (!book.Chapters.Any(c => !c.Ancillary && c.Sources.Any()))
                throw new InvalidDataException(L10n.T("В FB2 нет текста или изображений для конвертации."));

            foreach (Chapter chapter in book.Chapters)
            {
                foreach (XElement source in chapter.Sources)
                {
                    foreach (XElement element in source.DescendantsAndSelf())
                    {
                        string sourceId = Attribute(element, "id");
                        if (sourceId.Length == 0 && element.Name.LocalName != "section") continue;
                        Target target = new Target { Chapter = chapter, Source = element,
                            Id = "id" + (++book.NextId).ToString("D6", CultureInfo.InvariantCulture) };
                        book.ElementTargets[element] = target;
                        if (sourceId.Length != 0)
                        {
                            if (!book.Targets.ContainsKey(sourceId)) book.Targets[sourceId] = target;
                            else book.Warn(L10n.T("Повторяющийся идентификатор «") + Short(sourceId) + L10n.T("»: ссылки ведут к первому элементу."));
                        }
                    }
                }
            }
            // Resolve notes before rendering any chapter; targets may appear later in the book.
            foreach (Chapter chapter in book.Chapters)
                foreach (XElement source in chapter.Sources)
                    foreach (XElement anchor in source.DescendantsAndSelf().Where(e => e.Name.LocalName == "a"))
                    {
                        Target target;
                        string href = Attribute(anchor, "href");
                        if (!href.StartsWith("#", StringComparison.Ordinal) || !book.Targets.TryGetValue(Fragment(href), out target)) continue;
                        if (!IsNote(anchor, target)) continue;
                        Target referenceTarget;
                        string referenceId = book.ElementTargets.TryGetValue(anchor, out referenceTarget) ? referenceTarget.Id :
                            "ref" + (++book.NextReference).ToString("D6", CultureInfo.InvariantCulture);
                        book.NoteReferences[anchor] = referenceId;
                        XElement noteContainer = target.Source;
                        // Place backlinks after the actual footnote section, including when the
                        // source id is on its title or paragraph.
                        XElement section = target.Source.AncestorsAndSelf().FirstOrDefault(e => e.Name.LocalName == "section");
                        if (section != null) noteContainer = section;
                        List<Backlink> links;
                        if (!book.Backlinks.TryGetValue(noteContainer, out links))
                            book.Backlinks[noteContainer] = links = new List<Backlink>();
                        links.Add(new Backlink { Chapter = chapter, Id = referenceId });
                    }

            foreach (Chapter chapter in book.Chapters)
            {
                if (chapter.Content == null)
                {
                    chapter.Content = new XElement(Html + "div", new XAttribute("class", chapter.Notes ? "notes" : "chapter"));
                    foreach (XElement source in chapter.Sources)
                        chapter.Content.Add(RenderElement(book, chapter, source, 0));
                }
                TocItem item = new TocItem { Title = chapter.Title, Href = "text/" + chapter.File };
                XElement mainSection = chapter.Sources.Count == 1 && chapter.Sources[0].Name.LocalName == "section" ? chapter.Sources[0] : null;
                if (mainSection != null)
                {
                    Target sectionTarget;
                    if (book.ElementTargets.TryGetValue(mainSection, out sectionTarget)) item.Href += "#" + sectionTarget.Id;
                    AddSectionToc(book, chapter, mainSection, item.Children);
                }
                else
                    foreach (XElement source in chapter.Sources) AddSectionToc(book, chapter, source, item.Children);
                book.Toc.Add(item);
            }
            book.Result.ChapterCount = book.Chapters.Count(c => !c.Ancillary && !c.Notes);
            if (book.Result.ChapterCount == 0) book.Result.ChapterCount = book.Chapters.Count(c => !c.Ancillary);
            ValidateGeneratedLinks(book);
            return book;
        }

        private static Chapter AddChapter(Book book, string title, bool notes, bool ancillary)
        {
            if (book.Chapters.Count >= 10000)
                throw new InvalidDataException(L10n.T("В книге слишком много разделов: предел — 10 000."));
            Chapter chapter = new Chapter { Title = title, Notes = notes, Ancillary = ancillary,
                File = "ch" + (book.Chapters.Count + 1).ToString("D5", CultureInfo.InvariantCulture) + ".xhtml" };
            book.Chapters.Add(chapter);
            return chapter;
        }

        private static void LoadPictures(Book book, XElement root)
        {
            long total = 0;
            int number = 0;
            foreach (XElement binary in Children(root, "binary"))
            {
                string id = Attribute(binary, "id");
                if (id.Length == 0) { book.Warn(L10n.T("Встроенное изображение без идентификатора пропущено.")); continue; }
                if (book.Pictures.ContainsKey(id)) { book.Warn(L10n.T("Повторяющееся изображение «") + Short(id) + L10n.T("» пропущено.")); continue; }
                if (binary.Value.Length > MaxImageBytes * 2)
                    throw new InvalidDataException(L10n.T("Слишком большое встроенное изображение в FB2: «") + Short(id) + L10n.T("»."));
                byte[] bytes;
                try { bytes = System.Convert.FromBase64String(binary.Value); }
                catch (FormatException) { book.Warn(L10n.T("Повреждённое изображение «") + Short(id) + L10n.T("» пропущено.")); continue; }
                if (bytes.Length > MaxImageBytes) throw new InvalidDataException(L10n.T("Встроенное изображение превышает предел 24 МБ."));
                int originalSize = bytes.Length;
                total += originalSize;
                if (total > MaxAllImages) throw new InvalidDataException(L10n.T("Общий размер изображений превышает предел 80 МБ."));
                string extension = null;
                string mediaType = null;
                if (bytes.Length >= 8 && bytes[0] == 137 && bytes[1] == 80 && bytes[2] == 78 && bytes[3] == 71 &&
                    bytes[4] == 13 && bytes[5] == 10 && bytes[6] == 26 && bytes[7] == 10)
                { extension = ".png"; mediaType = "image/png"; }
                else if (bytes.Length >= 3 && bytes[0] == 255 && bytes[1] == 216 && bytes[2] == 255)
                { extension = ".jpg"; mediaType = "image/jpeg"; }
                else if (bytes.Length >= 6 && Encoding.ASCII.GetString(bytes, 0, 6).StartsWith("GIF8", StringComparison.Ordinal))
                { extension = ".gif"; mediaType = "image/gif"; }
                else if (bytes.Length >= 2 && bytes[0] == 66 && bytes[1] == 77)
                {
                    try
                    {
                        using (MemoryStream source = new MemoryStream(bytes, false))
                        using (Image image = Image.FromStream(source, true, true))
                        {
                            if ((long)image.Width * image.Height > 40000000)
                            { book.Warn(L10n.T("Изображение «") + Short(id) + L10n.T("» слишком большое по разрешению и пропущено.")); continue; }
                            using (MemoryStream target = new MemoryStream())
                            { image.Save(target, ImageFormat.Png); bytes = target.ToArray(); }
                        }
                        if (bytes.Length > MaxImageBytes)
                        { book.Warn(L10n.T("BMP-изображение «") + Short(id) + L10n.T("» слишком большое после преобразования и пропущено.")); continue; }
                        extension = ".png"; mediaType = "image/png";
                        total += bytes.Length - originalSize;
                        if (total > MaxAllImages) throw new InvalidDataException(L10n.T("Общий размер изображений превышает предел 80 МБ."));
                        book.Warn(L10n.T("BMP-изображение «") + Short(id) + L10n.T("» преобразовано в PNG."));
                    }
                    catch (ArgumentException) { book.Warn(L10n.T("Повреждённое BMP-изображение «") + Short(id) + L10n.T("» пропущено.")); continue; }
                    catch (System.Runtime.InteropServices.ExternalException) { book.Warn(L10n.T("Не удалось преобразовать BMP-изображение «") + Short(id) + L10n.T("».")); continue; }
                }
                if (extension == null)
                { book.Warn(L10n.T("Формат изображения «") + Short(id) + L10n.T("» не поддерживается. Поддерживаются JPEG, PNG, GIF и BMP.")); continue; }
                Picture picture = new Picture { File = "img" + (++number).ToString("D5", CultureInfo.InvariantCulture) + extension,
                    MediaType = mediaType, Bytes = bytes };
                book.Pictures[id] = picture;
            }
        }

        private static XElement RenderElement(Book book, Chapter chapter, XElement source, int depth)
        {
            string name = source.Name.LocalName;
            string tag;
            string cssClass = null;
            switch (name)
            {
                case "section": tag = "section"; break;
                case "title":
                    int level = Math.Min(6, Math.Max(1, depth));
                    tag = "h" + level.ToString(CultureInfo.InvariantCulture); break;
                case "subtitle": tag = "h" + Math.Min(6, Math.Max(2, depth + 1)).ToString(CultureInfo.InvariantCulture); cssClass = "subtitle"; break;
                case "p": tag = source.Parent != null && source.Parent.Name.LocalName == "title" ? "span" : "p"; break;
                case "empty-line": tag = "div"; cssClass = "empty-line"; break;
                case "strong": tag = "strong"; break;
                case "emphasis": tag = "em"; break;
                case "strikethrough": tag = "del"; break;
                case "sup": tag = "sup"; break;
                case "sub": tag = "sub"; break;
                case "code": tag = "code"; break;
                case "a": tag = "a"; break;
                case "image": tag = "img"; break;
                case "poem": tag = "div"; cssClass = "poem"; break;
                case "stanza": tag = "div"; cssClass = "stanza"; break;
                case "v": tag = "div"; cssClass = "verse"; break;
                case "epigraph": tag = "blockquote"; cssClass = "epigraph"; break;
                case "cite": tag = "blockquote"; break;
                case "text-author": tag = "p"; cssClass = "text-author"; break;
                case "date": tag = "p"; cssClass = "date"; break;
                case "annotation": tag = "section"; cssClass = "annotation"; break;
                case "table": tag = "table"; break;
                case "tr": tag = "tr"; break;
                case "td": tag = "td"; break;
                case "th": tag = "th"; break;
                case "style": tag = "span"; break;
                default:
                    tag = "div";
                    if (source.Parent != null && new[] { "p", "v", "a", "emphasis", "strong", "style", "td", "th" }.Contains(source.Parent.Name.LocalName)) tag = "span";
                    book.Warn(L10n.T("Элемент FB2 «") + Short(name) + L10n.T("» сохранён как обычный текстовый блок.")); break;
            }
            XElement target = new XElement(Html + tag);
            if (cssClass != null) target.SetAttributeValue("class", cssClass);
            Target mapped;
            if (book.ElementTargets.TryGetValue(source, out mapped)) target.SetAttributeValue("id", mapped.Id);
            if (name == "section" && chapter.Notes) target.SetAttributeValue(Epub + "type", "footnote");
            if (name == "image")
            {
                string href = Attribute(source, "href");
                string id = Fragment(href);
                Picture picture;
                if (!href.StartsWith("#", StringComparison.Ordinal) || !book.Pictures.TryGetValue(id, out picture))
                {
                    book.Warn(L10n.T("Изображение «") + Short(href) + L10n.T("» не найдено или не поддерживается."));
                    target.Name = Html + "span";
                    target.SetAttributeValue("class", "missing-image");
                    target.Value = L10n.T("[Изображение недоступно]");
                    return target;
                }
                target.SetAttributeValue("src", "../images/" + picture.File);
                string alt = Attribute(source, "alt");
                target.SetAttributeValue("alt", alt.Length == 0 ? L10n.T("Иллюстрация") : alt);
                string imageTitle = Attribute(source, "title");
                if (imageTitle.Length != 0) target.SetAttributeValue("title", imageTitle);
                if (source.Parent != null && new[] { "p", "a", "v", "emphasis", "strong" }.Contains(source.Parent.Name.LocalName))
                    target.SetAttributeValue("class", "inline-image");
                return target;
            }
            if (name == "a")
            {
                string href = Attribute(source, "href");
                Target destination;
                if (href.StartsWith("#", StringComparison.Ordinal))
                {
                    if (book.Targets.TryGetValue(Fragment(href), out destination))
                    {
                        target.SetAttributeValue("href", destination.Chapter.File + "#" + destination.Id);
                        if (IsNote(source, destination))
                        {
                            target.SetAttributeValue(Epub + "type", "noteref");
                            string reference;
                            if (book.NoteReferences.TryGetValue(source, out reference)) target.SetAttributeValue("id", reference);
                        }
                    }
                    else book.Warn(L10n.T("Внутренняя ссылка «") + Short(href) + L10n.T("» не найдена; её текст сохранён."));
                }
                else if (SafeExternalLink(href)) target.SetAttributeValue("href", href);
                else if (href.Length != 0) book.Warn(L10n.T("Неподдерживаемая ссылка «") + Short(href) + L10n.T("» сохранена как текст."));
                if (target.Attribute("href") == null) target.Name = Html + "span";
            }
            if (name == "td" || name == "th")
            {
                foreach (string attribute in new[] { "colspan", "rowspan" })
                {
                    int value;
                    if (Int32.TryParse(Attribute(source, attribute), out value) && value > 0 && value <= 1000)
                        target.SetAttributeValue(attribute, value.ToString(CultureInfo.InvariantCulture));
                }
                string alignment = Attribute(source, "align").ToLowerInvariant();
                if (alignment == "left" || alignment == "right" || alignment == "center" || alignment == "justify")
                    target.SetAttributeValue("class", "align-" + alignment);
            }
            if (name == "empty-line") target.Add(new XElement(Html + "br"));
            int childDepth = name == "section" ? depth + 1 : depth;
            bool titleParagraphSeen = false;
            foreach (XNode node in source.Nodes())
            {
                XElement element = node as XElement;
                if (element != null)
                {
                    if (name == "title" && element.Name.LocalName == "p")
                    {
                        if (titleParagraphSeen) target.Add(new XElement(Html + "br"));
                        titleParagraphSeen = true;
                    }
                    target.Add(RenderElement(book, chapter, element, childDepth));
                }
                else
                {
                    XText text = node as XText;
                    if (text != null) target.Add(new XText(text.Value));
                }
            }
            // A table needs a tbody in XHTML/HTML; FB2 stores its rows directly.
            if (name == "table")
            {
                List<XNode> nodes = target.Nodes().ToList();
                target.RemoveNodes();
                target.Add(new XElement(Html + "tbody", nodes));
            }
            List<Backlink> backlinks;
            if (book.Backlinks.TryGetValue(source, out backlinks))
            {
                XElement paragraph = new XElement(Html + "p", new XAttribute("class", "backlinks"));
                for (int i = 0; i < backlinks.Count; i++)
                {
                    if (i != 0) paragraph.Add(new XText(" "));
                    paragraph.Add(new XElement(Html + "a", new XAttribute("href", backlinks[i].Chapter.File + "#" + backlinks[i].Id),
                        new XAttribute(Epub + "type", "backlink"),
                        backlinks.Count == 1 ? L10n.T("↩ К тексту") : "↩ " + (i + 1).ToString(CultureInfo.InvariantCulture)));
                }
                if (tag == "p" || tag == "span")
                {
                    // Avoid block elements inside paragraphs when an FB2 footnote id is on p.
                    paragraph.Name = Html + "span";
                    paragraph.AddFirst(new XText(" "));
                }
                target.Add(paragraph);
            }
            return target;
        }

        private static bool IsNote(XElement source, Target target)
        {
            return String.Equals(Attribute(source, "type"), "note", StringComparison.OrdinalIgnoreCase) || target.Chapter.Notes;
        }

        private static bool SafeExternalLink(string href)
        {
            Uri uri;
            return Uri.TryCreate(href, UriKind.Absolute, out uri) &&
                (uri.Scheme == "https" || uri.Scheme == "http" || uri.Scheme == "mailto" || uri.Scheme == "ftp");
        }

        private static void AddSectionToc(Book book, Chapter chapter, XElement parent, List<TocItem> result)
        {
            foreach (XElement section in Children(parent, "section"))
            {
                string title = Text(Child(section, "title"));
                if (title.Length == 0)
                {
                    // Anonymous wrappers should not hide named subsections from navigation.
                    AddSectionToc(book, chapter, section, result);
                    continue;
                }
                Target target = book.ElementTargets[section];
                TocItem item = new TocItem { Title = title, Href = "text/" + chapter.File + "#" + target.Id };
                AddSectionToc(book, chapter, section, item.Children);
                result.Add(item);
            }
        }

        private static void ValidateGeneratedLinks(Book book)
        {
            Dictionary<string, HashSet<string>> ids = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
            foreach (Chapter chapter in book.Chapters)
            {
                HashSet<string> chapterIds = new HashSet<string>(StringComparer.Ordinal);
                foreach (XElement element in chapter.Content.DescendantsAndSelf())
                {
                    XAttribute id = element.Attribute("id");
                    if (id != null && !chapterIds.Add(id.Value))
                        throw new InvalidDataException(L10n.T("Не удалось создать уникальные идентификаторы EPUB."));
                }
                ids[chapter.File] = chapterIds;
            }
            foreach (Chapter chapter in book.Chapters)
                foreach (XElement anchor in chapter.Content.DescendantsAndSelf().Where(e => e.Name.LocalName == "a"))
                {
                    XAttribute href = anchor.Attribute("href");
                    if (href == null || SafeExternalLink(href.Value)) continue;
                    int separator = href.Value.IndexOf('#');
                    if (separator < 0) continue;
                    string file = href.Value.Substring(0, separator);
                    string id = href.Value.Substring(separator + 1);
                    HashSet<string> targets;
                    if (!ids.TryGetValue(file, out targets) || !targets.Contains(id))
                        throw new InvalidDataException(L10n.T("Не удалось проверить внутренние ссылки EPUB."));
                }
        }

        private static XDocument ChapterDocument(Book book, Chapter chapter)
        {
            XElement body = new XElement(Html + "body", new XElement(chapter.Content));
            if (chapter.Notes) body.SetAttributeValue(Epub + "type", "endnotes");
            return HtmlDocument(book, chapter.Title, "../styles/style.css", body);
        }

        private static XDocument HtmlDocument(Book book, string title, string style, XElement body)
        {
            return new XDocument(new XDeclaration("1.0", "utf-8", null),
                new XElement(Html + "html", new XAttribute(XNamespace.Xmlns + "epub", Epub.NamespaceName),
                    new XAttribute("lang", book.Language), new XAttribute(XNamespace.Xml + "lang", book.Language),
                    new XElement(Html + "head", new XElement(Html + "title", title),
                        new XElement(Html + "meta", new XAttribute("charset", "utf-8")),
                        new XElement(Html + "link", new XAttribute("rel", "stylesheet"),
                            new XAttribute("type", "text/css"), new XAttribute("href", style))), body));
        }

        private static XElement TocList(List<TocItem> items)
        {
            XElement list = new XElement(Html + "ol");
            foreach (TocItem item in items)
            {
                XElement row = new XElement(Html + "li", new XElement(Html + "a", new XAttribute("href", item.Href), item.Title));
                if (item.Children.Count != 0) row.Add(TocList(item.Children));
                list.Add(row);
            }
            return list;
        }

        private static XDocument NavigationDocument(Book book)
        {
            XElement body = new XElement(Html + "body", new XElement(Html + "nav", new XAttribute(Epub + "type", "toc"),
                new XAttribute("id", "toc"), new XElement(Html + "h1", L10n.T("Содержание")), TocList(book.Toc)));
            XElement landmarks = new XElement(Html + "ol");
            Chapter cover = book.Chapters.FirstOrDefault(c => c.Content.Attribute("class") != null && c.Content.Attribute("class").Value == "cover");
            if (cover != null) landmarks.Add(new XElement(Html + "li", new XElement(Html + "a", new XAttribute(Epub + "type", "cover"),
                new XAttribute("href", "text/" + cover.File), L10n.T("Обложка"))));
            Chapter start = book.Chapters.FirstOrDefault(c => !c.Ancillary && !c.Notes) ?? book.Chapters.First();
            landmarks.Add(new XElement(Html + "li", new XElement(Html + "a", new XAttribute(Epub + "type", "bodymatter"),
                new XAttribute("href", "text/" + start.File), L10n.T("Начало книги"))));
            body.Add(new XElement(Html + "nav", new XAttribute(Epub + "type", "landmarks"),
                new XAttribute("hidden", "hidden"), new XElement(Html + "h2", L10n.T("Навигация")), landmarks));
            return HtmlDocument(book, L10n.T("Содержание"), "styles/style.css", body);
        }

        private static XDocument NcxDocument(Book book)
        {
            XElement navMap = new XElement(Ncx + "navMap");
            int number = 0;
            AppendNcx(book.Toc, navMap, ref number);
            return new XDocument(new XDeclaration("1.0", "utf-8", null),
                new XElement(Ncx + "ncx", new XAttribute("version", "2005-1"), new XAttribute(XNamespace.Xml + "lang", book.Language),
                    new XElement(Ncx + "head",
                        new XElement(Ncx + "meta", new XAttribute("name", "dtb:uid"), new XAttribute("content", book.Identifier)),
                        new XElement(Ncx + "meta", new XAttribute("name", "dtb:depth"), new XAttribute("content", TocDepth(book.Toc).ToString(CultureInfo.InvariantCulture))),
                        new XElement(Ncx + "meta", new XAttribute("name", "dtb:totalPageCount"), new XAttribute("content", "0")),
                        new XElement(Ncx + "meta", new XAttribute("name", "dtb:maxPageNumber"), new XAttribute("content", "0"))),
                    new XElement(Ncx + "docTitle", new XElement(Ncx + "text", book.Result.Title)), navMap));
        }

        private static int TocDepth(List<TocItem> items)
        {
            return items.Count == 0 ? 0 : 1 + items.Max(i => TocDepth(i.Children));
        }

        private static void AppendNcx(List<TocItem> items, XElement parent, ref int number)
        {
            foreach (TocItem item in items)
            {
                int current = ++number;
                XElement point = new XElement(Ncx + "navPoint", new XAttribute("id", "nav" + current.ToString(CultureInfo.InvariantCulture)),
                    new XAttribute("playOrder", current.ToString(CultureInfo.InvariantCulture)),
                    new XElement(Ncx + "navLabel", new XElement(Ncx + "text", item.Title)),
                    new XElement(Ncx + "content", new XAttribute("src", item.Href)));
                AppendNcx(item.Children, point, ref number);
                parent.Add(point);
            }
        }

        private static XDocument PackageDocument(Book book)
        {
            XElement metadata = new XElement(Opf + "metadata", new XAttribute(XNamespace.Xmlns + "dc", Dc.NamespaceName),
                new XElement(Dc + "identifier", new XAttribute("id", "bookid"), book.Identifier),
                new XElement(Dc + "title", book.Result.Title), new XElement(Dc + "language", book.Language),
                new XElement(Opf + "meta", new XAttribute("property", "dcterms:modified"),
                    DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture)));
            for (int i = 0; i < book.Authors.Count; i++)
            {
                string authorId = "author" + (i + 1).ToString(CultureInfo.InvariantCulture);
                metadata.Add(new XElement(Dc + "creator", new XAttribute("id", authorId), book.Authors[i]));
                metadata.Add(new XElement(Opf + "meta", new XAttribute("refines", "#" + authorId),
                    new XAttribute("property", "role"), new XAttribute("scheme", "marc:relators"), "aut"));
            }
            if (book.Publisher.Length != 0) metadata.Add(new XElement(Dc + "publisher", book.Publisher));
            if (book.Series.Length != 0)
            {
                metadata.Add(new XElement(Opf + "meta", new XAttribute("property", "belongs-to-collection"), new XAttribute("id", "series"), book.Series));
                metadata.Add(new XElement(Opf + "meta", new XAttribute("refines", "#series"), new XAttribute("property", "collection-type"), "series"));
                metadata.Add(new XElement(Opf + "meta", new XAttribute("name", "calibre:series"), new XAttribute("content", book.Series)));
                decimal sequenceNumber;
                if (Decimal.TryParse(book.SeriesNumber, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out sequenceNumber) && sequenceNumber >= 0)
                {
                    string value = sequenceNumber.ToString(CultureInfo.InvariantCulture);
                    metadata.Add(new XElement(Opf + "meta", new XAttribute("refines", "#series"), new XAttribute("property", "group-position"), value));
                    metadata.Add(new XElement(Opf + "meta", new XAttribute("name", "calibre:series_index"), new XAttribute("content", value)));
                }
            }
            XElement manifest = new XElement(Opf + "manifest",
                new XElement(Opf + "item", new XAttribute("id", "nav"), new XAttribute("href", "nav.xhtml"),
                    new XAttribute("media-type", "application/xhtml+xml"), new XAttribute("properties", "nav")),
                new XElement(Opf + "item", new XAttribute("id", "ncx"), new XAttribute("href", "toc.ncx"), new XAttribute("media-type", "application/x-dtbncx+xml")),
                new XElement(Opf + "item", new XAttribute("id", "css"), new XAttribute("href", "styles/style.css"), new XAttribute("media-type", "text/css")));
            XElement spine = new XElement(Opf + "spine", new XAttribute("toc", "ncx"));
            for (int i = 0; i < book.Chapters.Count; i++)
            {
                Chapter chapter = book.Chapters[i];
                string id = "chapter" + (i + 1).ToString(CultureInfo.InvariantCulture);
                manifest.Add(new XElement(Opf + "item", new XAttribute("id", id), new XAttribute("href", "text/" + chapter.File),
                    new XAttribute("media-type", "application/xhtml+xml")));
                spine.Add(new XElement(Opf + "itemref", new XAttribute("idref", id)));
            }
            int pictureNumber = 0;
            foreach (KeyValuePair<string, Picture> pair in book.Pictures)
            {
                string imageId = "image" + (++pictureNumber).ToString(CultureInfo.InvariantCulture);
                XElement item = new XElement(Opf + "item", new XAttribute("id", imageId),
                    new XAttribute("href", "images/" + pair.Value.File), new XAttribute("media-type", pair.Value.MediaType));
                if (pair.Key == book.CoverId)
                {
                    item.SetAttributeValue("properties", "cover-image");
                    metadata.Add(new XElement(Opf + "meta", new XAttribute("name", "cover"), new XAttribute("content", imageId)));
                }
                manifest.Add(item);
            }
            XElement package = new XElement(Opf + "package", new XAttribute("version", "3.0"), new XAttribute("unique-identifier", "bookid"),
                new XAttribute("prefix", "calibre: http://calibre.kovidgoyal.net/"), metadata, manifest, spine);
            Chapter coverChapter = book.Chapters.FirstOrDefault(c => c.Content.Attribute("class") != null && c.Content.Attribute("class").Value == "cover");
            if (coverChapter != null)
                package.Add(new XElement(Opf + "guide", new XElement(Opf + "reference", new XAttribute("type", "cover"),
                    new XAttribute("title", L10n.T("Обложка")), new XAttribute("href", "text/" + coverChapter.File))));
            return new XDocument(new XDeclaration("1.0", "utf-8", null), package);
        }

        private static XDocument ContainerDocument()
        {
            XNamespace container = "urn:oasis:names:tc:opendocument:xmlns:container";
            return new XDocument(new XDeclaration("1.0", "utf-8", null),
                new XElement(container + "container", new XAttribute("version", "1.0"),
                    new XElement(container + "rootfiles", new XElement(container + "rootfile",
                        new XAttribute("full-path", "OEBPS/content.opf"), new XAttribute("media-type", "application/oebps-package+xml")))));
        }

        private static byte[] XmlBytes(XDocument document)
        {
            using (MemoryStream output = new MemoryStream())
            {
                XmlWriterSettings settings = new XmlWriterSettings { Encoding = new UTF8Encoding(false), Indent = false,
                    CloseOutput = false, CheckCharacters = true };
                using (XmlWriter writer = XmlWriter.Create(output, settings)) document.Save(writer);
                return output.ToArray();
            }
        }

        private static XElement Child(XElement element, string name)
        {
            return element == null ? null : element.Elements().FirstOrDefault(e => e.Name.LocalName == name);
        }
        private static IEnumerable<XElement> Children(XElement element, string name)
        {
            return element == null ? Enumerable.Empty<XElement>() : element.Elements().Where(e => e.Name.LocalName == name);
        }
        private static string Attribute(XElement element, string name)
        {
            if (element == null) return String.Empty;
            XAttribute attribute = element.Attributes().FirstOrDefault(a => a.Name.LocalName == name && !a.IsNamespaceDeclaration);
            return attribute == null ? String.Empty : attribute.Value.Trim();
        }
        private static string Text(XElement element)
        {
            if (element == null) return String.Empty;
            if (element.Name.LocalName == "title")
            {
                List<XElement> paragraphs = Children(element, "p").ToList();
                if (paragraphs.Count > 0) return String.Join(" ", paragraphs.Select(Text).Where(s => s.Length != 0));
            }
            return String.Join(" ", element.Value.Split((char[])null, StringSplitOptions.RemoveEmptyEntries));
        }
        private static string Fragment(string href)
        {
            if (String.IsNullOrEmpty(href)) return String.Empty;
            string value = href[0] == '#' ? href.Substring(1) : href;
            try { return Uri.UnescapeDataString(value); } catch (UriFormatException) { return value; }
        }
        private static string Short(string value)
        {
            return value.Length <= 100 ? value : value.Substring(0, 100) + "…";
        }

        private const string Styles = @"
html { padding: 0; margin: 0; }
body { margin: 0 4%; line-height: 1.45; }
p { margin: 0.3em 0; text-indent: 1.3em; }
h1, h2, h3, h4, h5, h6 { text-align: center; font-weight: bold; margin: 1.2em 0 0.8em; text-indent: 0; page-break-after: avoid; }
h1 { font-size: 1.5em; } h2 { font-size: 1.3em; } h3 { font-size: 1.15em; }
.subtitle { text-align: center; font-size: 1.1em; }
blockquote { margin: 1em 1.5em; }
.epigraph { margin-left: 20%; font-style: italic; }
.text-author, .date { text-align: right; text-indent: 0; }
.poem { margin: 1em 5%; } .stanza { margin: 0.8em 0; }
.verse { margin-left: 1em; text-indent: -1em; }
.empty-line { height: 1em; }
img { max-width: 100%; height: auto; } .inline-image { max-height: 2em; vertical-align: middle; }
.cover { text-align: center; margin: 0; padding: 0; } .cover img { max-height: 95vh; }
table { border-collapse: collapse; max-width: 100%; margin: 1em auto; }
td, th { border: 1px solid; padding: 0.25em; } td p, th p { text-indent: 0; }
.align-left { text-align: left; } .align-right { text-align: right; }
.align-center { text-align: center; } .align-justify { text-align: justify; }
.backlinks { text-indent: 0; font-size: 0.85em; margin: 0.8em 0; }
.missing-image { font-style: italic; font-size: 0.9em; }
code { font-family: monospace; } del { text-decoration: line-through; }
nav ol { list-style-type: none; padding-left: 1em; } nav li { margin: 0.45em 0; }
";

        // ZIP32 writer. .NET Framework's ZipArchive may use DEFLATE even for
        // NoCompression; EPUB requires a genuinely STORED mimetype without extras.
        private sealed class EpubZip : IDisposable
        {
            private sealed class Entry
            {
                public byte[] Name;
                public uint Crc;
                public uint Size;
                public uint PackedSize;
                public uint Offset;
                public ushort Method;
            }
            private readonly BinaryWriter writer;
            private readonly List<Entry> entries = new List<Entry>();
            private bool disposed;
            private readonly ushort date;
            private readonly ushort time;
            private static readonly uint[] CrcTable = MakeCrcTable();

            public EpubZip(Stream output)
            {
                writer = new BinaryWriter(output, Encoding.UTF8, true);
                DateTime now = DateTime.Now;
                date = (ushort)(((Math.Max(1980, Math.Min(2107, now.Year)) - 1980) << 9) | (now.Month << 5) | now.Day);
                time = (ushort)((now.Hour << 11) | (now.Minute << 5) | (now.Second / 2));
            }

            public void Add(string name, byte[] bytes, bool compress)
            {
                if (entries.Count >= 65534) throw new InvalidDataException(L10n.T("В EPUB слишком много файлов."));
                byte[] packed = bytes;
                ushort method = 0;
                if (compress && bytes.Length != 0)
                {
                    using (MemoryStream buffer = new MemoryStream())
                    {
                        using (DeflateStream deflate = new DeflateStream(buffer, CompressionLevel.Optimal, true))
                            deflate.Write(bytes, 0, bytes.Length);
                        packed = buffer.ToArray();
                    }
                    method = 8;
                }
                if (writer.BaseStream.Position > UInt32.MaxValue)
                    throw new InvalidDataException(L10n.T("Готовая книга превышает допустимый размер EPUB."));
                Entry entry = new Entry { Name = Encoding.UTF8.GetBytes(name), Crc = Crc32(bytes), Size = (uint)bytes.Length,
                    PackedSize = (uint)packed.Length, Offset = (uint)writer.BaseStream.Position, Method = method };
                writer.Write(0x04034b50U); writer.Write((ushort)20); writer.Write((ushort)0x0800); writer.Write(method);
                writer.Write(time); writer.Write(date); writer.Write(entry.Crc); writer.Write(entry.PackedSize); writer.Write(entry.Size);
                writer.Write((ushort)entry.Name.Length); writer.Write((ushort)0); writer.Write(entry.Name); writer.Write(packed);
                entries.Add(entry);
            }

            public void Dispose()
            {
                if (disposed) return;
                disposed = true;
                uint start = (uint)writer.BaseStream.Position;
                foreach (Entry entry in entries)
                {
                    writer.Write(0x02014b50U); writer.Write((ushort)20); writer.Write((ushort)20); writer.Write((ushort)0x0800);
                    writer.Write(entry.Method); writer.Write(time); writer.Write(date); writer.Write(entry.Crc);
                    writer.Write(entry.PackedSize); writer.Write(entry.Size); writer.Write((ushort)entry.Name.Length);
                    writer.Write((ushort)0); writer.Write((ushort)0); writer.Write((ushort)0); writer.Write((ushort)0);
                    writer.Write(0U); writer.Write(entry.Offset); writer.Write(entry.Name);
                }
                uint size = (uint)writer.BaseStream.Position - start;
                writer.Write(0x06054b50U); writer.Write((ushort)0); writer.Write((ushort)0);
                writer.Write((ushort)entries.Count); writer.Write((ushort)entries.Count); writer.Write(size); writer.Write(start); writer.Write((ushort)0);
                writer.Flush(); writer.Dispose();
            }

            private static uint Crc32(byte[] bytes)
            {
                uint crc = 0xffffffffU;
                for (int i = 0; i < bytes.Length; i++)
                    crc = (crc >> 8) ^ CrcTable[(crc ^ bytes[i]) & 255];
                return ~crc;
            }

            private static uint[] MakeCrcTable()
            {
                uint[] table = new uint[256];
                for (uint i = 0; i < 256; i++)
                {
                    uint value = i;
                    for (int bit = 0; bit < 8; bit++) value = (value >> 1) ^ ((value & 1) != 0 ? 0xedb88320U : 0U);
                    table[i] = value;
                }
                return table;
            }
        }
    }
}
