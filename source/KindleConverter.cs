using System;
using System.IO;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

namespace Fb2Kindle
{
    public static class KindleConverter
    {
        public static ConversionResult Convert(string inputPath, ArchiveBook entry, string outputPath, CancellationToken cancellation)
        {
            if (entry == null) return Convert(inputPath, outputPath, cancellation);
            cancellation.ThrowIfCancellationRequested();
            if (File.Exists(outputPath) || Directory.Exists(outputPath)) throw new IOException(L10n.T("Файл уже существует: ") + outputPath);
            string staging = Path.Combine(Path.GetTempPath(), "FB2Kindle-archive-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(staging);
            try
            {
                string book = Path.Combine(staging, "book.fb2");
                ArchiveBooks.Extract(inputPath, entry, book, cancellation);
                cancellation.ThrowIfCancellationRequested();
                return Convert(book, outputPath, cancellation);
            }
            finally
            {
                try { if (Directory.Exists(staging)) Directory.Delete(staging, true); } catch (IOException) { } catch (UnauthorizedAccessException) { }
            }
        }

        public static string FindCalibre()
        {
            string[] candidates = {
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "engine", "Calibre", "ebook-convert.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Calibre2", "ebook-convert.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Calibre2", "ebook-convert.exe")
            };
            foreach (string candidate in candidates) if (File.Exists(candidate)) return candidate;
            foreach (string directory in (Environment.GetEnvironmentVariable("PATH") ?? "").Split(Path.PathSeparator))
            {
                try { string candidate = Path.Combine(directory.Trim('"'), "ebook-convert.exe"); if (File.Exists(candidate)) return candidate; }
                catch (ArgumentException) { }
            }
            return null;
        }

        public static ConversionResult Convert(string inputPath, string outputPath, CancellationToken cancellation)
        {
            cancellation.ThrowIfCancellationRequested();
            string output = Path.GetFullPath(outputPath);
            string extension = Path.GetExtension(output).ToLowerInvariant();
            if (extension != ".epub" && extension != ".azw3") throw new ArgumentException(L10n.T("Выберите формат EPUB или AZW3."));
            string sourceExtension = Path.GetExtension(inputPath).ToLowerInvariant();
            if (sourceExtension == ".zip" || sourceExtension == ".rar" || sourceExtension == ".7z")
            {
                List<ArchiveBook> entries = ArchiveBooks.List(inputPath, cancellation);
                if (entries.Count != 1) throw new InvalidDataException(L10n.T("В архиве несколько книг. Добавьте архив в окно программы: каждая книга будет сохранена отдельным файлом."));
                return Convert(inputPath, entries[0], output, cancellation);
            }
            if (extension == ".epub")
            {
                // The core writes atomically. A completed book is retained if cancellation
                // arrives at the end; the batch stops before starting the next book.
                return Fb2Converter.Convert(inputPath, output);
            }
            if (extension != ".azw3") throw new ArgumentException(L10n.T("Выберите формат EPUB или AZW3."));
            if (File.Exists(output)) throw new IOException(L10n.T("Файл уже существует: ") + output);
            string engine = FindCalibre();
            if (engine == null) throw new InvalidOperationException(L10n.T("Движок AZW3 не найден. Распакуйте полный архив программы вместе с папкой engine или установите Calibre."));
            string parent = Path.GetDirectoryName(output);
            if (!Directory.Exists(parent)) throw new DirectoryNotFoundException(L10n.T("Папка сохранения не найдена."));
            string staging = Path.Combine(Path.GetTempPath(), "FB2Kindle-" + Guid.NewGuid().ToString("N"));
            string pending = Path.Combine(parent, ".fb2kindle-" + Guid.NewGuid().ToString("N") + ".azw3");
            Directory.CreateDirectory(staging);
            try
            {
                string epub = Path.Combine(staging, "book.epub");
                ConversionResult result = Fb2Converter.Convert(inputPath, epub);
                cancellation.ThrowIfCancellationRequested();
                StringBuilder log = new StringBuilder();
                object logLock = new object();
                NormalizeDuplicateEnvironmentNames();
                ProcessStartInfo info = new ProcessStartInfo(engine, Quote(epub) + " " + Quote(pending) + " --output-profile kindle_pw3");
                info.UseShellExecute = false;
                info.CreateNoWindow = true;
                info.WorkingDirectory = staging;
                info.RedirectStandardOutput = true;
                info.RedirectStandardError = true;
                info.StandardOutputEncoding = Encoding.UTF8;
                info.StandardErrorEncoding = Encoding.UTF8;
                info.EnvironmentVariables["CALIBRE_CONFIG_DIRECTORY"] = Path.Combine(staging, "config");
                info.EnvironmentVariables["CALIBRE_CACHE_DIRECTORY"] = Path.Combine(staging, "cache");
                info.EnvironmentVariables["CALIBRE_TEMP_DIR"] = staging;
                using (Process process = new Process())
                {
                    process.StartInfo = info;
                    DataReceivedEventHandler capture = delegate(object sender, DataReceivedEventArgs e)
                    {
                        if (e.Data == null) return;
                        lock (logLock) { log.AppendLine(e.Data); if (log.Length > 16000) log.Remove(0, log.Length - 16000); }
                    };
                    process.OutputDataReceived += capture;
                    process.ErrorDataReceived += capture;
                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    Stopwatch timeout = Stopwatch.StartNew();
                    while (!process.WaitForExit(200))
                    {
                        if (cancellation.IsCancellationRequested || timeout.Elapsed.TotalMinutes > 5)
                        {
                            try { process.Kill(); } catch (InvalidOperationException) { }
                            process.WaitForExit();
                            cancellation.ThrowIfCancellationRequested();
                            throw new TimeoutException(L10n.T("Создание AZW3 заняло больше пяти минут. Попробуйте EPUB или книгу меньшего размера."));
                        }
                    }
                    process.WaitForExit();
                    if (process.ExitCode != 0 || !File.Exists(pending) || new FileInfo(pending).Length == 0)
                    {
                        string details; lock (logLock) details = log.ToString().Trim();
                        throw new IOException(L10n.T("Не удалось создать AZW3.\r\n") + details);
                    }
                }
                cancellation.ThrowIfCancellationRequested();
                File.Move(pending, output);
                return result;
            }
            finally
            {
                try { if (File.Exists(pending)) File.Delete(pending); } catch (IOException) { } catch (UnauthorizedAccessException) { }
                try { if (Directory.Exists(staging)) Directory.Delete(staging, true); } catch (IOException) { } catch (UnauthorizedAccessException) { }
            }
        }

        // Some launchers supply both Path and PATH in the inherited environment.
        // .NET Framework's child environment is case-insensitive and rejects these.
        // Normalize only duplicate names in this process, preserving all PATH entries.
        internal static void NormalizeDuplicateEnvironmentNames()
        {
            Dictionary<string, List<DictionaryEntry>> groups = new Dictionary<string, List<DictionaryEntry>>(StringComparer.OrdinalIgnoreCase);
            foreach (DictionaryEntry entry in Environment.GetEnvironmentVariables())
            {
                string name = (string)entry.Key;
                List<DictionaryEntry> entries;
                if (!groups.TryGetValue(name, out entries)) { entries = new List<DictionaryEntry>(); groups.Add(name, entries); }
                entries.Add(entry);
            }
            foreach (KeyValuePair<string, List<DictionaryEntry>> group in groups)
            {
                if (group.Value.Count < 2) continue;
                string value = (string)group.Value[0].Value;
                if (String.Equals(group.Key, "PATH", StringComparison.OrdinalIgnoreCase))
                {
                    List<string> paths = new List<string>();
                    foreach (DictionaryEntry entry in group.Value)
                    {
                        string path = (string)entry.Value;
                        if (!paths.Contains(path)) paths.Add(path);
                    }
                    value = String.Join(Path.PathSeparator.ToString(), paths);
                }
                foreach (DictionaryEntry entry in group.Value) Environment.SetEnvironmentVariable((string)entry.Key, null);
                Environment.SetEnvironmentVariable(group.Key, value);
            }
        }

        // Windows CommandLineToArgvW quoting, including a trailing backslash.
        private static string Quote(string value)
        {
            StringBuilder quoted = new StringBuilder("\"");
            int slashes = 0;
            foreach (char character in value)
            {
                if (character == '\\') { slashes++; continue; }
                if (character == '"') { quoted.Append('\\', slashes * 2 + 1); quoted.Append('"'); }
                else { quoted.Append('\\', slashes); quoted.Append(character); }
                slashes = 0;
            }
            quoted.Append('\\', slashes * 2); quoted.Append('"');
            return quoted.ToString();
        }

        public static string AvailableOutput(string source, string directory, string extension)
        {
            string name = source.Replace('\\', '/');
            name = name.Substring(name.LastIndexOf('/') + 1);
            if (name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase)) name = name.Substring(0, name.Length - 4);
            if (name.EndsWith(".fb2", StringComparison.OrdinalIgnoreCase)) name = name.Substring(0, name.Length - 4);
            StringBuilder safeName = new StringBuilder();
            char[] invalid = Path.GetInvalidFileNameChars();
            foreach (char character in name) safeName.Append(Array.IndexOf(invalid, character) >= 0 ? '_' : character);
            name = safeName.ToString().TrimEnd(' ', '.');
            if (name.Length > 160) name = name.Substring(0, 160).TrimEnd(' ', '.');
            if (String.IsNullOrWhiteSpace(name)) name = "book";
            string reserved = name.Split('.')[0].ToUpperInvariant();
            if (reserved == "CON" || reserved == "PRN" || reserved == "AUX" || reserved == "NUL" ||
                (reserved.Length == 4 && (reserved.StartsWith("COM") || reserved.StartsWith("LPT")) && reserved[3] >= '1' && reserved[3] <= '9')) name = "_" + name;
            string candidate = Path.Combine(directory, name + extension);
            int number = 2;
            while (File.Exists(candidate) || Directory.Exists(candidate)) candidate = Path.Combine(directory, name + " (" + number++ + ")" + extension);
            return candidate;
        }
    }
}
