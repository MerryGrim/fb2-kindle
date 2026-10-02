using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Fb2Kindle
{
    public sealed class ArchiveBook
    {
        public int Index;
        public string EntryName;
        public long Size;
        public bool Encrypted;
        internal long ArchiveLength;
        internal long ArchiveWriteTicks;
        internal uint Crc;
        internal bool Ambiguous;
        internal bool External;
    }

    // Entry names are labels only: all extraction goes to the caller's CreateNew
    // temporary file. Even an entry named ../../book.fb2 cannot choose a disk path.
    public static class ArchiveBooks
    {
        private const long MaxBookBytes = 96L * 1024 * 1024;
        private const int MaxEntries = 10000;
        private const int MaxListingBytes = 8 * 1024 * 1024;
        private static readonly uint[] CrcTable = CreateCrcTable();

        private sealed class Problem : Exception
        {
            public Problem(string message) : base(message) { }
        }

        private sealed class ZipInfo
        {
            public uint Crc;
            public ushort Flags;
        }

        public static List<ArchiveBook> List(string archivePath)
        {
            return List(archivePath, CancellationToken.None);
        }

        public static List<ArchiveBook> List(string archivePath, CancellationToken cancellation)
        {
            archivePath = CheckPath(archivePath);
            cancellation.ThrowIfCancellationRequested();
            try
            {
                if (IsZip(archivePath))
                {
                    try { return ListZip(archivePath, cancellation); }
                    // .NET Framework's ZIP reader applies Windows path rules to
                    // stored names. The streaming engine can safely read names
                    // such as quotes or colons without creating those disk paths.
                    catch (ArgumentException) { return ListExternal(archivePath, cancellation); }
                }
                return ListExternal(archivePath, cancellation);
            }
            catch (Problem exception) { throw new InvalidDataException(exception.Message, exception); }
            catch (InvalidDataException exception) { throw Corrupt(exception); }
            catch (NotSupportedException exception) { throw Corrupt(exception); }
        }

        public static void Extract(string archivePath, ArchiveBook book, string destination, CancellationToken cancellation)
        {
            archivePath = CheckPath(archivePath);
            if (book == null) throw new ArgumentNullException("book");
            if (String.IsNullOrWhiteSpace(destination)) throw new ArgumentException(L10n.T("Не выбран временный файл для книги."));
            destination = Path.GetFullPath(destination);
            if (String.Equals(archivePath, destination, StringComparison.OrdinalIgnoreCase))
                throw new IOException(L10n.T("Архив и временный файл должны иметь разные имена."));
            cancellation.ThrowIfCancellationRequested();
            bool created = false;
            bool finished = false;
            try
            {
                CheckFingerprint(archivePath, book);
                if (IsZip(archivePath) && !book.External)
                {
                    using (FileStream input = new FileStream(archivePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    using (ZipArchive zip = new ZipArchive(input, ZipArchiveMode.Read, true, Encoding.GetEncoding(866)))
                    {
                        List<ZipInfo> metadata = ReadZipInfo(input, cancellation);
                        if (zip.Entries.Count != metadata.Count || book.Index < 0 || book.Index >= zip.Entries.Count)
                            throw new Problem(L10n.T("Содержимое архива изменилось. Удалите его из списка и добавьте заново."));
                        ZipArchiveEntry entry = zip.Entries[book.Index];
                        ValidateBook(book, entry.FullName, entry.Length);
                        ZipInfo info = metadata[book.Index];
                        if (info.Crc != book.Crc) throw new Problem(L10n.T("Содержимое архива изменилось. Добавьте архив заново."));
                        CheckBook(entry.Length, (info.Flags & 1) != 0);
                        using (Stream source = entry.Open())
                        using (FileStream output = new FileStream(destination, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                        {
                            created = true;
                            uint crc;
                            long copied = CopyBounded(source, output, cancellation, out crc);
                            if (copied != entry.Length || crc != info.Crc)
                                throw new Problem(L10n.T("Книга в ZIP-архиве повреждена: проверка целостности не пройдена."));
                        }
                    }
                }
                else
                {
                    List<ArchiveBook> current = ListExternal(archivePath, cancellation);
                    ArchiveBook selected = current.Find(delegate(ArchiveBook candidate) { return candidate.Index == book.Index; });
                    if (selected == null) throw new Problem(L10n.T("Содержимое архива изменилось. Добавьте архив заново."));
                    ValidateBook(book, selected.EntryName, selected.Size);
                    if (selected.EntryName.IndexOfAny(new char[] { '\r', '\n', '\0' }) >= 0)
                        throw new Problem(L10n.T("В архиве есть неоднозначное имя файла. Распакуйте эту книгу вручную."));
                    CheckBook(selected.Size, selected.Encrypted);
                    if (selected.Ambiguous)
                        throw new Problem(L10n.T("В архиве несколько книг с одинаковым путём. Распакуйте их и добавьте нужную книгу отдельно."));
                    string selector = Path.Combine(Path.GetDirectoryName(destination), ".fb2kindle-selector-" + Guid.NewGuid().ToString("N") + ".txt");
                    bool selectorCreated = false;
                    try
                    {
                        using (FileStream selection = new FileStream(selector, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                        {
                            selectorCreated = true;
                            using (StreamWriter writer = new StreamWriter(selection, new UTF8Encoding(false))) writer.WriteLine(selected.EntryName);
                        }
                        using (FileStream output = new FileStream(destination, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                        {
                            created = true;
                            long copied = 0;
                            // 7-Zip has its own argument parser, which removes
                            // literal quotes from filenames. A UTF-8 include list
                            // preserves the exact stored name, including quotes.
                            RunExternal(new string[] { "x", "-so", "-spd", "-ssc", "-r-", "-y", "-bd", "-sccUTF-8", "-scsUTF-8", "-i@" + selector, "--", archivePath },
                                delegate(Stream source)
                                {
                                    uint ignored;
                                    copied = CopyBounded(source, output, cancellation, out ignored);
                                }, cancellation, 300000);
                            if (copied != selected.Size)
                                throw new Problem(L10n.T("Книга в архиве повреждена: распакован неверный объём данных."));
                        }
                    }
                    finally
                    {
                        if (selectorCreated)
                        {
                            try { File.Delete(selector); } catch (IOException) { } catch (UnauthorizedAccessException) { }
                        }
                    }
                }
                cancellation.ThrowIfCancellationRequested();
                CheckFingerprint(archivePath, book);
                finished = true;
            }
            catch (Problem exception) { throw new InvalidDataException(exception.Message, exception); }
            catch (InvalidDataException exception) { throw Corrupt(exception); }
            catch (NotSupportedException exception) { throw Corrupt(exception); }
            finally
            {
                if (created && !finished)
                {
                    try { File.Delete(destination); } catch (IOException) { } catch (UnauthorizedAccessException) { }
                }
            }
        }

        private static string CheckPath(string path)
        {
            if (String.IsNullOrWhiteSpace(path)) throw new ArgumentException(L10n.T("Не выбран архив с книгами."));
            path = Path.GetFullPath(path);
            if (!File.Exists(path)) throw new FileNotFoundException(L10n.T("Архив с книгами не найден."), path);
            string extension = Path.GetExtension(path);
            if (!extension.Equals(".zip", StringComparison.OrdinalIgnoreCase) &&
                !extension.Equals(".rar", StringComparison.OrdinalIgnoreCase) &&
                !extension.Equals(".7z", StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException(L10n.T("Выберите архив ZIP, RAR или 7Z с книгами FB2."));
            return path;
        }

        private static bool IsZip(string path) { return Path.GetExtension(path).Equals(".zip", StringComparison.OrdinalIgnoreCase); }

        private static List<ArchiveBook> ListZip(string path, CancellationToken cancellation)
        {
            using (FileStream input = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (ZipArchive zip = new ZipArchive(input, ZipArchiveMode.Read, true, Encoding.GetEncoding(866)))
            {
                List<ZipInfo> metadata = ReadZipInfo(input, cancellation);
                if (zip.Entries.Count != metadata.Count) throw new Problem(L10n.T("ZIP-архив повреждён: неверный список файлов."));
                List<ArchiveBook> books = new List<ArchiveBook>();
                FileInfo fingerprint = new FileInfo(path);
                for (int index = 0; index < zip.Entries.Count; index++)
                {
                    cancellation.ThrowIfCancellationRequested();
                    ZipArchiveEntry entry = zip.Entries[index];
                    if (IsBook(entry.FullName, entry.FullName.EndsWith("/", StringComparison.Ordinal) || entry.FullName.EndsWith("\\", StringComparison.Ordinal)))
                        books.Add(new ArchiveBook { Index = index, EntryName = entry.FullName, Size = entry.Length,
                            Encrypted = (metadata[index].Flags & 1) != 0, Crc = metadata[index].Crc,
                            ArchiveLength = fingerprint.Length, ArchiveWriteTicks = fingerprint.LastWriteTimeUtc.Ticks });
                }
                if (books.Count == 0) throw new Problem(L10n.T("В архиве не найдено книг FB2."));
                return books;
            }
        }

        private static bool IsBook(string name, bool directory)
        {
            if (directory || String.IsNullOrEmpty(name) || !name.EndsWith(".fb2", StringComparison.OrdinalIgnoreCase)) return false;
            string[] parts = name.Replace('\\', '/').Split('/');
            foreach (string part in parts)
                if (part.Equals("__MACOSX", StringComparison.OrdinalIgnoreCase)) return false;
            return !parts[parts.Length - 1].StartsWith("._", StringComparison.Ordinal);
        }

        private static void CheckBook(long size, bool encrypted)
        {
            if (encrypted) throw new Problem(L10n.T("Книга в архиве защищена паролем. Распакуйте её с паролем и добавьте файл FB2."));
            if (size < 0) throw new Problem(L10n.T("В архиве указан неверный размер книги."));
            if (size > MaxBookBytes) throw new Problem(L10n.T("Книга в архиве слишком большая: предел для FB2 — 96 МБ."));
        }

        private static void ValidateBook(ArchiveBook book, string name, long size)
        {
            if (!String.Equals(book.EntryName, name, StringComparison.Ordinal) || book.Size != size)
                throw new Problem(L10n.T("Содержимое архива изменилось. Удалите его из списка и добавьте заново."));
        }

        private static void CheckFingerprint(string path, ArchiveBook book)
        {
            FileInfo info = new FileInfo(path);
            if (book.ArchiveLength != info.Length || book.ArchiveWriteTicks != info.LastWriteTimeUtc.Ticks)
                throw new Problem(L10n.T("Архив изменился после добавления. Удалите его из списка и добавьте заново."));
        }

        private static InvalidDataException Corrupt(Exception inner)
        {
            return new InvalidDataException(L10n.T("Не удалось прочитать архив. Проверьте, что он не повреждён и не защищён паролем."), inner);
        }

        private static long CopyBounded(Stream source, Stream destination, CancellationToken cancellation, out uint resultCrc)
        {
            byte[] buffer = new byte[65536];
            long total = 0;
            uint crc = 0xffffffffU;
            while (true)
            {
                cancellation.ThrowIfCancellationRequested();
                int count = source.Read(buffer, 0, buffer.Length);
                if (count == 0) break;
                if (total + count > MaxBookBytes) throw new Problem(L10n.T("Книга в архиве слишком большая: предел для FB2 — 96 МБ."));
                for (int index = 0; index < count; index++) crc = CrcTable[(crc ^ buffer[index]) & 255] ^ (crc >> 8);
                destination.Write(buffer, 0, count);
                total += count;
            }
            resultCrc = crc ^ 0xffffffffU;
            return total;
        }

        private static uint[] CreateCrcTable()
        {
            uint[] table = new uint[256];
            for (uint index = 0; index < table.Length; index++)
            {
                uint value = index;
                for (int bit = 0; bit < 8; bit++) value = (value & 1) != 0 ? 0xedb88320U ^ (value >> 1) : value >> 1;
                table[index] = value;
            }
            return table;
        }

        // ZipArchive on .NET Framework does not verify CRC on reads. Read the
        // central directory without relying on private framework implementation.
        private static List<ZipInfo> ReadZipInfo(FileStream input, CancellationToken cancellation)
        {
            int length = (int)Math.Min(input.Length, 65557L);
            byte[] tail = new byte[length];
            input.Position = input.Length - length;
            ReadExactly(input, tail, 0, length);
            int eocd = -1;
            for (int index = length - 22; index >= 0; index--)
                if (U32(tail, index) == 0x06054b50U && index + 22 + U16(tail, index + 20) == length) { eocd = index; break; }
            if (eocd < 0) throw new Problem(L10n.T("Это не ZIP-архив или архив повреждён."));
            if (U16(tail, eocd + 4) != 0 || U16(tail, eocd + 6) != 0 || U16(tail, eocd + 8) != U16(tail, eocd + 10))
                throw new Problem(L10n.T("Многотомный ZIP-архив не поддерживается. Соберите его в один архив."));
            long count = U16(tail, eocd + 10);
            long directoryOffset = U32(tail, eocd + 16);
            long directorySize = U32(tail, eocd + 12);
            long eocdOffset = input.Length - length + eocd;
            if (count == 65535 || directoryOffset == uint.MaxValue || directorySize == uint.MaxValue)
            {
                if (eocdOffset < 20) throw new Problem(L10n.T("ZIP64-архив повреждён."));
                byte[] locator = new byte[20];
                input.Position = eocdOffset - 20;
                ReadExactly(input, locator, 0, locator.Length);
                if (U32(locator, 0) != 0x07064b50U || U32(locator, 4) != 0 || U32(locator, 16) != 1)
                    throw new Problem(L10n.T("ZIP64-архив повреждён или состоит из нескольких томов."));
                long recordOffset = U64(locator, 8);
                if (recordOffset < 0 || recordOffset > input.Length - 56) throw new Problem(L10n.T("ZIP64-архив повреждён."));
                byte[] record = new byte[56];
                input.Position = recordOffset;
                ReadExactly(input, record, 0, record.Length);
                if (U32(record, 0) != 0x06064b50U || U32(record, 16) != 0 || U32(record, 20) != 0 || U64(record, 24) != U64(record, 32))
                    throw new Problem(L10n.T("ZIP64-архив повреждён или состоит из нескольких томов."));
                count = U64(record, 32);
                directorySize = U64(record, 40);
                directoryOffset = U64(record, 48);
            }
            if (count < 0 || count > MaxEntries) throw new Problem(L10n.T("В архиве слишком много файлов: предел — 10 000."));
            if (directoryOffset < 0 || directorySize < 0 || directoryOffset > input.Length || directorySize > input.Length - directoryOffset)
                throw new Problem(L10n.T("ZIP-архив повреждён: неверная структура каталога."));
            input.Position = directoryOffset;
            long directoryEnd = directoryOffset + directorySize;
            List<ZipInfo> result = new List<ZipInfo>((int)count);
            byte[] header = new byte[46];
            for (int index = 0; index < count; index++)
            {
                cancellation.ThrowIfCancellationRequested();
                if (input.Position > directoryEnd - header.Length) throw new Problem(L10n.T("ZIP-архив повреждён: каталог обрезан."));
                ReadExactly(input, header, 0, header.Length);
                if (U32(header, 0) != 0x02014b50U) throw new Problem(L10n.T("ZIP-архив повреждён: неверная запись каталога."));
                long extra = (long)U16(header, 28) + U16(header, 30) + U16(header, 32);
                if (extra > directoryEnd - input.Position) throw new Problem(L10n.T("ZIP-архив повреждён: каталог обрезан."));
                result.Add(new ZipInfo { Flags = U16(header, 8), Crc = U32(header, 16) });
                input.Position += extra;
            }
            return result;
        }

        private static void ReadExactly(Stream input, byte[] bytes, int offset, int count)
        {
            while (count > 0)
            {
                int read = input.Read(bytes, offset, count);
                if (read == 0) throw new Problem(L10n.T("Архив повреждён: неожиданный конец файла."));
                offset += read;
                count -= read;
            }
        }

        private static ushort U16(byte[] bytes, int offset) { return (ushort)(bytes[offset] | bytes[offset + 1] << 8); }
        private static uint U32(byte[] bytes, int offset) { return (uint)(bytes[offset] | bytes[offset + 1] << 8 | bytes[offset + 2] << 16 | bytes[offset + 3] << 24); }
        private static long U64(byte[] bytes, int offset) { return unchecked((long)((ulong)U32(bytes, offset) | (ulong)U32(bytes, offset + 4) << 32)); }

        private static List<ArchiveBook> ListExternal(string path, CancellationToken cancellation)
        {
            byte[] listing;
            using (MemoryStream captured = new MemoryStream())
            {
                RunExternal(new string[] { "l", "-slt", "-ba", "-sccUTF-8", "--", path }, delegate(Stream source)
                {
                    byte[] buffer = new byte[32768];
                    int count;
                    while ((count = source.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        cancellation.ThrowIfCancellationRequested();
                        if (captured.Length + count > MaxListingBytes) throw new Problem(L10n.T("Каталог архива слишком большой."));
                        captured.Write(buffer, 0, count);
                    }
                }, cancellation, 60000);
                listing = captured.ToArray();
            }
            FileInfo fingerprint = new FileInfo(path);
            List<ArchiveBook> books = new List<ArchiveBook>();
            Dictionary<string, string> fields = new Dictionary<string, string>(StringComparer.Ordinal);
            int index = 0;
            using (StringReader reader = new StringReader(new UTF8Encoding(false, true).GetString(listing)))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Length == 0)
                    {
                        AddExternalBook(fields, books, fingerprint, index);
                        if (fields.Count > 0 && ++index > MaxEntries) throw new Problem(L10n.T("В архиве слишком много файлов: предел — 10 000."));
                        fields.Clear();
                        continue;
                    }
                    int separator = line.IndexOf(" = ", StringComparison.Ordinal);
                    if (separator <= 0) throw new Problem(L10n.T("В архиве есть имя файла, которое невозможно прочитать. Распакуйте архив вручную."));
                    string key = line.Substring(0, separator);
                    if (fields.ContainsKey(key)) throw new Problem(L10n.T("В архиве есть неоднозначное имя файла. Распакуйте архив вручную."));
                    fields.Add(key, line.Substring(separator + 3));
                }
                AddExternalBook(fields, books, fingerprint, index);
                if (fields.Count > 0 && ++index > MaxEntries) throw new Problem(L10n.T("В архиве слишком много файлов: предел — 10 000."));
            }
            if (books.Count == 0) throw new Problem(L10n.T("В архиве не найдено книг FB2."));
            Dictionary<string, int> paths = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (ArchiveBook book in books)
            {
                string normalized = book.EntryName.Replace('\\', '/');
                int count;
                paths.TryGetValue(normalized, out count);
                paths[normalized] = count + 1;
            }
            foreach (ArchiveBook book in books) book.Ambiguous = paths[book.EntryName.Replace('\\', '/')] > 1;
            return books;
        }

        private static void AddExternalBook(Dictionary<string, string> fields, List<ArchiveBook> books, FileInfo fingerprint, int index)
        {
            if (fields.Count == 0) return;
            string name;
            if (!fields.TryGetValue("Path", out name)) throw new Problem(L10n.T("Не удалось прочитать список файлов архива."));
            string folder;
            string attributes;
            bool directory = fields.TryGetValue("Folder", out folder) && folder == "+";
            if (fields.TryGetValue("Attributes", out attributes) && attributes.StartsWith("D", StringComparison.OrdinalIgnoreCase)) directory = true;
            if (!IsBook(name, directory)) return;
            string sizeText;
            long size;
            if (!fields.TryGetValue("Size", out sizeText) || !Int64.TryParse(sizeText, NumberStyles.None, CultureInfo.InvariantCulture, out size) || size < 0)
                throw new Problem(L10n.T("В архиве указан неверный размер книги."));
            string encrypted;
            books.Add(new ArchiveBook { Index = index, EntryName = name, Size = size,
                Encrypted = fields.TryGetValue("Encrypted", out encrypted) && encrypted == "+",
                External = true, ArchiveLength = fingerprint.Length, ArchiveWriteTicks = fingerprint.LastWriteTimeUtc.Ticks });
        }

        private static string EnginePath()
        {
            string bundled = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "engine", "Archive7z", "7z.exe");
            if (File.Exists(bundled)) return bundled;
            string installed = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "7-Zip", "7z.exe");
            if (File.Exists(installed)) return installed;
            throw new Problem(L10n.T("Не найден модуль чтения RAR и 7Z. Распакуйте программу целиком вместе с папкой engine."));
        }

        private static void RunExternal(string[] arguments, Action<Stream> readOutput, CancellationToken cancellation, int timeoutMilliseconds)
        {
            cancellation.ThrowIfCancellationRequested();
            StringBuilder command = new StringBuilder();
            foreach (string argument in arguments) { if (command.Length != 0) command.Append(' '); command.Append(Quote(argument)); }
            ProcessStartInfo start = new ProcessStartInfo(EnginePath(), command.ToString());
            start.UseShellExecute = false;
            start.CreateNoWindow = true;
            start.RedirectStandardOutput = true;
            start.RedirectStandardError = true;
            start.RedirectStandardInput = true;
            start.StandardErrorEncoding = Encoding.UTF8;
            using (Process process = new Process())
            {
                process.StartInfo = start;
                try
                {
                    if (!process.Start()) throw new Problem(L10n.T("Не удалось запустить модуль чтения архива."));
                }
                catch (System.ComponentModel.Win32Exception)
                {
                    throw new Problem(L10n.T("Не удалось запустить модуль чтения RAR и 7Z. Распакуйте программу целиком вместе с папкой engine."));
                }
                process.StandardInput.Close();
                Task output = Task.Factory.StartNew(delegate { readOutput(process.StandardOutput.BaseStream); }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
                Task<string> errors = Task.Factory.StartNew(delegate
                {
                    StringBuilder text = new StringBuilder();
                    char[] buffer = new char[4096];
                    int count;
                    while ((count = process.StandardError.Read(buffer, 0, buffer.Length)) != 0)
                        if (text.Length < 262144) text.Append(buffer, 0, Math.Min(count, 262144 - text.Length));
                    return text.ToString();
                }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
                Stopwatch timer = Stopwatch.StartNew();
                try
                {
                    while (!process.WaitForExit(30))
                    {
                        cancellation.ThrowIfCancellationRequested();
                        if (output.IsFaulted) throw output.Exception.InnerException;
                        if (timer.ElapsedMilliseconds > timeoutMilliseconds) throw new Problem(L10n.T("Чтение архива заняло слишком много времени. Распакуйте архив вручную."));
                    }
                    cancellation.ThrowIfCancellationRequested();
                    try { output.Wait(); errors.Wait(); }
                    catch (AggregateException exception) { throw exception.Flatten().InnerExceptions[0]; }
                    if (process.ExitCode != 0)
                    {
                        string error = errors.Result;
                        if (error.IndexOf("password", StringComparison.OrdinalIgnoreCase) >= 0 || error.IndexOf("encrypted", StringComparison.OrdinalIgnoreCase) >= 0)
                            throw new Problem(L10n.T("Архив защищён паролем. Распакуйте его с паролем и добавьте файлы FB2."));
                        throw new Problem(L10n.T("Не удалось прочитать архив. Он повреждён, имеет неподдерживаемый формат или состоит из нескольких томов."));
                    }
                }
                finally
                {
                    try { if (!process.HasExited) process.Kill(); } catch (InvalidOperationException) { } catch (System.ComponentModel.Win32Exception) { }
                    // Ensure the output worker no longer owns the destination stream.
                    try { Task.WaitAll(new Task[] { output, errors }, 10000); } catch (AggregateException) { }
                }
            }
        }

        private static string Quote(string value)
        {
            StringBuilder quoted = new StringBuilder("\"");
            int slashes = 0;
            foreach (char character in value)
            {
                if (character == '\\') { slashes++; continue; }
                if (character == '"') quoted.Append('\\', slashes * 2 + 1);
                else quoted.Append('\\', slashes);
                quoted.Append(character);
                slashes = 0;
            }
            quoted.Append('\\', slashes * 2);
            quoted.Append('"');
            return quoted.ToString();
        }
    }
}
