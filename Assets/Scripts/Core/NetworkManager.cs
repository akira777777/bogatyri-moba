using UnityEngine;
using BogatyriMoba.Core.Networking;

namespace BogatyriMoba.Core
{
    /// <summary>
    /// Thin adapter over INetworkService. Preserves Instance for backward compatibility
    /// while delegating all transport logic to an injectable INetworkService.
    /// </summary>
    public class NetworkManager : MonoBehaviour
    {
        public static NetworkManager Instance { get; private set; }

        [Header("Network Settings")]
        public string gameVersion = "1.0";
        public byte maxPlayersPerRoom = 6;
        [Tooltip("If true, uses offline (single-player) network service.")]
        public bool useOfflineMode = true;

        public bool IsConnected => _service?.IsConnected ?? false;
        public bool IsInRoom => _service?.IsInRoom ?? false;
        public bool IsMasterClient => _service?.IsServer ?? true;

        private INetworkService _service;

        // Legacy events (preserved for backward compatibility)
        public System.Action OnConnectedToMaster;
        public System.Action OnJoinedRoom;
        public System.Action OnLeftRoom;
        public System.Action<int> OnPlayerEnteredRoom;
        public System.Action<int> OnPlayerLeftRoom;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Resolve or create network service
            _service = GameServices.Get<INetworkService>();
            if (_service == null)
            {
                var spawnService = GameServices.Get<ISpawnService>();
                _service = useOfflineMode
                    ? new OfflineNetworkService(spawnService)
                    : new NetcodeNetworkService(spawnService);
                GameServices.Register(_service);
            }

            WireEvents();
        }

        private void WireEvents()
        {
            _service.OnConnectedToMaster += () => OnConnectedToMaster?.Invoke();
            _service.OnJoinedRoom      += () => OnJoinedRoom?.Invoke();
            _service.OnLeftRoom        += () => OnLeftRoom?.Invoke();
            _service.OnPlayerEnteredRoom += id => OnPlayerEnteredRoom?.Invoke(id);
            _service.OnPlayerLeftRoom    += id => OnPlayerLeftRoom?.Invoke(id);
        }

        #region Connection & Room (delegated)

        public void Connect() => _service?.Connect();
        public void Disconnect() => _service?.Disconnect();
        public void CreateRoom(string roomName, byte? maxPlayers = null) => _service?.CreateRoom(roomName, maxPlayers);
        public void JoinRoom(string roomName) => _service?.JoinRoom(roomName);
        public void JoinRandomRoom() => _service?.JoinRandomRoom();
        public void LeaveRoom() => _service?.LeaveRoom();

        #endregion

        #region Spawning

        public void SpawnNetworkedPlayer(BrawlerData data, int teamId, Vector3 position)
        {
            _service?.SpawnPlayer(data, teamId, position);
        }

        #endregion

        #region Game Actions

        /// <summary>
        /// Broadcasts a game action through the network abstraction.
        /// </summary>
        public void SendGameAction(GameAction action) => _service?.SendGameAction(action);

        #endregion

        #region Legacy RPC Validation (deprecated, use ServerValidator directly)

        [System.Obsolete("Use ServerValidator.ValidateDamage instead.")]
        public bool ValidateDamageRPC(int attackerActorNumber, int victimActorNumber, int damage)
        {
            // Legacy stub — real validation should use ServerValidator
            return ServerValidator.ValidateDamage(damage, Vector3.zero, Vector3.zero, float.MaxValue);
        }

        [System.Obsolete("Use ServerValidator.ValidateSuper instead.")]
        public bool ValidateSuperRPC(int actorNumber) => true;

        [System.Obsolete("Use ServerValidator.ValidatePosition instead.")]
        public bool ValidatePositionRPC(int actorNumber, Vector3 position) => true;

        #endregion

        #region Legacy Networked Events (deprecated)

        [System.Obsolete("Use SendGameAction or EventBus instead.")]
        public void SendDamageEvent(int attackerActor, int victimActor, int damage, bool isSuper)
        {
            _service?.SendGameAction(new GameAction
            {
                Type = GameActionType.DamageApplied,
                ActorNumber = attackerActor,
                TargetActorNumber = victimActor,
                IntValue = damage
            });
        }

        [System.Obsolete("Use SendGameAction or EventBus instead.")]
        public void SendSuperEvent(int actorNumber, string abilityName)
        {
            _service?.SendGameAction(new GameAction
            {
                Type = GameActionType.SuperActivated,
                ActorNumber = actorNumber,
                StringValue = abilityName
            });
        }

        #endregion

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}
