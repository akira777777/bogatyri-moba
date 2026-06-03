#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using BogatyriMoba.Localization;

namespace BogatyriMoba.EditorTools
{
    public static class LocaleDebugMenu
    {
        [MenuItem("Bogatyri/Locale/Use Russian (ru)")]
        private static void UseRussian()
        {
            LocaleManager.SetLocale(LocaleManager.Russian);
            Debug.Log("[Locale] Active locale: ru");
        }

        [MenuItem("Bogatyri/Locale/Use English (en)")]
        private static void UseEnglish()
        {
            LocaleManager.SetLocale(LocaleManager.English);
            Debug.Log("[Locale] Active locale: en");
        }
    }
}
#endif
