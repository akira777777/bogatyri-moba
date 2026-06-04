using UnityEngine;
using UnityEngine.UI;

namespace BogatyriMoba.UI
{
    [CreateAssetMenu(fileName = "DefaultUITheme", menuName = "Bogatyri/UI Theme")]
    public class UITheme : ScriptableObject
    {
        public Color teamBlue = new Color(0.302f, 0.671f, 0.969f, 1f);
        public Color teamRed = new Color(1f, 0.42f, 0.42f, 1f);
        public Color accentGold = new Color(1f, 0.835f, 0.31f, 1f);
        public Color hudPanelBackground = new Color(0f, 0f, 0f, 0.6f);
        public Color neutralGray = new Color(0.62f, 0.62f, 0.62f, 1f);

        public int timerFontSize = 32;
        public int scoreFontSize = 28;
        public int gameOverTitleFontSize = 36;
        public int gameOverBodyFontSize = 22;
        public int controlLabelFontSize = 18;

        public Vector2 referenceResolution = new Vector2(1080f, 1920f);
        public float canvasMatch = 0.5f;
        public float safeAreaInsetPx = 48f;

        public void ApplyToCanvasScaler(CanvasScaler scaler)
        {
            if (scaler == null) return;
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = referenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = canvasMatch;
        }

        private static UITheme _cached;

        public static UITheme LoadDefault()
        {
            if (_cached != null) return _cached;
            _cached = Core.AssetLoader.Default.LoadAsset<UITheme>("UI/DefaultUITheme");
            if (_cached == null)
                _cached = CreateInstance<UITheme>();
            return _cached;
        }
    }
}
