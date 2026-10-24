using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using Fb2Kindle;

internal static class ConversionProgressTests
{
    static int passed;
    static readonly List<string> failures = new List<string>();
    static string directory;
    static string source;

    static void Require(bool condition, string message) { if (!condition) throw new Exception(message); }
    static void Test(string name, Action test)
    {
        try { test(); passed++; Console.WriteLine("PASS " + name); }
        catch (Exception error) { failures.Add(name + ": " + error.Message); Console.WriteLine("FAIL " + name + ": " + error); }
    }
    static string Output(string name) { return Path.Combine(directory, name + ".epub"); }
    static void NoPartials() { Require(Directory.GetFiles(directory, ".fb2kindle-*").Length == 0, "partial output remains"); }
    static void Cancelled(Action test)
    {
        try { test(); throw new Exception("Operation was not cancelled"); }
        catch (OperationCanceledException) { }
    }
    static void CheckProgress(List<ConversionProgress> values, bool finished)
    {
        Require(values.Count > 3, "not enough measured progress points");
        Require(values[0].Percent == 0, "progress must begin at zero");
        int previous = -1;
        foreach (ConversionProgress value in values)
        {
            Require(value.Percent >= previous && value.Percent >= 0 && value.Percent <= 100, "nonmonotonic or out-of-range progress");
            previous = value.Percent;
        }
        Require((values[values.Count - 1].Percent == 100) == finished, "incorrect completion state");
    }
    static string BookText()
    {
        string paragraph = String.Join(" ", Enumerable.Repeat("Текст книги: кириллица, <emphasis>важное</emphasis> слово и иллюстрации.", 1600));
        return "<?xml version=\"1.0\" encoding=\"utf-8\"?><FictionBook xmlns=\"http://www.gribuser.ru/xml/fictionbook/2.0\" xmlns:l=\"http://www.w3.org/1999/xlink\"><description><title-info><genre>sf</genre><author><first-name>Тестовый</first-name><last-name>Автор</last-name></author><book-title>Проверочная книга</book-title><lang>ru</lang><coverpage><image l:href=\"#cover\"/></coverpage></title-info></description><body><section id=\"chapter-one\"><title><p>Первая глава</p></title><p>Сноска<a l:href=\"#note-one\" type=\"note\">1</a>.</p><p>" + paragraph + "</p></section><section><title><p>Вторая глава</p></title><p>Конец книги.</p><image l:href=\"#cover\"/></section></body><body name=\"notes\"><section id=\"note-one\"><title><p>Примечание</p></title><p>Текст примечания.</p></section></body><binary id=\"cover\" content-type=\"image/png\">iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+a3ioAAAAASUVORK5CYII=</binary></FictionBook>";
    }
    public static int Main(string[] args)
    {
        directory = Path.Combine(args.Length > 0 ? Path.GetFullPath(args[0]) : Path.GetTempPath(), "conversion-progress-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        source = Path.Combine(directory, "Проверочная книга.fb2");
        File.WriteAllText(source, BookText(), new UTF8Encoding(false));
        byte[] original = File.ReadAllBytes(source);
        Test("EPUB preserves contents and reports ordered measured progress", delegate
        {
            string output = Output("complete");
            List<ConversionProgress> values = new List<ConversionProgress>();
            ConversionResult result = KindleConverter.Convert(source, output, CancellationToken.None, delegate(ConversionProgress value)
            {
                if (value.Percent == 100) Require(File.Exists(output), "100% arrived before atomic output move");
                values.Add(value);
            });
            CheckProgress(values, true);
            Require(values.Any(v => v.Percent > 0 && v.Percent < 15 && v.Stage == ConversionStage.Reading), "no byte-based read progress");
            Require(values.Count(v => v.Stage == ConversionStage.BuildingEpub && v.Percent > 65 && v.Percent < 98) >= 4, "no file-based package progress");
            Require(result.Title == "Проверочная книга" && result.Author == "Тестовый Автор" && result.ChapterCount == 2, "metadata mismatch");
            using (ZipArchive zip = ZipFile.OpenRead(output))
            {
                Require(zip.Entries[0].FullName == "mimetype", "mimetype is not first");
                Require(zip.GetEntry("OEBPS/nav.xhtml") != null && zip.GetEntry("OEBPS/toc.ncx") != null, "missing navigation");
                Require(zip.Entries.Any(e => e.FullName.StartsWith("OEBPS/images/")), "missing image");
                string texts = String.Join("\n", zip.Entries.Where(e => e.FullName.EndsWith("xhtml")).Select(e => { using (StreamReader r = new StreamReader(e.Open(), Encoding.UTF8)) return r.ReadToEnd(); }));
                Require(texts.Contains("Первая глава") && texts.Contains("Текст примечания") && texts.Contains("epub:type=\"noteref\""), "text or footnote lost");
                Require(texts.Contains("Конец книги"), "last chapter lost");
                foreach (ZipArchiveEntry entry in zip.Entries.Where(e => e.FullName.EndsWith("xhtml") || e.FullName.EndsWith("opf") || e.FullName.EndsWith("ncx"))) using (Stream stream = entry.Open()) XDocument.Load(stream);
            }
            byte[] header = File.ReadAllBytes(output);
            Require(header[8] == 0 && header[9] == 0 && header[28] == 0 && header[29] == 0, "EPUB mimetype must be STORED without ZIP extras");
            Require(original.SequenceEqual(File.ReadAllBytes(source)), "original book changed");
            NoPartials();
        });
        Test("Legacy overload remains usable", delegate { Fb2Converter.Convert(source, Output("legacy")); });
        Test("Existing output is preserved", delegate
        {
            string output = Output("existing"); File.WriteAllText(output, "keep");
            try { KindleConverter.Convert(source, output, CancellationToken.None); throw new Exception("overwrite was allowed"); } catch (IOException) { }
            Require(File.ReadAllText(output) == "keep", "existing output changed");
        });
        Test("Cancellation before conversion creates no file", delegate
        {
            CancellationTokenSource c = new CancellationTokenSource(); c.Cancel(); string output = Output("pre-cancel");
            Cancelled(delegate { KindleConverter.Convert(source, output, c.Token); }); Require(!File.Exists(output), "cancelled output exists"); NoPartials();
        });
        foreach (int threshold in new[] { 1, 70, 99 })
        {
            int at = threshold;
            Test("Cancellation at measured " + at + "% removes partial output", delegate
            {
                CancellationTokenSource c = new CancellationTokenSource(); string output = Output("cancel-" + at); List<ConversionProgress> values = new List<ConversionProgress>();
                Cancelled(delegate { KindleConverter.Convert(source, output, c.Token, delegate(ConversionProgress value) { values.Add(value); if (value.Percent >= at) c.Cancel(); }); });
                Require(!File.Exists(output), "cancelled output exists"); Require(!values.Any(v => v.Percent == 100), "cancelled conversion reported completion"); NoPartials();
            });
        }
        Test("Cancellation after completed move retains a valid book", delegate
        {
            CancellationTokenSource c = new CancellationTokenSource(); string output = Output("finished-race");
            KindleConverter.Convert(source, output, c.Token, delegate(ConversionProgress value) { if (value.Percent == 100) c.Cancel(); });
            using (ZipArchive zip = ZipFile.OpenRead(output)) Require(zip.GetEntry("OEBPS/content.opf") != null, "completed book invalid");
        });
        string archive = Path.Combine(directory, "books.zip");
        using (ZipArchive zip = ZipFile.Open(archive, ZipArchiveMode.Create)) { zip.CreateEntryFromFile(source, "nested/one.fb2"); zip.CreateEntryFromFile(source, "two.fb2"); }
        Test("Archive entry reports measured extraction followed by monotonic conversion", delegate
        {
            List<ArchiveBook> books = ArchiveBooks.List(archive); Require(books.Count == 2, "archive listing lost books");
            List<ConversionProgress> values = new List<ConversionProgress>(); string output = Output("archive");
            KindleConverter.Convert(archive, books[0], output, CancellationToken.None, delegate(ConversionProgress value) { values.Add(value); if (value.Percent == 100) Require(File.Exists(output), "archive completion arrived too early"); });
            CheckProgress(values, true); Require(values.Any(v => v.Stage == ConversionStage.Extracting && v.Percent > 0 && v.Percent < 12), "no extraction byte progress");
            NoPartials();
        });
        Test("Cancellation during archive extraction removes all partial outputs", delegate
        {
            CancellationTokenSource c = new CancellationTokenSource(); string output = Output("archive-cancel");
            Cancelled(delegate { KindleConverter.Convert(archive, ArchiveBooks.List(archive)[0], output, c.Token, delegate(ConversionProgress value) { if (value.Stage == ConversionStage.Extracting && value.Percent > 0) c.Cancel(); }); });
            Require(!File.Exists(output), "cancelled archive output exists"); NoPartials();
        });
        Test("Multi-book archive CLI still rejects ambiguity", delegate
        {
            try { KindleConverter.Convert(archive, Output("ambiguous"), CancellationToken.None); throw new Exception("ambiguous CLI archive accepted"); } catch (InvalidDataException) { }
        });
        Test("Real Calibre percent parser ignores unrelated log numbers", delegate
        {
            int percent;
            Require(KindleConverter.TryCalibrePercent("  47% Running AZW3 Output plugin", out percent) && percent == 47, "integer engine progress not read");
            Require(KindleConverter.TryCalibrePercent("72.5% Building records", out percent) && percent == 72, "fractional engine progress not read");
            Require(KindleConverter.TryCalibrePercent("100% Output saved", out percent) && percent == 100, "engine completion not read");
            Require(!KindleConverter.TryCalibrePercent("Chapter 50% in book title", out percent), "embedded percentage was accepted");
            Require(!KindleConverter.TryCalibrePercent("101% invalid", out percent), "out-of-range percentage was accepted");
        });
        Test("Malformed XML never reports success", delegate
        {
            string bad = Path.Combine(directory, "bad.fb2"); File.WriteAllText(bad, "<FictionBook><body>"); string output = Output("bad"); List<ConversionProgress> values = new List<ConversionProgress>();
            try { KindleConverter.Convert(bad, output, CancellationToken.None, values.Add); throw new Exception("malformed XML accepted"); } catch (InvalidDataException) { }
            Require(!File.Exists(output) && !values.Any(v => v.Percent == 100), "bad book reported success"); NoPartials();
        });
        Console.WriteLine("RESULT " + passed + " passed, " + failures.Count + " failed");
        Console.WriteLine("ARTIFACTS " + directory);
        return failures.Count == 0 ? 0 : 1;
    }
}
