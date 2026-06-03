using UnityEngine;

namespace BogatyriMoba.Localization
{
    public static class LocaleManager
    {
        public const string PrefKey = "bogatyri_locale";
        public const string Russian = "ru";
        public const string English = "en";

        public static string CurrentLocale { get; private set; } = Russian;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoInitialize()
        {
            Initialize();
        }

        public static void Initialize()
        {
            if (PlayerPrefs.HasKey(PrefKey))
            {
                CurrentLocale = PlayerPrefs.GetString(PrefKey, Russian);
                return;
            }

            CurrentLocale = MapSystemLanguage(Application.systemLanguage);
            PlayerPrefs.SetString(PrefKey, CurrentLocale);
            PlayerPrefs.Save();
        }

        public static void SetLocale(string localeCode)
        {
            if (string.IsNullOrEmpty(localeCode)) return;
            CurrentLocale = localeCode == English ? English : Russian;
            PlayerPrefs.SetString(PrefKey, CurrentLocale);
            PlayerPrefs.Save();
        }

        private static string MapSystemLanguage(SystemLanguage language)
        {
            switch (language)
            {
                case SystemLanguage.Russian:
                case SystemLanguage.Ukrainian:
                case SystemLanguage.Belarusian:
                    return Russian;
                default:
                    return English;
            }
        }
    }
}
