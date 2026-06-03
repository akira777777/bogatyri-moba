using System;
using System.Collections.Generic;
using UnityEngine;

namespace BogatyriMoba.Core
{
    /// <summary>
    /// Type-safe event bus for decoupled communication between systems.
    /// Replaces direct Action delegates for cross-system events.
    /// </summary>
    public static class EventBus
    {
        private static readonly Dictionary<Type, Delegate> _handlers = new Dictionary<Type, Delegate>();
        private static readonly Dictionary<Type, List<Delegate>> _onceHandlers = new Dictionary<Type, List<Delegate>>();

        public static void Subscribe<T>(Action<T> handler) where T : IGameEvent
        {
            var type = typeof(T);
            if (_handlers.TryGetValue(type, out var existing))
                _handlers[type] = Delegate.Combine(existing, handler);
            else
                _handlers[type] = handler;
        }

        public static void Unsubscribe<T>(Action<T> handler) where T : IGameEvent
        {
            var type = typeof(T);
            if (_handlers.TryGetValue(type, out var existing))
            {
                _handlers[type] = Delegate.Remove(existing, handler);
                if (_handlers[type] == null)
                    _handlers.Remove(type);
            }
        }

        public static void SubscribeOnce<T>(Action<T> handler) where T : IGameEvent
        {
            var type = typeof(T);
            if (!_onceHandlers.TryGetValue(type, out var list))
            {
                list = new List<Delegate>();
                _onceHandlers[type] = list;
            }
            list.Add(handler);
        }

        public static void Publish<T>(T eventData) where T : IGameEvent
        {
            var type = typeof(T);

            if (_handlers.TryGetValue(type, out var handler))
            {
                // Invoke each subscriber independently so a throwing handler doesn't kill the rest
                foreach (var invocation in handler.GetInvocationList())
                {
                    try
                    {
                        ((Action<T>)invocation).Invoke(eventData);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[EventBus] Error publishing {type.Name}: {ex}");
                    }
                }
            }

            if (_onceHandlers.TryGetValue(type, out var onceList))
            {
                foreach (var once in onceList)
                {
                    try
                    {
                        ((Action<T>)once)?.Invoke(eventData);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[EventBus] Error in once-handler for {type.Name}: {ex}");
                    }
                }
                onceList.Clear();
            }
        }

        public static void Clear()
        {
            _handlers.Clear();
            _onceHandlers.Clear();
        }
    }

    public interface IGameEvent { }

    // Core gameplay events
    public struct PlayerSpawnedEvent : IGameEvent
    {
        public BrawlerController Player;
        public int TeamId;
        public bool IsLocal;
    }

    public struct PlayerDeathEvent : IGameEvent
    {
        public BrawlerController Player;
        public BrawlerController Killer;
        public int TeamId;
    }

    public struct PlayerRespawnedEvent : IGameEvent
    {
        public BrawlerController Player;
        public Vector3 Position;
    }

    public struct DamageDealtEvent : IGameEvent
    {
        public BrawlerController Attacker;
        public BrawlerController Victim;
        public int Damage;
        public bool IsSuper;
    }

    public struct SuperActivatedEvent : IGameEvent
    {
        public BrawlerController Player;
        public string AbilityName;
    }

    public struct GemCollectedEvent : IGameEvent
    {
        public BrawlerController Player;
        public int TeamId;
        public int TotalGems;
    }

    public struct GemDroppedEvent : IGameEvent
    {
        public Vector3 Position;
        public int GemCount;
    }

    public struct MatchStartedEvent : IGameEvent
    {
        public GameModeType Mode;
        public float Duration;
    }

    public struct MatchEndedEvent : IGameEvent
    {
        public int WinningTeamId;
        public GameModeType Mode;
    }

    public struct TimerUpdatedEvent : IGameEvent
    {
        public float RemainingTime;
    }

    public struct ScoreUpdatedEvent : IGameEvent
    {
        public int TeamId;
        public int NewScore;
    }
}
