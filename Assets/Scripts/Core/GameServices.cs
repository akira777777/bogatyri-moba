using System;
using System.Collections.Generic;
using UnityEngine;

namespace BogatyriMoba.Core
{
    /// <summary>
    /// Lightweight service locator for decoupled dependency injection.
    /// Replaces direct singleton access (SpawnManager.Instance, etc.) with typed service resolution.
    /// </summary>
    public static class GameServices
    {
        private static readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        public static void Register<T>(T service) where T : class
        {
            _services[typeof(T)] = service;
        }

        public static T Get<T>() where T : class
        {
            if (_services.TryGetValue(typeof(T), out var service))
                return service as T;
            return null;
        }

        public static bool TryGet<T>(out T service) where T : class
        {
            if (_services.TryGetValue(typeof(T), out var obj))
            {
                service = obj as T;
                return service != null;
            }
            service = null;
            return false;
        }

        public static void Unregister<T>() where T : class
        {
            _services.Remove(typeof(T));
        }

        public static void Clear()
        {
            _services.Clear();
        }
    }

    /// <summary>
    /// Abstraction for spawning players, bots, gems, and objects.
    /// </summary>
    public interface ISpawnService
    {
        BrawlerController SpawnPlayer(BrawlerData data, int teamId, Vector3 position);
        BrawlerController SpawnBot(string brawlerKey, int teamId, Vector3 position);
        void SpawnGem();
        void UnregisterGem(Gem gem);
        void ClearAll();
        IReadOnlyList<BrawlerController> AllPlayers { get; }
        IReadOnlyList<Gem> ActiveGems { get; }
        Transform[] GetSpawnPoints(int teamId);
    }

    /// <summary>
    /// Abstraction for match timing and score state.
    /// </summary>
    public interface IMatchTimer
    {
        bool IsMatchActive { get; }
        float RemainingTime { get; }
        float ElapsedTime { get; }
        void AddScore(int teamId, int amount);
        int GetScore(int teamId);
    }

    /// <summary>
    /// Abstraction for the type-safe event bus.
    /// </summary>
    public interface IEventPublisher
    {
        void Subscribe<T>(Action<T> handler) where T : IGameEvent;
        void Unsubscribe<T>(Action<T> handler) where T : IGameEvent;
        void Publish<T>(T eventData) where T : IGameEvent;
    }

    /// <summary>
    /// Wrapper around static EventBus to allow mocking in tests.
    /// </summary>
    public class EventBusPublisher : IEventPublisher
    {
        public void Subscribe<T>(Action<T> handler) where T : IGameEvent => EventBus.Subscribe(handler);
        public void Unsubscribe<T>(Action<T> handler) where T : IGameEvent => EventBus.Unsubscribe(handler);
        public void Publish<T>(T eventData) where T : IGameEvent => EventBus.Publish(eventData);
    }
}
