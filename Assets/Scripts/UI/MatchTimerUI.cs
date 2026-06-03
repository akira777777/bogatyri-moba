using UnityEngine;
using TMPro;
using BogatyriMoba.GameModes;

namespace BogatyriMoba.UI
{
    public class MatchTimerUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private GameMode gameMode;

        private void Start()
        {
            if (gameMode != null)
            {
                gameMode.OnTimerChanged += UpdateTimer;
            }
        }

        private void UpdateTimer(float remainingTime)
        {
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
