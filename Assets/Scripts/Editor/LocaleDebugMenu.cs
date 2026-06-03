#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using BogatyriMoba.Localization;

namespace BogatyriMoba.EditorTools
{
    public static class LocaleDebugMenu
    {
        [MenuItem("Bogatyri/Locale/Use Russian (ru)")]
        private static void UseRussian()
        {
            LocaleManager.SetLocale(LocaleManager.Russian);
            UnityEngine.Debug.Log("[Locale] Active locale: ru");
        }

        [MenuItem("Bogatyri/Locale/Use English (en)")]
        private static void UseEnglish()
        {
            LocaleManager.SetLocale(LocaleManager.English);
            UnityEngine.Debug.Log("[Locale] Active locale: en");
        }
    }
}
#endif
