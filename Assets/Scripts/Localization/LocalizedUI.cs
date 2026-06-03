namespace BogatyriMoba.Localization
{
    public static class LocalizedUI
    {
        public static string Get(string key) => LocalizationCatalog.Get(key);

        public static string Format(string key, params object[] args) =>
            LocalizationCatalog.Format(key, args);
    }
}
