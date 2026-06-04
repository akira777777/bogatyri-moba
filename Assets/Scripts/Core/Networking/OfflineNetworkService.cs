using System;
using UnityEngine;

namespace BogatyriMoba.Core.Networking
{
    /// <summary>
    /// Offline / single-player implementation of INetworkService.
    /// All actions are applied immediately (local = server).
    /// </summary>
    public class OfflineNetworkService : INetworkService
    {
        public bool IsConnected => true;
        public bool IsInRoom => true;
        public bool IsServer => true;
        public int LocalPlayerId => 1;

        public event Action OnConnectedToMaster;
        public event Action OnJoinedRoom;
        public event Action OnLeftRoom;
        public event Action<int> OnPlayerEnteredRoom;
        public event Action<int> OnPlayerLeftRoom;
        public event Action<GameAction> OnGameActionReceived;

        private readonly ISpawnService _spawnService;

        public OfflineNetworkService(ISpawnService spawnService)
        {
            _spawnService = spawnService;
        }

        public void Connect()
        {
            Debug.Log("[OfflineNetworkService] Connected.");
            OnConnectedToMaster?.Invoke();
        }

        public void Disconnect()
        {
            OnLeftRoom?.Invoke();
        }

        public void CreateRoom(string roomName, byte? maxPlayers = null)
        {
            Debug.Log($"[OfflineNetworkService] Created room '{roomName}'.");
            OnJoinedRoom?.Invoke();
        }

        public void JoinRoom(string roomName)
        {
            Debug.Log($"[OfflineNetworkService] Joined room '{roomName}'.");
            OnJoinedRoom?.Invoke();
        }

        public void JoinRandomRoom()
        {
            Debug.Log("[OfflineNetworkService] Joined random room.");
            OnJoinedRoom?.Invoke();
        }

        public void LeaveRoom()
        {
            OnLeftRoom?.Invoke();
        }

        public void SpawnPlayer(BrawlerData data, int teamId, Vector3 position)
        {
            if (data == null)
            {
                Debug.LogError("[OfflineNetworkService] SpawnPlayer called with null BrawlerData.");
                return;
            }
            _spawnService?.SpawnPlayer(data, teamId, position);
        }

        /// <summary>
        /// In offline mode, broadcast the action locally so listeners can apply it.
        /// </summary>
        public void SendGameAction(GameAction action)
        {
            // Immediate local application — no server round-trip.
            OnGameActionReceived?.Invoke(action);

            // Also publish via EventBus for legacy subscribers.
            EventBus.Publish(new GameActionEvent(action));
        }
    }

    public struct GameActionEvent : IGameEvent
    {
        public GameAction Action;
        public GameActionEvent(GameAction action) => Action = action;
    }
}
