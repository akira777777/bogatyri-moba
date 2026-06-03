// Force recompile
using UnityEngine;
using System.Collections.Generic;
using BogatyriMoba.GameModes;
using BogatyriMoba.UI;
using BogatyriMoba.UI.Mobile;

namespace BogatyriMoba.Core
{
    /// <summary>
    /// Main game coordinator. Delegates spawning to SpawnManager,
    /// match state to MatchManager, and uses EventBus for decoupled communication.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Game Mode")]
        public GameMode currentGameMode;

        [Header("Camera")]
        public CameraFollow cameraFollow;

        [Header("UI")]
        public MatchHudController matchHud;
        public MatchTimerUI timerUI;
        public GameOverUI gameOverUI;
        public MobileControlsUI mobileControlsUI;

        public BrawlerController localPlayer { get; private set; }
        public IReadOnlyList<BrawlerController> AllPlayers =>
            SpawnManager.Instance != null
                ? SpawnManager.Instance.AllPlayers
                : System.Array.Empty<BrawlerController>();

        public IReadOnlyList<Gem> ActiveGems =>
            SpawnManager.Instance != null
                ? SpawnManager.Instance.ActiveGems
                : System.Array.Empty<Gem>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            BrawlerRegistry.Clear();
        }

        private void Start()
        {
            EventBus.Subscribe<PlayerSpawnedEvent>(OnPlayerSpawned);
            EventBus.Subscribe<MatchEndedEvent>(OnMatchEnded);

            if (MatchManager.Instance == null)
            {
                Debug.LogWarning("[GameManager] MatchManager not found. Adding one.");
                gameObject.AddComponent<MatchManager>();
            }
        }

        public void StartMatch(BrawlerData playerData)
        {
            if (SpawnManager.Instance == null)
            {
                Debug.LogError("[GameManager] SpawnManager is required!");
                return;
            }

            if (SpawnManager.Instance.team1SpawnPoints == null || SpawnManager.Instance.team1SpawnPoints.Length < 3 ||
                SpawnManager.Instance.team2SpawnPoints == null || SpawnManager.Instance.team2SpawnPoints.Length < 3)
            {
                Debug.LogError("[GameManager] Spawn points are not configured.");
                return;
            }

            // Clear previous
            SpawnManager.Instance.ClearAll();

            // Spawn local player
            Vector3 spawnPos = SpawnManager.Instance.team1SpawnPoints[1].position;
            localPlayer = SpawnManager.Instance.SpawnPlayer(playerData, TEAM_BLUE, spawnPos);
            if (localPlayer == null) return;

            if (cameraFollow != null)
                cameraFollow.SetTarget(localPlayer.transform);

            // Spawn bots
            var botPool = new List<string>(BotBrawlerKeys);
            botPool.RemoveAll(key => key.Equals(playerData.name, System.StringComparison.OrdinalIgnoreCase));

            for (int i = 0; i < 2; i++)
            {
                string key = botPool[i % botPool.Count];
                Vector3 pos = SpawnManager.Instance.team1SpawnPoints[i == 0 ? 0 : 2].position;
                SpawnManager.Instance.SpawnBot(key, TEAM_BLUE, pos);
            }

            for (int i = 0; i < 3; i++)
            {
                string key = botPool[(i + 2) % botPool.Count];
                Vector3 pos = SpawnManager.Instance.team2SpawnPoints[i].position;
                SpawnManager.Instance.SpawnBot(key, TEAM_RED, pos);
            }

            // Gems
            if (currentGameMode is GemGrabMode)
            {
                for (int i = 0; i < 3; i++)
                    SpawnManager.Instance.SpawnGem();
            }

            // Start match logic
            if (MatchManager.Instance != null)
            {
                MatchManager.Instance.currentGameMode = currentGameMode;
                MatchManager.Instance.StartMatch();
            }
        }

        private void OnPlayerSpawned(PlayerSpawnedEvent evt)
        {
            if (evt.IsLocal && cameraFollow != null)
                cameraFollow.SetTarget(evt.Player.transform);
        }

        private void OnMatchEnded(MatchEndedEvent evt)
        {
            // Show UI handled by MatchManager / UI layer
            Debug.Log($"[GameManager] Match ended. Winner: Team {evt.WinningTeamId}");
        }

        public void SpawnGem()
        {
            SpawnManager.Instance?.SpawnGem();
        }

        public void UnregisterGem(Gem gem)
        {
            SpawnManager.Instance?.UnregisterGem(gem);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<PlayerSpawnedEvent>(OnPlayerSpawned);
            EventBus.Unsubscribe<MatchEndedEvent>(OnMatchEnded);
            BrawlerRegistry.Clear();
        }

        public const int TEAM_BLUE = 0;
        public const int TEAM_RED = 1;

        private static readonly string[] BotBrawlerKeys =
        {
            "Alesha", "Dobrynya", "Ilya", "BabaYaga", "ZmeyGorynych",
            "Tugarin", "Varvara", "Knyaz", "Konyukh"
        };
    }
}
