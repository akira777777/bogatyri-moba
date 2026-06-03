#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using BogatyriMoba.Core;
using BogatyriMoba.GameModes;
using BogatyriMoba.UI;
using BogatyriMoba.UI.Mobile;

namespace BogatyriMoba.EditorTools
{
    public static class UiHudBuilder
    {
        public const string ThemeAssetPath = "Assets/Resources/UI/DefaultUITheme.asset";

        public static void EnsureThemeAsset()
        {
            if (!System.IO.Directory.Exists("Assets/Resources/UI"))
                System.IO.Directory.CreateDirectory("Assets/Resources/UI");

            if (AssetDatabase.LoadAssetAtPath<UITheme>(ThemeAssetPath) != null)
                return;

            var theme = ScriptableObject.CreateInstance<UITheme>();
            AssetDatabase.CreateAsset(theme, ThemeAssetPath);
            AssetDatabase.SaveAssets();
        }

        public static MatchHudController BuildMatchUi(GameManager gm, GameMode mode)
        {
            EnsureThemeAsset();
            var theme = AssetDatabase.LoadAssetAtPath<UITheme>(ThemeAssetPath);

            GameObject canvasGO = new GameObject("Canvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGO.AddComponent<CanvasScaler>();
            if (theme != null)
                theme.ApplyToCanvasScaler(scaler);
            else
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080f, 1920f);
                scaler.matchWidthOrHeight = 0.5f;
            }

            canvasGO.AddComponent<GraphicRaycaster>();

            float inset = theme != null ? theme.safeAreaInsetPx : 48f;

            GameObject safeArea = CreateRectChild(canvasGO.transform, "SafeArea");
            StretchFull(safeArea.GetComponent<RectTransform>());
            var safeRect = safeArea.GetComponent<RectTransform>();
            safeRect.offsetMin = new Vector2(inset, inset);
            safeRect.offsetMax = new Vector2(-inset, -inset);

            GameObject hudRoot = CreateRectChild(safeArea.transform, "MatchHUD");
            StretchFull(hudRoot.GetComponent<RectTransform>());
            var hudController = hudRoot.AddComponent<MatchHudController>();

            var timerUI = CreateTimer(hudRoot.transform, theme);
            var teamScore = CreateTeamScore(hudRoot.transform, theme);
            var heistHud = CreateHeistHud(hudRoot.transform, theme);
            heistHud.gameObject.SetActive(mode is HeistMode);

            SetPrivateField(hudController, "timerUI", timerUI);
            SetPrivateField(hudController, "teamScoreUI", teamScore);
            SetPrivateField(hudController, "heistSafeHudUI", heistHud);

            var gameOver = CreateGameOver(safeArea.transform, theme);
            // gameOver and mobile controls created in scene; no longer assigned to GameManager
            var mobile = CreateMobileControls(safeArea.transform, theme);

            hudController.BindGameMode(mode);
            // timerUI wired via SetPrivateField on hudController above

            return hudController;
        }

        private static MatchTimerUI CreateTimer(Transform parent, UITheme theme)
        {
            var go = CreateRectChild(parent, "Timer");
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -8f);
            rect.sizeDelta = new Vector2(220f, 56f);

            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = theme != null ? theme.timerFontSize : 32;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.text = "02:30";
            tmp.color = theme != null ? theme.accentGold : Color.yellow;

            var timerUI = go.AddComponent<MatchTimerUI>();
            SetPrivateField(timerUI, "timerText", tmp);
            SetPrivateField(timerUI, "theme", theme);
            return timerUI;
        }

        private static TeamScoreUI CreateTeamScore(Transform parent, UITheme theme)
        {
            var root = CreateRectChild(parent, "TeamScores");
            var rootRect = root.GetComponent<RectTransform>();
            rootRect.anchorMin = new Vector2(0f, 1f);
            rootRect.anchorMax = new Vector2(1f, 1f);
            rootRect.pivot = new Vector2(0.5f, 1f);
            rootRect.anchoredPosition = new Vector2(0f, -72f);
            rootRect.sizeDelta = new Vector2(0f, 64f);

            var blue = CreateScoreLabel(root.transform, "BlueScore", TextAnchor.MiddleLeft, theme, true);
            var red = CreateScoreLabel(root.transform, "RedScore", TextAnchor.MiddleRight, theme, false);

            var teamUI = root.AddComponent<TeamScoreUI>();
            SetPrivateField(teamUI, "blueScoreText", blue);
            SetPrivateField(teamUI, "redScoreText", red);
            SetPrivateField(teamUI, "theme", theme);
            return teamUI;
        }

        private static TextMeshProUGUI CreateScoreLabel(Transform parent, string name, TextAnchor anchor, UITheme theme, bool isBlue)
        {
            var go = CreateRectChild(parent, name);
            var rect = go.GetComponent<RectTransform>();
            if (anchor == TextAnchor.MiddleLeft)
            {
                rect.anchorMin = new Vector2(0f, 0f);
                rect.anchorMax = new Vector2(0.5f, 1f);
                rect.offsetMin = new Vector2(8f, 0f);
                rect.offsetMax = Vector2.zero;
            }
            else
            {
                rect.anchorMin = new Vector2(0.5f, 0f);
                rect.anchorMax = new Vector2(1f, 1f);
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = new Vector2(-8f, 0f);
            }

            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = theme != null ? theme.scoreFontSize : 28;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = anchor == TextAnchor.MiddleLeft
                ? TextAlignmentOptions.MidlineLeft
                : TextAlignmentOptions.MidlineRight;
            tmp.text = "0";
            tmp.color = theme != null
                ? (isBlue ? theme.teamBlue : theme.teamRed)
                : (isBlue ? Color.cyan : Color.red);
            return tmp;
        }

        private static HeistSafeHudUI CreateHeistHud(Transform parent, UITheme theme)
        {
            var root = CreateRectChild(parent, "HeistSafeHud");
            var rootRect = root.GetComponent<RectTransform>();
            rootRect.anchorMin = new Vector2(0f, 1f);
            rootRect.anchorMax = new Vector2(1f, 1f);
            rootRect.pivot = new Vector2(0.5f, 1f);
            rootRect.anchoredPosition = new Vector2(0f, -140f);
            rootRect.sizeDelta = new Vector2(0f, 40f);

            var blue = CreateScoreLabel(root.transform, "BlueSafe", TextAnchor.MiddleLeft, theme, true);
            var red = CreateScoreLabel(root.transform, "RedSafe", TextAnchor.MiddleRight, theme, false);
            blue.fontSize = 22;
            red.fontSize = 22;

            var hud = root.AddComponent<HeistSafeHudUI>();
            SetPrivateField(hud, "blueSafeText", blue);
            SetPrivateField(hud, "redSafeText", red);
            SetPrivateField(hud, "theme", theme);
            return hud;
        }

        private static GameOverUI CreateGameOver(Transform parent, UITheme theme)
        {
            var panel = CreateRectChild(parent, "GameOverPanel");
            StretchFull(panel.GetComponent<RectTransform>());
            var dim = panel.AddComponent<Image>();
            dim.color = new Color(0f, 0f, 0f, 0.75f);

            var box = CreateRectChild(panel.transform, "Content");
            var boxRect = box.GetComponent<RectTransform>();
            boxRect.anchorMin = new Vector2(0.5f, 0.5f);
            boxRect.anchorMax = new Vector2(0.5f, 0.5f);
            boxRect.sizeDelta = new Vector2(520f, 360f);
            var boxBg = box.AddComponent<Image>();
            boxBg.color = theme != null ? theme.hudPanelBackground : new Color(0f, 0f, 0f, 0.85f);

            var titleGo = CreateRectChild(box.transform, "Title");
            var titleRect = titleGo.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0f, 0.55f);
            titleRect.anchorMax = new Vector2(1f, 0.95f);
            titleRect.offsetMin = new Vector2(24f, 0f);
            titleRect.offsetMax = new Vector2(-24f, 0f);
            var titleTmp = titleGo.AddComponent<TextMeshProUGUI>();
            titleTmp.fontSize = theme != null ? theme.gameOverTitleFontSize : 36;
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.alignment = TextAlignmentOptions.Center;

            var bodyGo = CreateRectChild(box.transform, "Body");
            var bodyRect = bodyGo.GetComponent<RectTransform>();
            bodyRect.anchorMin = new Vector2(0f, 0.28f);
            bodyRect.anchorMax = new Vector2(1f, 0.55f);
            bodyRect.offsetMin = new Vector2(24f, 0f);
            bodyRect.offsetMax = new Vector2(-24f, 0f);
            var bodyTmp = bodyGo.AddComponent<TextMeshProUGUI>();
            bodyTmp.fontSize = theme != null ? theme.gameOverBodyFontSize : 22;
            bodyTmp.alignment = TextAlignmentOptions.Center;
            bodyTmp.enableWordWrapping = true;

            var buttonGo = CreateRectChild(box.transform, "PlayAgainButton");
            var buttonRect = buttonGo.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.2f, 0.05f);
            buttonRect.anchorMax = new Vector2(0.8f, 0.22f);
            buttonRect.offsetMin = Vector2.zero;
            buttonRect.offsetMax = Vector2.zero;
            var buttonImage = buttonGo.AddComponent<Image>();
            buttonImage.color = theme != null ? theme.teamBlue : Color.cyan;
            var button = buttonGo.AddComponent<Button>();

            var labelGo = CreateRectChild(buttonGo.transform, "Label");
            StretchFull(labelGo.GetComponent<RectTransform>());
            var labelTmp = labelGo.AddComponent<TextMeshProUGUI>();
            labelTmp.fontSize = theme != null ? theme.controlLabelFontSize : 18;
            labelTmp.alignment = TextAlignmentOptions.Center;
            labelTmp.color = Color.white;

            var gameOverUI = panel.AddComponent<GameOverUI>();
            SetPrivateField(gameOverUI, "panelRoot", panel);
            SetPrivateField(gameOverUI, "titleText", titleTmp);
            SetPrivateField(gameOverUI, "bodyText", bodyTmp);
            SetPrivateField(gameOverUI, "playAgainButton", button);
            SetPrivateField(gameOverUI, "playAgainLabel", labelTmp);
            SetPrivateField(gameOverUI, "theme", theme);

            panel.SetActive(false);
            return gameOverUI;
        }

        private static MobileControlsUI CreateMobileControls(Transform parent, UITheme theme)
        {
            var root = CreateRectChild(parent, "MobileControls");
            StretchFull(root.GetComponent<RectTransform>());

            var moveJoy = CreateJoystick(root.transform, "MoveJoystick",
                new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(160f, 160f), new Vector2(120f, 120f));
            var aimJoy = CreateJoystick(root.transform, "AimJoystick",
                new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-200f, 160f), new Vector2(120f, 120f));

            var attackBtn = CreateActionButton(root.transform, "AttackButton", new Vector2(-120f, 280f), theme);
            var superBtn = CreateActionButton(root.transform, "SuperButton", new Vector2(-220f, 360f), theme);
            var gadgetBtn = CreateActionButton(root.transform, "GadgetButton", new Vector2(-20f, 360f), theme);

            var mobile = root.AddComponent<MobileControlsUI>();
            SetPrivateField(mobile, "moveJoystick", moveJoy);
            SetPrivateField(mobile, "aimJoystick", aimJoy);
            SetPrivateField(mobile, "attackButton", attackBtn.button);
            SetPrivateField(mobile, "superButton", superBtn.button);
            SetPrivateField(mobile, "gadgetButton", gadgetBtn.button);
            SetPrivateField(mobile, "attackLabel", attackBtn.label);
            SetPrivateField(mobile, "superLabel", superBtn.label);
            SetPrivateField(mobile, "gadgetLabel", gadgetBtn.label);
            return mobile;
        }

        private static VirtualJoystick CreateJoystick(Transform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 size)
        {
            var go = CreateRectChild(parent, name);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = anchorMin;
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = size;

            var bg = go.AddComponent<Image>();
            bg.color = new Color(1f, 1f, 1f, 0.15f);
            bg.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");

            var handleGo = CreateRectChild(go.transform, "Handle");
            var handleRect = handleGo.GetComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(56f, 56f);
            var handleImg = handleGo.AddComponent<Image>();
            handleImg.color = new Color(1f, 1f, 1f, 0.45f);
            handleImg.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");

            var joy = go.AddComponent<VirtualJoystick>();
            SetPrivateField(joy, "background", rect);
            SetPrivateField(joy, "handle", handleRect);
            return joy;
        }

        private static (Button button, TextMeshProUGUI label) CreateActionButton(
            Transform parent, string name, Vector2 pos, UITheme theme)
        {
            var go = CreateRectChild(parent, name);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(1f, 0f);
            rect.anchoredPosition = pos;
            rect.sizeDelta = new Vector2(88f, 88f);

            var img = go.AddComponent<Image>();
            img.color = theme != null ? theme.hudPanelBackground : new Color(0f, 0f, 0f, 0.6f);
            img.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            var button = go.AddComponent<Button>();

            var labelGo = CreateRectChild(go.transform, "Label");
            StretchFull(labelGo.GetComponent<RectTransform>());
            var label = labelGo.AddComponent<TextMeshProUGUI>();
            label.fontSize = theme != null ? theme.controlLabelFontSize : 16;
            label.alignment = TextAlignmentOptions.Center;
            label.color = Color.white;

            return (button, label);
        }

        private static GameObject CreateRectChild(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        private static void StretchFull(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            field?.SetValue(target, value);
        }
    }
}
#endif
