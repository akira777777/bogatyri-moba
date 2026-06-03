using UnityEngine;
using UnityEngine.UI;
using BogatyriMoba.GameModes;

namespace BogatyriMoba.UI
{
    public class MatchTimerUI : MonoBehaviour
    {
        [SerializeField] private Text timerText;
        [SerializeField] private GameMode gameMode;

        public void SetGameMode(GameMode mode)
        {
            if (gameMode != null)
                gameMode.OnTimerChanged -= UpdateTimer;

            gameMode = mode;
            if (gameMode != null)
            {
                gameMode.OnTimerChanged += UpdateTimer;
                UpdateTimer(mode.matchDuration);
            }
        }

        private void Awake()
        {
            if (timerText == null)
                timerText = GetComponent<Text>();
        }

        private void Start()
        {
            if (gameMode != null)
                gameMode.OnTimerChanged += UpdateTimer;
        }

        private void UpdateTimer(float remainingTime)
        {
            if (timerText == null) return;

            int minutes = Mathf.FloorToInt(remainingTime / 60f);
            int seconds = Mathf.FloorToInt(remainingTime % 60f);
            timerText.text = $"{minutes:00}:{seconds:00}";
        }

        private void OnDestroy()
        {
            if (gameMode != null)
                gameMode.OnTimerChanged -= UpdateTimer;
        }
    }
}
