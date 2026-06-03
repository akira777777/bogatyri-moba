using TMPro;
using UnityEngine;
using BogatyriMoba.GameModes;
using BogatyriMoba.Core;

namespace BogatyriMoba.UI
{
    public class TeamScoreUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI blueScoreText;
        [SerializeField] private TextMeshProUGUI redScoreText;
        [SerializeField] private UITheme theme;

        private GemGrabMode _gemGrab;

        private void Awake()
        {
            if (theme == null)
                theme = UITheme.LoadDefault();
            ApplyColors();
        }

        public void BindGemGrabMode(GemGrabMode mode)
        {
            if (_gemGrab != null)
                _gemGrab.OnTeamGemsChanged -= RefreshScores;

            _gemGrab = mode;
            if (_gemGrab != null)
            {
                _gemGrab.OnTeamGemsChanged += RefreshScores;
                RefreshScores();
            }
        }

        public void Clear()
        {
            if (_gemGrab != null)
                _gemGrab.OnTeamGemsChanged -= RefreshScores;
            _gemGrab = null;
        }

        private void RefreshScores()
        {
            if (_gemGrab == null) return;
            if (blueScoreText != null)
                blueScoreText.text = _gemGrab.GetTeamGems(GameManager.TEAM_BLUE).ToString();
            if (redScoreText != null)
                redScoreText.text = _gemGrab.GetTeamGems(GameManager.TEAM_RED).ToString();
        }

        private void ApplyColors()
        {
            if (theme == null) return;
            if (blueScoreText != null)
                blueScoreText.color = theme.teamBlue;
            if (redScoreText != null)
                redScoreText.color = theme.teamRed;
        }

        private void OnDestroy() => Clear();
    }
}
