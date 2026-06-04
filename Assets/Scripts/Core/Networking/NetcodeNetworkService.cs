using System;
using UnityEngine;

namespace BogatyriMoba.Core.Networking
{
    /// <summary>
    /// Stub implementation for Unity Netcode for GameObjects.
    /// To be wired up once Netcode package is installed.
    /// </summary>
    public class NetcodeNetworkService : INetworkService
    {
        public bool IsConnected { get; private set; }
        public bool IsInRoom { get; private set; }
        public bool IsServer { get; private set; }
        public int LocalPlayerId { get; private set; } = -1;

        public event Action OnConnectedToMaster;
        public event Action OnJoinedRoom;
        public event Action OnLeftRoom;
        public event Action<int> OnPlayerEnteredRoom;
        public event Action<int> OnPlayerLeftRoom;
        public event Action<GameAction> OnGameActionReceived;

        private readonly ISpawnService _spawnService;

        public NetcodeNetworkService(ISpawnService spawnService)
        {
            _spawnService = spawnService;
        }

        public void Connect()
        {
            Debug.Log("[NetcodeNetworkService] Connect() — stub. Install Unity Netcode package to enable.");
            IsConnected = true;
            OnConnectedToMaster?.Invoke();
        }

        public void Disconnect()
        {
            IsConnected = false;
            IsInRoom = false;
            OnLeftRoom?.Invoke();
        }

        public void CreateRoom(string roomName, byte? maxPlayers = null)
        {
            Debug.Log($"[NetcodeNetworkService] CreateRoom('{roomName}') — stub.");
            IsInRoom = true;
            OnJoinedRoom?.Invoke();
        }

        public void JoinRoom(string roomName)
        {
            Debug.Log($"[NetcodeNetworkService] JoinRoom('{roomName}') — stub.");
            IsInRoom = true;
            OnJoinedRoom?.Invoke();
        }

        public void JoinRandomRoom()
        {
            Debug.Log("[NetcodeNetworkService] JoinRandomRoom() — stub.");
            IsInRoom = true;
            OnJoinedRoom?.Invoke();
        }

        public void LeaveRoom()
        {
            IsInRoom = false;
            OnLeftRoom?.Invoke();
        }

        public void SpawnPlayer(BrawlerData data, int teamId, Vector3 position)
        {
            if (data == null)
            {
                Debug.LogError("[NetcodeNetworkService] SpawnPlayer called with null BrawlerData.");
                return;
            }
            Debug.Log($"[NetcodeNetworkService] SpawnPlayer — stub. Should call NetworkManager.Spawn.");
            _spawnService?.SpawnPlayer(data, teamId, position);
        }

        public void SendGameAction(GameAction action)
        {
            Debug.Log($"[NetcodeNetworkService] SendGameAction {action.Type} — stub.");
            // TODO: wire to Unity Netcode custom messaging or RPCs.
        }
    }
}
