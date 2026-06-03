using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BogatyriMoba.Core;

namespace BogatyriMoba.UI
{
    public static class WorldHealthBarFactory
    {
        public static UIHealthBar AttachToBrawler(BrawlerController brawler, UITheme theme)
        {
            if (brawler == null) return null;
            if (theme == null)
                theme = UITheme.LoadDefault();

            var root = new GameObject("HealthBar");
            root.transform.SetParent(brawler.transform, false);
            root.transform.localPosition = new Vector3(0f, 1.2f, 0f);

            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 10;
            var scaler = root.AddComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 100f;
            root.AddComponent<GraphicRaycaster>();

            var rect = root.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(120f, 20f);
            rect.localScale = Vector3.one * 0.01f;

            var bgGo = new GameObject("Background");
            bgGo.transform.SetParent(root.transform, false);
            var bgRect = bgGo.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            var bgImage = bgGo.AddComponent<Image>();
            bgImage.color = new Color(0f, 0f, 0f, 0.65f);
            bgImage.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Background.psd");

            var fillGo = new GameObject("Fill");
            fillGo.transform.SetParent(root.transform, false);
            var fillRect = fillGo.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            var fillImage = fillGo.AddComponent<Image>();
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
            fillImage.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Background.psd");

            Color tint = brawler.TeamId == GameManager.TEAM_BLUE ? theme.teamBlue : theme.teamRed;
            fillImage.color = tint;

            // Add Name Text
            var nameGo = new GameObject("BrawlerName");
            nameGo.transform.SetParent(root.transform, false);
            var nameRect = nameGo.AddComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0.5f, 1f);
            nameRect.anchorMax = new Vector2(0.5f, 1f);
            nameRect.pivot = new Vector2(0.5f, 0f);
            nameRect.anchoredPosition = new Vector2(0f, 4f);
            nameRect.sizeDelta = new Vector2(200f, 30f);

            var nameText = nameGo.AddComponent<TextMeshProUGUI>();
            nameText.text = brawler.GetData() != null ? brawler.GetData().brawlerName : brawler.name;
            nameText.fontSize = 14f;
            nameText.alignment = TextAlignmentOptions.Center;
            nameText.color = Color.white;
            nameText.raycastTarget = false;

            var healthBar = root.AddComponent<UIHealthBar>();
            healthBar.Configure(fillImage, bgImage);
            healthBar.SetTarget(brawler, tint);
            return healthBar;
        }
    }
}
