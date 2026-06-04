using System;
using UnityEngine;

namespace BogatyriMoba.Core.Networking
{
    /// <summary>
    /// Abstraction over the underlying networking transport (offline, Netcode, Photon, etc.).
    /// </summary>
    public interface INetworkService
    {
        bool IsConnected { get; }
        bool IsInRoom { get; }
        bool IsServer { get; }
        int LocalPlayerId { get; }

        void Connect();
        void Disconnect();
        void CreateRoom(string roomName, byte? maxPlayers = null);
        void JoinRoom(string roomName);
        void JoinRandomRoom();
        void LeaveRoom();

        void SpawnPlayer(BrawlerData data, int teamId, Vector3 position);
        void SendGameAction(GameAction action);

        event Action OnConnectedToMaster;
        event Action OnJoinedRoom;
        event Action OnLeftRoom;
        event Action<int> OnPlayerEnteredRoom;
        event Action<int> OnPlayerLeftRoom;
        event Action<GameAction> OnGameActionReceived;
    }

    /// <summary>
    /// Serializable game action for network replication.
    /// </summary>
    [Serializable]
    public struct GameAction
    {
        public GameActionType Type;
        public int ActorNumber;
        public int TargetActorNumber;
        public int IntValue;
        public Vector3 Position;
        public string StringValue;
    }

    public enum GameActionType
    {
        DamageRequest,
        DamageApplied,
        SuperActivated,
        PositionUpdate,
        GemCollected,
        PlayerDied,
        PlayerRespawned
    }
}
