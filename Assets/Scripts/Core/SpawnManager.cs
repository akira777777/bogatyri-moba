using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;

namespace BogatyriMoba.Core
{
    /// <summary>
    /// Handles spawning of players, bots, gems, and objects.
    /// Uses object pooling where possible.
    /// </summary>
    public class SpawnManager : MonoBehaviour, ISpawnService
    {
        public static SpawnManager Instance { get; private set; }

        [Header("Spawn Points")]
        public Transform[] team1SpawnPoints;
        public Transform[] team2SpawnPoints;
        public Transform gemSpawnParent;

        [Header("Prefabs (legacy direct references)")]
        public GameObject brawlerPrefab;
        public GameObject gemPrefab;

        [Header("Addressable References (future-proof)")]
        [SerializeField] private AssetReferenceGameObject brawlerAssetRef;
        [SerializeField] private AssetReferenceGameObject gemAssetRef;

        [Header("Bot Settings")]
        private static readonly string[] BotBrawlerKeys =
        {
            "Alesha", "Dobrynya", "Ilya", "BabaYaga", "ZmeyGorynych",
            "Tugarin", "Varvara", "Knyaz", "Konyukh"
        };

        private readonly List<BrawlerController> _allPlayers = new List<BrawlerController>();
        private readonly List<Gem> _activeGems = new List<Gem>();
        private readonly Dictionary<string, BrawlerData> _brawlerCache = new Dictionary<string, BrawlerData>();

        public IReadOnlyList<BrawlerController> AllPlayers => _allPlayers;
        public IReadOnlyList<Gem> ActiveGems => _activeGems;

        private int _nextActorNumber = 0;

        private GameObject BrawlerPrefab => brawlerPrefab != null ? brawlerPrefab : (brawlerAssetRef?.Asset as GameObject);
        private GameObject GemPrefab => gemPrefab != null ? gemPrefab : (gemAssetRef?.Asset as GameObject);

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            BrawlerRegistry.Clear();
            GameServices.Register<ISpawnService>(this);
            if (GameServices.Get<IBrawlerRegistry>() == null)
                GameServices.Register<IBrawlerRegistry>(new BrawlerRegistryWrapper());
        }

        public BrawlerController SpawnPlayer(BrawlerData data, int teamId, Vector3 position)
        {
            if (BrawlerPrefab == null)
            {
                Debug.LogError("[SpawnManager] Brawler prefab is not assigned!");
                return null;
            }

            GameObject go = Instantiate(BrawlerPrefab, position, Quaternion.identity);
            var controller = go.GetComponent<BrawlerController>();
            if (controller == null)
            {
                Debug.LogError("[SpawnManager] Brawler prefab missing BrawlerController!");
                Destroy(go);
                return null;
            }

            controller.SetData(data);
            controller.TeamId = teamId;
            controller.ActorNumber = _nextActorNumber++;
            controller.OnDeath += () => HandlePlayerDeath(controller, teamId);

            _allPlayers.Add(controller);
            BrawlerRegistry.Register(controller);

            EventBus.Publish(new PlayerSpawnedEvent
            {
                Player = controller,
                TeamId = teamId,
                IsLocal = GameManager.Instance?.localPlayer == controller
            });

            return controller;
        }

        public BrawlerController SpawnBot(string brawlerKey, int teamId, Vector3 position)
        {
            var data = LoadBrawlerData(brawlerKey);
            if (data == null)
            {
                Debug.LogWarning($"[SpawnManager] Brawler data not found: Brawlers/{brawlerKey}");
                return null;
            }

            var bot = SpawnPlayer(data, teamId, position);
            if (bot == null) return null;

            var legacyAi = bot.GetComponent<SimpleBotAI>();
            if (legacyAi != null)
                Destroy(legacyAi);

            GameObject botGO = bot.gameObject;
            var ai = botGO.GetComponent<OptimizedBotAI>();
            if (ai == null)
                ai = botGO.AddComponent<OptimizedBotAI>();
            ai.Initialize(bot);

            var input = bot.GetComponent<PlayerInput>();
            if (input != null)
                input.enabled = false;

            return bot;
        }

        private BrawlerData LoadBrawlerData(string brawlerKey)
        {
            if (_brawlerCache.TryGetValue(brawlerKey, out var cached) && cached != null)
                return cached;

            var data = AssetLoader.Default.LoadAsset<BrawlerData>("Brawlers/" + brawlerKey);
            if (data != null)
                _brawlerCache[brawlerKey] = data;
            return data;
        }

        public void SpawnGem()
        {
            if (GemPrefab == null || gemSpawnParent == null) return;

            Vector2 offset = Random.insideUnitCircle * 3f;
            Vector3 pos = gemSpawnParent.position + new Vector3(offset.x, offset.y, 0f);

            GameObject gemObject = Instantiate(GemPrefab, pos, Quaternion.identity, gemSpawnParent);
            var gemComponent = gemObject.GetComponent<Gem>();
            if (gemComponent == null) return;

            gemComponent.OnCollected += HandleGemCollected;
            _activeGems.Add(gemComponent);
        }

        public void UnregisterGem(Gem gem)
        {
            if (gem != null)
                _activeGems.Remove(gem);
        }

        public void ClearAll()
        {
            foreach (var p in _allPlayers)
            {
                if (p != null)
                    Destroy(p.gameObject);
            }
            _allPlayers.Clear();

            foreach (var g in _activeGems)
            {
                if (g != null)
                    Destroy(g.gameObject);
            }
            _activeGems.Clear();

            BrawlerRegistry.Clear();
            _brawlerCache.Clear();
            _nextActorNumber = 0;
        }

        private void HandlePlayerDeath(BrawlerController player, int teamId)
        {
            if (MatchManager.Instance != null && MatchManager.Instance.currentGameMode != null)
            {
                if (MatchManager.Instance.currentGameMode.CanRespawn(player))
                {
                    StartCoroutine(RespawnCoroutine(player, teamId));
                }
            }
        }

        private System.Collections.IEnumerator RespawnCoroutine(BrawlerController player, int teamId)
        {
            yield return new WaitForSeconds(3f);

            if (MatchManager.Instance != null && MatchManager.Instance.currentGameMode != null)
            {
                Vector3 respawnPos = MatchManager.Instance.currentGameMode.GetRespawnPosition(teamId);
                player.Respawn(respawnPos);

                EventBus.Publish(new PlayerRespawnedEvent
                {
                    Player = player,
                    Position = respawnPos
                });
            }
        }

        private void HandleGemCollected(BrawlerController collector)
        {
            EventBus.Publish(new GemCollectedEvent
            {
                Player = collector,
                TeamId = collector.TeamId,
                TotalGems = 0
            });
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
            GameServices.Unregister<ISpawnService>();
            BrawlerRegistry.Clear();
        }

        public Transform[] GetSpawnPoints(int teamId)
        {
            return teamId == GameManager.TEAM_BLUE ? team1SpawnPoints : team2SpawnPoints;
        }
    }
}
