using System;
using System.IO;

namespace SimpleML.Localization
{
    /// <summary>
    /// UI language preference. Default: English.
    /// Ribbon names apply at Grasshopper load; runtime texts follow the current setting.
    /// </summary>
    public static class UiLanguage
    {
        public const string English = "en";
        public const string Chinese = "zh";

        private static string _cached;

        public static string PreferencePath
        {
            get
            {
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                return Path.Combine(appData, "Grasshopper", "SimpleML", "language.txt");
            }
        }

        public static string Current
        {
            get
            {
                if (!string.IsNullOrEmpty(_cached))
                    return _cached;

                try
                {
                    string env = Environment.GetEnvironmentVariable("SIMPLEML_LANG");
                    if (!string.IsNullOrWhiteSpace(env))
                    {
                        _cached = Normalize(env);
                        return _cached;
                    }
                }
                catch { }

                try
                {
                    string path = PreferencePath;
                    if (File.Exists(path))
                    {
                        string raw = File.ReadAllText(path).Trim();
                        _cached = Normalize(raw);
                        return _cached;
                    }
                }
                catch { }

                _cached = English;
                return _cached;
            }
        }

        public static bool IsChinese => Current == Chinese;
        public static bool IsEnglish => Current == English;

        public static string Normalize(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return English;
            value = value.Trim().ToLowerInvariant();
            if (value == "zh" || value == "zh-cn" || value == "zh_cn" || value == "cn"
                || value == "chinese" || value == "中文")
                return Chinese;
            return English;
        }

        public static void Set(string language)
        {
            string lang = Normalize(language);
            _cached = lang;
            try
            {
                Environment.SetEnvironmentVariable("SIMPLEML_LANG", lang);
                string path = PreferencePath;
                string dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                File.WriteAllText(path, lang + Environment.NewLine);
            }
            catch { }
        }

        public static void Reload()
        {
            _cached = null;
            _ = Current;
        }
    }
}
