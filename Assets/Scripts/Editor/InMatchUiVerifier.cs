#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using BogatyriMoba.Localization;

namespace BogatyriMoba.EditorTools
{
    public static class InMatchUiVerifier
    {
        private static readonly string[] RequiredKeys =
        {
            "gameover.title_win",
            "gameover.title_loss",
            "gameover.title_draw",
            "gameover.body_win",
            "gameover.body_loss",
            "gameover.body_draw",
            "gameover.play_again",
            "controls.attack",
            "controls.super",
            "controls.gadget",
            "heist.safe_hp",
        };

        [MenuItem("Bogatyri/Verify In-Match UI")]
        public static void Verify()
        {
            int errors = 0;
            foreach (var key in RequiredKeys)
            {
                LocaleManager.SetLocale(LocaleManager.Russian);
                var ru = LocalizationCatalog.Get(key);
                LocaleManager.SetLocale(LocaleManager.English);
                var en = LocalizationCatalog.Get(key);

                if (ru == key || en == key || string.IsNullOrEmpty(ru) || string.IsNullOrEmpty(en))
                {
                    Debug.LogError($"[Verify] Missing localization for key: {key}");
                    errors++;
                }
            }

            if (errors == 0)
                Debug.Log("[Verify] In-match UI localization catalog OK (ru + en).");
            else
                Debug.LogError($"[Verify] Failed with {errors} missing keys.");
        }
    }
}
#endif
