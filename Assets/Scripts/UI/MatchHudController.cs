using UnityEngine;
using BogatyriMoba.GameModes;

namespace BogatyriMoba.UI
{
    public class MatchHudController : MonoBehaviour
    {
        [SerializeField] private MatchTimerUI timerUI;
        [SerializeField] private TeamScoreUI teamScoreUI;
        [SerializeField] private HeistSafeHudUI heistSafeHudUI;

        public MatchTimerUI TimerUI => timerUI;

        public void BindGameMode(GameMode mode)
        {
            if (timerUI != null)
                timerUI.SetGameMode(mode);

            if (teamScoreUI != null)
            {
                teamScoreUI.Clear();
                teamScoreUI.gameObject.SetActive(mode is GemGrabMode);
                if (mode is GemGrabMode gemGrab)
                    teamScoreUI.BindGemGrabMode(gemGrab);
            }

            if (heistSafeHudUI != null)
            {
                heistSafeHudUI.Clear();
                if (mode is HeistMode heist)
                    heistSafeHudUI.BindHeistMode(heist);
                else
                    heistSafeHudUI.gameObject.SetActive(false);
            }
        }
    }
}
