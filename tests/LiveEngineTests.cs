using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Fb2Kindle;

internal static class LiveEngineTests
{
    static string root, artifacts, sample, calibre, sevenZip;
    static int passed;
    static readonly List<string> failures = new List<string>();
    static readonly string[] processNames = { "ebook-convert", "calibre-parallel", "calibre-worker" };

    static void Require(bool condition, string message) { if (!condition) throw new Exception(message); }
    static void Test(string name, Action body)
    {
        Stopwatch timer = Stopwatch.StartNew();
        try { body(); passed++; Console.WriteLine("PASS " + name + " (" + timer.ElapsedMilliseconds + " ms)"); }
        catch (Exception error) { failures.Add(name + ": " + error.Message); Console.WriteLine("FAIL " + name + "\n" + error); }
    }
    static string Quote(string value) { return "\"" + value + "\""; }
    static void Run(string executable, string arguments, string name)
    {
        ProcessStartInfo info = new ProcessStartInfo(executable, arguments);
        info.UseShellExecute = false; info.CreateNoWindow = true; info.RedirectStandardOutput = true; info.RedirectStandardError = true;
        info.StandardOutputEncoding = Encoding.UTF8; info.StandardErrorEncoding = Encoding.UTF8;
        info.WorkingDirectory = artifacts;
        info.EnvironmentVariables["CALIBRE_CONFIG_DIRECTORY"] = Path.Combine(artifacts, "calibre-config");
        info.EnvironmentVariables["CALIBRE_CACHE_DIRECTORY"] = Path.Combine(artifacts, "calibre-cache");
        info.EnvironmentVariables["CALIBRE_TEMP_DIR"] = Path.Combine(artifacts, "calibre-temp");
        Directory.CreateDirectory(info.EnvironmentVariables["CALIBRE_TEMP_DIR"]);
        using (Process p = new Process())
        {
            p.StartInfo = info; p.Start();
            Task<string> stdout = Task.Factory.StartNew(delegate { return p.StandardOutput.ReadToEnd(); });
            Task<string> stderr = Task.Factory.StartNew(delegate { return p.StandardError.ReadToEnd(); });
            if (!p.WaitForExit(180000)) { p.Kill(); throw new Exception("External tool timed out"); }
            Task.WaitAll(stdout, stderr);
            File.WriteAllText(Path.Combine(artifacts, name + ".log"), stdout.Result + "\nSTDERR\n" + stderr.Result, new UTF8Encoding(false));
            Require(p.ExitCode == 0, name + " failed, exit " + p.ExitCode);
        }
    }
    static List<int> EngineIds()
    {
        List<int> ids = new List<int>();
        foreach (string name in processNames)
            foreach (Process process in Process.GetProcessesByName(name)) { using (process) ids.Add(process.Id); }
        return ids;
    }
    static void VerifyProgress(List<ConversionProgress> values, bool complete)
    {
        Require(values.Count > 5, "too few progress points");
        int previous = -1;
        foreach (ConversionProgress value in values) { Require(value.Percent >= previous && value.Percent <= 100, "nonmonotonic progress"); previous = value.Percent; }
        Require(values[0].Percent == 0, "progress must start at zero");
        Require(values[values.Count - 1].Percent == 100 == complete, "incorrect completion state");
    }
    static ConversionResult Convert(string input, ArchiveBook entry, string output, string logName)
    {
        List<ConversionProgress> values = new List<ConversionProgress>();
        Stopwatch timer = Stopwatch.StartNew();
        List<string> events = new List<string> { "milliseconds,percent,stage,finalFileExists" };
        ConversionResult result = KindleConverter.Convert(input, entry, output, CancellationToken.None, delegate(ConversionProgress value)
        {
            lock (values)
            {
                values.Add(value);
                events.Add(timer.ElapsedMilliseconds + "," + value.Percent + "," + value.Stage + "," + File.Exists(output));
                if (value.Percent == 100) Require(File.Exists(output), "100% before output committed");
            }
        });
        File.WriteAllLines(Path.Combine(artifacts, logName + "-progress.csv"), events, new UTF8Encoding(false));
        VerifyProgress(values, true);
        if (output.EndsWith(".azw3", StringComparison.OrdinalIgnoreCase))
        {
            int[] enginePercentages = values.Where(v => v.Stage == ConversionStage.ConvertingAzw3).Select(v => v.Percent).Distinct().ToArray();
            Require(enginePercentages.Length >= 3 && enginePercentages.Any(p => p > 35 && p < 98), "missing real intermediate Calibre progress");
        }
        Require(!Directory.GetFiles(artifacts, ".fb2kindle-*").Any(), "pending output remains");
        return result;
    }
    static void VerifyEpub(string path)
    {
        using (ZipArchive zip = ZipFile.OpenRead(path))
        {
            Require(zip.Entries.Any(e => e.FullName.EndsWith(".opf")), "missing package metadata");
            Require(zip.Entries.Any(e => e.FullName.EndsWith(".ncx") || e.FullName.EndsWith("nav.xhtml")), "missing TOC");
            Require(zip.Entries.Any(e => e.FullName.EndsWith(".png") || e.FullName.EndsWith(".jpeg") || e.FullName.EndsWith(".jpg")), "missing images");
            List<XDocument> docs = new List<XDocument>();
            foreach (ZipArchiveEntry entry in zip.Entries.Where(e => e.FullName.EndsWith(".xhtml") || e.FullName.EndsWith(".html"))) using (Stream s = entry.Open()) docs.Add(XDocument.Load(s));
            string text = String.Join("\n", docs.Select(d => d.Root.Value));
            Require(text.Contains("ALPHA_BODY_MARKER") && text.Contains("OMEGA_BODY_MARKER"), "book text not preserved");
            Require(text.Contains("Кириллица"), "Cyrillic body text not preserved");
            ZipArchiveEntry package = zip.Entries.First(e => e.FullName.EndsWith(".opf"));
            using (Stream packageStream = package.Open()) Require(XDocument.Load(packageStream).Descendants().Any(e => e.Name.LocalName == "title" && e.Value.IndexOf("проверочная книга", StringComparison.OrdinalIgnoreCase) >= 0), "Cyrillic book title not preserved in package metadata");
            Require(text.Contains("FOOTNOTE_MARKER"), "footnote text not preserved");
            Require(docs.SelectMany(d => d.Descendants()).Any(e => e.Name.LocalName == "a" && e.Attributes().Any(a => a.Name.LocalName == "href" && a.Value.Contains("#"))), "footnote/internal links lost");
            Require(docs.SelectMany(d => d.Descendants()).Any(e => e.Name.LocalName == "img"), "image element lost");
        }
    }
    static string Sample()
    {
        string paragraph = String.Join(" ", Enumerable.Repeat("Кириллица и <emphasis>курсив</emphasis>, <strong>выделение</strong> в проверочном тексте.", 1200));
        return "<?xml version=\"1.0\" encoding=\"utf-8\"?><FictionBook xmlns=\"http://www.gribuser.ru/xml/fictionbook/2.0\" xmlns:l=\"http://www.w3.org/1999/xlink\"><description><title-info><genre>sf</genre><author><first-name>Test</first-name><last-name>Author</last-name></author><book-title>Проверочная книга</book-title><lang>ru</lang><coverpage><image l:href=\"#cover\"/></coverpage></title-info></description><body><section id=\"one\"><title><p>Первая глава</p></title><p>ALPHA_BODY_MARKER Сноска<a l:href=\"#note1\" type=\"note\">1</a>.</p><p>" + paragraph + "</p></section><section><title><p>Вторая глава</p></title><p>OMEGA_BODY_MARKER</p><image l:href=\"#cover\"/></section></body><body name=\"notes\"><section id=\"note1\"><title><p>Примечание 1</p></title><p>FOOTNOTE_MARKER Проверочное примечание.</p></section></body><binary id=\"cover\" content-type=\"image/png\">iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+a3ioAAAAASUVORK5CYII=</binary></FictionBook>";
    }
    static uint Crc32(byte[] bytes)
    {
        uint crc = 0xffffffff;
        foreach (byte value in bytes)
        {
            crc ^= value;
            for (int bit = 0; bit < 8; bit++) crc = (crc >> 1) ^ ((crc & 1) == 0 ? 0 : 0xedb88320U);
        }
        return ~crc;
    }
    // A fixture generator, not a production archiver. RAR4 header offsets/CRC were
    // checked against bundled 7-Zip 26 source CPP/7zip/Archive/Rar/RarHandler.cpp,
    // RarHeader.h. Store method 0x30 has no compression algorithm to approximate.
    static void Rar4Block(BinaryWriter archive, byte type, ushort flags, Action<BinaryWriter> fields)
    {
        using (MemoryStream block = new MemoryStream())
        {
            using (BinaryWriter header = new BinaryWriter(block, Encoding.UTF8, true)) { header.Write(type); header.Write(flags); header.Write((ushort)0); fields(header); header.Flush(); }
            byte[] bytes = block.ToArray(); int size = bytes.Length + 2; bytes[3] = (byte)size; bytes[4] = (byte)(size >> 8);
            archive.Write((ushort)Crc32(bytes)); archive.Write(bytes);
        }
    }
    static void MakeStoredRar4(string path, params string[] books)
    {
        using (BinaryWriter archive = new BinaryWriter(File.Create(path)))
        {
            archive.Write(new byte[] { 0x52, 0x61, 0x72, 0x21, 0x1a, 0x07, 0x00 });
            Rar4Block(archive, 0x73, 0, h => { h.Write((ushort)0); h.Write((uint)0); });
            for (int index = 0; index < books.Length; index++)
            {
                byte[] bytes = File.ReadAllBytes(books[index]); byte[] name = Encoding.ASCII.GetBytes("folder/book" + index + ".fb2");
                Rar4Block(archive, 0x74, 0x8000, h => { h.Write((uint)bytes.Length); h.Write((uint)bytes.Length); h.Write((byte)2); h.Write(Crc32(bytes)); h.Write((uint)0x58210000); h.Write((byte)20); h.Write((byte)0x30); h.Write((ushort)name.Length); h.Write((uint)0x20); h.Write(name); });
                archive.Write(bytes);
            }
            Rar4Block(archive, 0x7b, 0, h => { });
        }
    }
    static void Vint(BinaryWriter writer, ulong value)
    {
        while (value >= 128) { writer.Write((byte)((value & 127) | 128)); value >>= 7; }
        writer.Write((byte)value);
    }
    static void Rar5Block(BinaryWriter archive, Action<BinaryWriter> fields)
    {
        using (MemoryStream block = new MemoryStream())
        {
            using (BinaryWriter header = new BinaryWriter(block, Encoding.UTF8, true)) { fields(header); header.Flush(); }
            byte[] body = block.ToArray();
            using (MemoryStream data = new MemoryStream())
            {
                using (BinaryWriter complete = new BinaryWriter(data, Encoding.UTF8, true)) { Vint(complete, (ulong)body.Length); complete.Write(body); complete.Flush(); }
                byte[] bytes = data.ToArray(); archive.Write(Crc32(bytes)); archive.Write(bytes);
            }
        }
    }
    // RAR5 store headers follow the primary format specification:
    // https://www.rarlab.com/technote.htm (CRC32, vint, main/file/end headers).
    static void MakeStoredRar5(string path, params string[] books)
    {
        using (BinaryWriter archive = new BinaryWriter(File.Create(path)))
        {
            archive.Write(new byte[] { 0x52, 0x61, 0x72, 0x21, 0x1a, 0x07, 0x01, 0x00 });
            Rar5Block(archive, h => { Vint(h, 1); Vint(h, 0); Vint(h, 0); });
            for (int index = 0; index < books.Length; index++)
            {
                byte[] bytes = File.ReadAllBytes(books[index]); byte[] name = Encoding.UTF8.GetBytes("Книги/книга" + index + ".fb2");
                Rar5Block(archive, h => { Vint(h, 2); Vint(h, 2); Vint(h, (ulong)bytes.Length); Vint(h, 4); Vint(h, (ulong)bytes.Length); Vint(h, 0x20); h.Write(Crc32(bytes)); Vint(h, 0); Vint(h, 0); Vint(h, (ulong)name.Length); h.Write(name); });
                archive.Write(bytes);
            }
            Rar5Block(archive, h => { Vint(h, 5); Vint(h, 0); Vint(h, 0); });
        }
    }
    public static int Main(string[] args)
    {
        root = AppDomain.CurrentDomain.BaseDirectory;
        artifacts = Path.GetFullPath(args[0]); Directory.CreateDirectory(artifacts);
        calibre = Path.Combine(root, "engine", "Calibre", "ebook-convert.exe"); sevenZip = Path.Combine(root, "engine", "Archive7z", "7z.exe");
        Environment.SetEnvironmentVariable("PATH", Path.GetDirectoryName(calibre) + Path.PathSeparator + Path.GetDirectoryName(sevenZip) + Path.PathSeparator + Environment.GetEnvironmentVariable("PATH"));
        sample = Path.Combine(artifacts, "sample.fb2"); File.WriteAllText(sample, Sample(), new UTF8Encoding(false));
        string second = Path.Combine(artifacts, "second.fb2"); File.WriteAllText(second, Sample().Replace("Проверочная книга", "Вторая проверочная книга"), new UTF8Encoding(false));
        Test("Bundled engine versions", delegate { Require(File.Exists(calibre) && File.Exists(sevenZip), "bundled engines missing"); Run(calibre, "--version", "calibre-version"); Run(sevenZip, "i", "7zip-version"); });
        Test("Real FB2 to AZW3 with measured intermediate progress", delegate
        {
            string output = Path.Combine(artifacts, "book.azw3"); ConversionResult result = Convert(sample, null, output, "azw3");
            Require(new FileInfo(output).Length > 1000 && result.ChapterCount == 2, "AZW3 output missing or metadata wrong");
        });
        Test("AZW3 roundtrip preserves text images footnotes and TOC", delegate
        {
            string output = Path.Combine(artifacts, "roundtrip.epub"); Run(calibre, Quote(Path.Combine(artifacts, "book.azw3")) + " " + Quote(output) + " --epub-version 3", "azw3-roundtrip"); VerifyEpub(output);
        });
        Test("Cancel live Calibre removes pending output and its process", delegate
        {
            string output = Path.Combine(artifacts, "cancelled.azw3"); List<int> before = EngineIds(); List<ConversionProgress> values = new List<ConversionProgress>();
            List<int> runningIds = new List<int>(); CancellationTokenSource c = new CancellationTokenSource(); Stopwatch timer = Stopwatch.StartNew(); bool wasCancelled = false;
            try
            {
                KindleConverter.Convert(sample, output, c.Token, delegate(ConversionProgress value)
                {
                    values.Add(value);
                    if (value.Stage == ConversionStage.ConvertingAzw3 && value.Percent >= 40) { runningIds.AddRange(EngineIds().Where(id => !before.Contains(id))); c.Cancel(); }
                });
            }
            catch (OperationCanceledException) { wasCancelled = true; }
            Require(wasCancelled && runningIds.Count > 0, "cancel did not interrupt a real running engine");
            Thread.Sleep(250);
            Require(!EngineIds().Except(before).Any(), "orphan conversion process remains");
            Require(!File.Exists(output) && !Directory.GetFiles(artifacts, ".fb2kindle-*").Any(), "cancelled output or pending file remains");
            Require(!values.Any(v => v.Percent == 100), "cancelled engine reported completion");
            Require(timer.Elapsed.TotalSeconds < 10, "engine cancellation took too long");
            File.WriteAllLines(Path.Combine(artifacts, "cancel-live-evidence.txt"), new[] { "cancelled=" + wasCancelled, "elapsedMilliseconds=" + timer.ElapsedMilliseconds, "enginePids=" + String.Join(",", runningIds), "orphans=0", "outputExists=False", "pendingFiles=0" });
        });
        string zipPath = Path.Combine(artifacts, "multi.zip");
        using (ZipArchive zip = ZipFile.Open(zipPath, ZipArchiveMode.Create)) { zip.CreateEntryFromFile(sample, "nested/book.fb2"); zip.CreateEntryFromFile(second, "second.fb2"); }
        string sevenPath = Path.Combine(artifacts, "multi.7z");
        Test("Create real multi-book 7Z fixture", delegate { Run(sevenZip, "a -t7z " + Quote(sevenPath) + " " + Quote(sample) + " " + Quote(second), "7zip-create"); });
        string rar4Path = Path.Combine(artifacts, "multi-rar4.rar"), rar5Path = Path.Combine(artifacts, "multi-rar5.rar");
        Test("Independent 7-Zip test verifies synthetic stored RAR4 fixture", delegate { MakeStoredRar4(rar4Path, sample, second); Run(sevenZip, "t " + Quote(rar4Path), "rar4-integrity"); });
        Test("Independent 7-Zip test verifies synthetic stored RAR5 fixture", delegate { MakeStoredRar5(rar5Path, sample, second); Run(sevenZip, "t " + Quote(rar5Path), "rar5-integrity"); });
        foreach (string archive in new[] { zipPath, sevenPath, rar4Path, rar5Path })
        {
            string archivePath = archive; string label = archivePath == rar4Path ? "rar4" : archivePath == rar5Path ? "rar5" : Path.GetExtension(archivePath).TrimStart('.');
            Test(label.ToUpperInvariant() + " multi-book EPUB and AZW3 conversion", delegate
            {
                List<ArchiveBook> entries = ArchiveBooks.List(archivePath); Require(entries.Count == 2, "archive book count mismatch");
                for (int i = 0; i < entries.Count; i++)
                {
                    string extension = i == 0 ? ".epub" : ".azw3";
                    string output = Path.Combine(artifacts, label + "-book-" + i + extension);
                    Convert(archivePath, entries[i], output, label + "-" + i);
                    if (extension == ".epub") VerifyEpub(output);
                    else { string roundtrip = Path.Combine(artifacts, label + "-roundtrip.epub"); Run(calibre, Quote(output) + " " + Quote(roundtrip), label + "-roundtrip"); VerifyEpub(roundtrip); }
                }
            });
        }
        Console.WriteLine("RESULT " + passed + " passed, " + failures.Count + " failed");
        File.WriteAllText(Path.Combine(artifacts, "result.txt"), passed + " passed, " + failures.Count + " failed\n" + String.Join("\n", failures) + "\nRAR4/RAR5 fixtures use store mode and are independently verified with 7-Zip. Compressed/encrypted/multivolume RAR and physical Kindle/Send to Kindle not tested.\n", new UTF8Encoding(false));
        return failures.Count == 0 ? 0 : 1;
    }
}
