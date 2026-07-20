namespace SimpleML.Localization
{
    /// <summary>
    /// Localization helper. Default language is English.
    /// </summary>
    public static class L
    {
        public static string T(string key)
        {
            if (ComponentLocales.TryGet(key, out string en, out string zh))
                return UiLanguage.IsChinese ? zh : en;
            return key;
        }

        public static string T(string key, params object[] args)
        {
            string fmt = T(key);
            try { return string.Format(fmt, args); }
            catch { return fmt; }
        }

        public static string Comp(string componentClass, string field)
        {
            return T("comp." + componentClass + "." + field);
        }

        public static string Name(string componentClass) => Comp(componentClass, "name");
        public static string Nick(string componentClass) => Comp(componentClass, "nick");
        public static string Desc(string componentClass) => Comp(componentClass, "desc");
    }
}
