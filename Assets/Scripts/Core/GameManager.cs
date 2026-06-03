using UnityEngine;
using System.Collections.Generic;
using BogatyriMoba.GameModes;
using BogatyriMoba.UI;

namespace BogatyriMoba.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Game Mode")]
        public GameMode currentGameMode;

        [Header("Spawn Points")]
        public Transform[] team1SpawnPoints;
        public Transform[] team2SpawnPoints;
        public Transform gemSpawnParent;

        [Header("Prefabs")]
        public GameObject brawlerPrefab;
        public GameObject gemPrefab;

        [Header("UI")]
        public MatchTimerUI timerUI;
        public GameObject gameOverPanel;
        public UnityEngine.UI.Text resultTitle;
        public UnityEngine.UI.Text resultText;

        [Header("Players")]
        public List<BrawlerController> allPlayers = new List<BrawlerController>();
        public BrawlerController localPlayer;

        [Header("Camera")]
        public CameraFollow cameraFollow;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            if (currentGameMode != null)
            {
                currentGameMode.Initialize();
                currentGameMode.OnMatchEnded += HandleMatchEnded;
            }
        }

        public void StartMatch(BrawlerData playerData)
        {
            // Spawn player
            Vector3 spawnPos = team1SpawnPoints[1].position;
            localPlayer = SpawnPlayer(playerData, TEAM_BLUE, spawnPos);
            cameraFollow.SetTarget(localPlayer.transform);

            // Spawn blue bots
            var bluePool = new List<string> { "alesha", "dobrynya", "ilya", "babyaga", "zmey", "tugarin", "varvara", "knyaz", "konyukh" };
            bluePool.Remove(playerData.name.ToLower().Replace(" ", "").Replace("-", ""));
            
            for (int i = 0; i < 2; i++)
            {
                string key = bluePool[i % bluePool.Count];
                SpawnBot(key, TEAM_BLUE, team1SpawnPoints[i == 0 ? 0 : 2].position);
            }

            // Spawn red bots
            for (int i = 0; i < 3; i++)
            {
                string key = bluePool[(i + 2) % bluePool.Count];
                SpawnBot(key, TEAM_RED, team2SpawnPoints[i].position);
            }

            // Spawn initial gems
            for (int i = 0; i < 3; i++)
            {
                SpawnGem();
            }

            if (timerUI != null)
                timerUI.SetGameMode(currentGameMode);
        }

        public BrawlerController SpawnPlayer(BrawlerData data, int teamId, Vector3 position)
        {
            GameObject go = Instantiate(brawlerPrefab, position, Quaternion.identity);
            var controller = go.GetComponent<BrawlerController>();
            if (controller == null)
            {
                Debug.LogError("Brawler prefab missing BrawlerController!");
                Destroy(go);
                return null;
            }

            controller.SetData(data);
            controller.TeamId = teamId;
            controller.ActorNumber = allPlayers.Count;
            controller.OnDeath += () => StartCoroutine(RespawnCoroutine(controller, teamId));

            allPlayers.Add(controller);
            currentGameMode?.RegisterPlayer(controller, teamId);

            return controller;
        }

        private void SpawnBot(string brawlerKey, int teamId, Vector3 position)
        {
            // Load brawler data from Resources or use default
            var data = Resources.Load<BrawlerData>("Brawlers/" + brawlerKey);
            if (data == null)
            {
                Debug.LogWarning($"Brawler data not found: {brawlerKey}");
                return;
            }

            var bot = SpawnPlayer(data, teamId, position);
            if (bot != null)
            {
                // Add AI component
                var ai = bot.gameObject.GetComponent<SimpleBotAI>();
                if (ai == null)
                    ai = bot.gameObject.AddComponent<SimpleBotAI>();
                ai.Initialize(bot);

                // Disable player input for bots
                var input = bot.GetComponent<PlayerInput>();
                if (input != null)
                    input.enabled = false;
            }
        }

        public void SpawnGem()
        {
            if (gemPrefab == null || gemSpawnParent == null) return;

            Vector3 pos = new Vector3(
                Random.Range(700f, 1100f),
                Random.Range(500f, 900f),
                0f
            );

            GameObject gem = Instantiate(gemPrefab, pos, Quaternion.identity, gemSpawnParent);
            var gemComponent = gem.GetComponent<Gem>();
            if (gemComponent != null)
                gemComponent.OnCollected += OnGemCollected;
        }

        private void OnGemCollected(BrawlerController collector)
        {
            var gemGrab = currentGameMode as GemGrabMode;
            if (gemGrab != null)
            {
                gemGrab.CollectGem(collector);
            }

            // Spawn replacement gem after delay
            Invoke(nameof(SpawnGem), 3f);
        }

        private System.Collections.IEnumerator RespawnCoroutine(BrawlerController player, int teamId)
        {
            yield return new WaitForSeconds(3f);

            if (currentGameMode != null && currentGameMode.CanRespawn(player))
            {
                Vector3 respawnPos = currentGameMode.GetRespawnPosition(teamId);
                player.Respawn(respawnPos);
            }
        }

        private void HandleMatchEnded(int winningTeam)
        {
            if (gameOverPanel != null)
                gameOverPanel.SetActive(true);

            bool won = winningTeam == TEAM_BLUE;
            if (resultTitle != null)
            {
                resultTitle.text = won ? "Победа!" : winningTeam == -1 ? "Ничья!" : "Поражение!";
                resultTitle.color = won ? new Color(0.3f, 0.67f, 0.97f) : winningTeam == -1 ? Color.gray : new Color(1f, 0.42f, 0.42f);
            }
            if (resultText != null)
            {
                string blueGems = currentGameMode is GemGrabMode gg ? gg.GetTeamGems(TEAM_BLUE).ToString() : "?";
                string redGems = currentGameMode is GemGrabMode gg2 ? gg2.GetTeamGems(TEAM_RED).ToString() : "?";
                resultText.text = won ? $"Синяя команда победила! {blueGems} 💎 vs {redGems} 💎" : $"Красная команда победила! {redGems} 💎 vs {blueGems} 💎";
            }
        }

        private void OnDestroy()
        {
            if (currentGameMode != null)
                currentGameMode.OnMatchEnded -= HandleMatchEnded;
        }

        private const int TEAM_BLUE = 0;
        private const int TEAM_RED = 1;
    }
}
