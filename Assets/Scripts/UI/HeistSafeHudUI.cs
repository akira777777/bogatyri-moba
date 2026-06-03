using TMPro;
using UnityEngine;
using BogatyriMoba.GameModes;
using BogatyriMoba.Core;

namespace BogatyriMoba.UI
{
    public class HeistSafeHudUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI blueSafeText;
        [SerializeField] private TextMeshProUGUI redSafeText;
        [SerializeField] private UITheme theme;

        private HeistMode _heist;

        private void Awake()
        {
            if (theme == null)
                theme = UITheme.LoadDefault();
        }

        public void BindHeistMode(HeistMode mode)
        {
            if (_heist != null)
                _heist.OnSafeHealthChanged -= Refresh;

            _heist = mode;
            gameObject.SetActive(mode != null);

            if (_heist != null)
            {
                _heist.OnSafeHealthChanged += Refresh;
                Refresh();
            }
        }

        public void Clear()
        {
            if (_heist != null)
                _heist.OnSafeHealthChanged -= Refresh;
            _heist = null;
        }

        private void Refresh()
        {
            if (_heist == null) return;

            int max = _heist.safeHealth > 0 ? _heist.safeHealth : 1;
            int blue = _heist.GetSafeHealth(GameManager.TEAM_BLUE);
            int red = _heist.GetSafeHealth(GameManager.TEAM_RED);
            int bluePct = Mathf.Clamp(Mathf.RoundToInt(100f * blue / max), 0, 100);
            int redPct = Mathf.Clamp(Mathf.RoundToInt(100f * red / max), 0, 100);

            if (blueSafeText != null)
            {
                blueSafeText.color = theme != null ? theme.teamBlue : Color.cyan;
                blueSafeText.text = BogatyriMoba.Localization.LocalizedUI.Format("heist.safe_hp", bluePct);
            }

            if (redSafeText != null)
            {
                redSafeText.color = theme != null ? theme.teamRed : Color.red;
                redSafeText.text = BogatyriMoba.Localization.LocalizedUI.Format("heist.safe_hp", redPct);
            }
        }

        private void OnDestroy() => Clear();
    }
}
