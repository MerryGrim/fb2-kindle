using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Fb2Kindle
{
    static partial class L10n
    {
        static readonly Dictionary<string, string[]> Translations = new Dictionary<string, string[]>(StringComparer.Ordinal);
        static volatile string languageCode = "ru";
        static string SettingsPath
        {
            get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FB2Kindle", "language.txt"); }
        }

        static L10n()
        {
            InitializeTranslations();
            try
            {
                string saved = File.ReadAllText(SettingsPath, Encoding.UTF8).Trim();
                if (saved == "ru" || saved == "en" || saved == "zh") languageCode = saved;
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }

        public static string LanguageCode { get { return languageCode; } }

        public static void SetLanguage(string code)
        {
            if (code != "ru" && code != "en" && code != "zh") throw new ArgumentException("Unsupported UI language", "code");
            languageCode = code;
            try
            {
                string path = SettingsPath;
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(path, code, new UTF8Encoding(false));
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }

        public static string T(string russian)
        {
            if (russian == null || languageCode == "ru") return russian;
            string[] translated;
            if (!Translations.TryGetValue(russian, out translated)) return russian;
            return translated[languageCode == "zh" ? 1 : 0];
        }

        static void Add(string russian, string english, string chinese)
        {
            Translations.Add(russian, new[] { english, chinese });
        }
    }
}
