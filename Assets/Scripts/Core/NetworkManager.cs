using UnityEngine;
using System.Collections.Generic;

namespace BogatyriMoba.Core
{
    /// <summary>
    /// Network abstraction layer for Photon PUN 2.
    /// Handles connection, room management, player instantiation, and RPC validation.
    /// </summary>
    public class NetworkManager : MonoBehaviour
    {
        public static NetworkManager Instance { get; private set; }

        [Header("Photon Settings")]
        public string gameVersion = "1.0";
        public byte maxPlayersPerRoom = 6;

        [Header("Prefabs")]
        public GameObject networkedBrawlerPrefab;

        public bool IsConnected => _isConnected;
        public bool IsInRoom => _isInRoom;
        public bool IsMasterClient => _isMasterClient;

        private bool _isConnected;
        private bool _isInRoom;
        private bool _isMasterClient;
        private Dictionary<int, BrawlerController> _networkedPlayers = new Dictionary<int, BrawlerController>();

        // Events
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
        }

        #region Connection

        public void Connect()
        {
            if (_isConnected) return;

            Debug.Log("[NetworkManager] Connecting to Photon...");
            // PhotonNetwork.GameVersion = gameVersion;
            // PhotonNetwork.ConnectUsingSettings();

            // Stub for compilation without Photon DLL
            SimulateConnection();
        }

        public void Disconnect()
        {
            // PhotonNetwork.Disconnect();
            _isConnected = false;
            _isInRoom = false;
        }

        private void SimulateConnection()
        {
            // Offline fallback for testing without Photon
            _isConnected = true;
            OnConnectedToMaster?.Invoke();
            Debug.Log("[NetworkManager] Connected (offline mode).");
        }

        #endregion

        #region Room Management

        public void CreateRoom(string roomName, byte? maxPlayers = null)
        {
            if (!_isConnected)
            {
                Debug.LogWarning("[NetworkManager] Not connected!");
                return;
            }

            byte players = maxPlayers ?? maxPlayersPerRoom;
            Debug.Log($"[NetworkManager] Creating room: {roomName} (max {players} players)");
            // PhotonNetwork.CreateRoom(roomName, new RoomOptions { MaxPlayers = players });

            // Offline simulation
            _isInRoom = true;
            _isMasterClient = true;
            OnJoinedRoom?.Invoke();
        }

        public void JoinRoom(string roomName)
        {
            if (!_isConnected) return;
            Debug.Log($"[NetworkManager] Joining room: {roomName}");
            // PhotonNetwork.JoinRoom(roomName);

            _isInRoom = true;
            _isMasterClient = false;
            OnJoinedRoom?.Invoke();
        }

        public void JoinRandomRoom()
        {
            if (!_isConnected) return;
            Debug.Log("[NetworkManager] Joining random room...");
            // PhotonNetwork.JoinRandomRoom();

            _isInRoom = true;
            _isMasterClient = false;
            OnJoinedRoom?.Invoke();
        }

        public void LeaveRoom()
        {
            if (!_isInRoom) return;
            // PhotonNetwork.LeaveRoom();
            _isInRoom = false;
            _isMasterClient = false;
            OnLeftRoom?.Invoke();
        }

        #endregion

        #region Player Spawning

        public void SpawnNetworkedPlayer(BrawlerData data, int teamId, Vector3 position)
        {
            if (!_isInRoom)
            {
                Debug.LogWarning("[NetworkManager] Not in room, spawning local only.");
                SpawnLocalPlayer(data, teamId, position);
                return;
            }

            // PhotonNetwork.Instantiate(networkedBrawlerPrefab.name, position, Quaternion.identity);
            // For now, local spawn with network component stub
            var player = SpawnLocalPlayer(data, teamId, position);
            if (player != null)
            {
                // int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
                // player.ActorNumber = actorNumber;
                // _networkedPlayers[actorNumber] = player;
            }
        }

        private BrawlerController SpawnLocalPlayer(BrawlerData data, int teamId, Vector3 position)
        {
            if (SpawnManager.Instance != null)
            {
                return SpawnManager.Instance.SpawnPlayer(data, teamId, position);
            }
            return null;
        }

        #endregion

        #region RPC Validation (Server Authority)

        /// <summary>
        /// Validates if an RPC call is legitimate from the sender.
        /// Prevents cheating by verifying sender identity and state.
        /// </summary>
        public bool ValidateDamageRPC(int attackerActorNumber, int victimActorNumber, int damage)
        {
            if (!_networkedPlayers.TryGetValue(attackerActorNumber, out var attacker))
                return false;
            if (!_networkedPlayers.TryGetValue(victimActorNumber, out var victim))
                return false;
            if (attacker.IsDead || victim.IsDead)
                return false;
            if (attacker.TeamId == victim.TeamId)
                return false;
            if (damage < 0 || damage > 5000)
                return false;

            float dist = Vector2.Distance(attacker.transform.position, victim.transform.position);
            float maxRange = attacker.GetData().attackRange * 1.5f;
            if (dist > maxRange)
            {
                Debug.LogWarning($"[NetworkManager] Rejected damage RPC: distance {dist:F2} > max {maxRange:F2}");
                return false;
            }

            return true;
        }

        public bool ValidateSuperRPC(int actorNumber)
        {
            if (!_networkedPlayers.TryGetValue(actorNumber, out var player))
                return false;
            if (!player.HasSuperReady)
            {
                Debug.LogWarning($"[NetworkManager] Rejected super RPC: player {actorNumber} super not ready.");
                return false;
            }
            return true;
        }

        public bool ValidatePositionRPC(int actorNumber, Vector3 position)
        {
            if (!_networkedPlayers.TryGetValue(actorNumber, out var player))
                return false;

            float dist = Vector2.Distance(player.transform.position, position);
            float maxTeleport = player.GetData().movementSpeed * Time.deltaTime * 3f;
            if (dist > maxTeleport)
            {
                Debug.LogWarning($"[NetworkManager] Rejected position RPC: teleport {dist:F2} > max {maxTeleport:F2}");
                return false;
            }
            return true;
        }

        #endregion

        #region Networked Events

        [System.Obsolete("Use Photon RPCs in production")]
        public void SendDamageEvent(int attackerActor, int victimActor, int damage, bool isSuper)
        {
            if (!_isInRoom) return;
            // PhotonView.Get(this).RPC("ReceiveDamageRPC", RpcTarget.All, attackerActor, victimActor, damage, isSuper);
        }

        [System.Obsolete("Use Photon RPCs in production")]
        public void SendSuperEvent(int actorNumber, string abilityName)
        {
            if (!_isInRoom) return;
            // PhotonView.Get(this).RPC("ReceiveSuperRPC", RpcTarget.All, actorNumber, abilityName);
        }

        #endregion

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}
