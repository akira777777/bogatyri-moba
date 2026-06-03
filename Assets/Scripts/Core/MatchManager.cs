using UnityEngine;
using System.Collections.Generic;
using BogatyriMoba.GameModes;

namespace BogatyriMoba.Core
{
    /// <summary>
    /// Handles match lifecycle: start, end, timer, score tracking.
    /// Decoupled from spawning and player management.
    /// </summary>
    public class MatchManager : MonoBehaviour
    {
        public static MatchManager Instance { get; private set; }

        [Header("Settings")]
        public GameMode currentGameMode;
        public bool autoStartOnAwake = true;

        [Header("Teams")]
        public int[] teamScores = new int[2];

        public bool IsMatchActive { get; private set; }
        public float ElapsedTime { get; private set; }
        public float RemainingTime => currentGameMode != null ? currentGameMode.matchDuration - ElapsedTime : 0f;

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
            if (autoStartOnAwake)
                StartMatch();
        }

        public void StartMatch()
        {
            if (currentGameMode == null)
            {
                Debug.LogError("[MatchManager] No game mode assigned!");
                return;
            }

            IsMatchActive = true;
            ElapsedTime = 0f;
            teamScores[0] = 0;
            teamScores[1] = 0;

            currentGameMode.Initialize();
            currentGameMode.OnMatchEnded += HandleMatchEnded;
            currentGameMode.OnTimerChanged += HandleTimerChanged;

            EventBus.Publish(new MatchStartedEvent
            {
                Mode = currentGameMode.modeType,
                Duration = currentGameMode.matchDuration
            });

            Debug.Log($"[MatchManager] Match started: {currentGameMode.modeType}");
        }

        public void EndMatch(int winningTeamId)
        {
            if (!IsMatchActive) return;
            IsMatchActive = false;

            if (currentGameMode != null)
            {
                currentGameMode.OnMatchEnded -= HandleMatchEnded;
                currentGameMode.OnTimerChanged -= HandleTimerChanged;
            }

            EventBus.Publish(new MatchEndedEvent
            {
                WinningTeamId = winningTeamId,
                Mode = currentGameMode?.modeType ?? GameModeType.GemGrab
            });
        }

        public void AddScore(int teamId, int amount)
        {
            if (teamId < 0 || teamId >= teamScores.Length) return;
            teamScores[teamId] += amount;
            EventBus.Publish(new ScoreUpdatedEvent { TeamId = teamId, NewScore = teamScores[teamId] });
        }

        public int GetScore(int teamId)
        {
            if (teamId < 0 || teamId >= teamScores.Length) return 0;
            return teamScores[teamId];
        }

        private void Update()
        {
            if (!IsMatchActive) return;
            ElapsedTime += Time.deltaTime;
        }

        private void HandleMatchEnded(int winningTeamId)
        {
            EndMatch(winningTeamId);
        }

        private void HandleTimerChanged(float remaining)
        {
            EventBus.Publish(new TimerUpdatedEvent { RemainingTime = remaining });
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}
