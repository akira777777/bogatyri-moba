using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using BogatyriMoba.Core;
using BogatyriMoba.Localization;

namespace BogatyriMoba.UI
{
    public class GameOverUI : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI bodyText;
        [SerializeField] private Button playAgainButton;
        [SerializeField] private TextMeshProUGUI playAgainLabel;
        [SerializeField] private UITheme theme;

        private void Awake()
        {
            if (theme == null)
                theme = UITheme.LoadDefault();

            if (panelRoot != null)
                panelRoot.SetActive(false);

            if (playAgainButton != null)
                playAgainButton.onClick.AddListener(ReloadScene);

            RefreshPlayAgainLabel();
        }

        public void Show(int winningTeam, int blueGems, int redGems)
        {
            if (panelRoot != null)
                panelRoot.SetActive(true);

            bool isDraw = winningTeam == -1;
            bool localWon = winningTeam == GameManager.TEAM_BLUE;

            if (titleText != null)
            {
                if (isDraw)
                {
                    titleText.text = LocalizedUI.Get("gameover.title_draw");
                    titleText.color = theme != null ? theme.neutralGray : Color.gray;
                }
                else if (localWon)
                {
                    titleText.text = LocalizedUI.Get("gameover.title_win");
                    titleText.color = theme != null ? theme.teamBlue : Color.cyan;
                }
                else
                {
                    titleText.text = LocalizedUI.Get("gameover.title_loss");
                    titleText.color = theme != null ? theme.teamRed : Color.red;
                }
            }

            if (bodyText != null)
            {
                string bodyKey = isDraw
                    ? "gameover.body_draw"
                    : localWon
                        ? "gameover.body_win"
                        : "gameover.body_loss";

                bodyText.text = LocalizedUI.Get(bodyKey)
                    .Replace("{blue}", blueGems.ToString())
                    .Replace("{red}", redGems.ToString());
                bodyText.color = Color.white;
            }
        }

        public void Hide()
        {
            if (panelRoot != null)
                panelRoot.SetActive(false);
        }

        private void RefreshPlayAgainLabel()
        {
            if (playAgainLabel != null)
                playAgainLabel.text = LocalizedUI.Get("gameover.play_again");
        }

        private void ReloadScene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
