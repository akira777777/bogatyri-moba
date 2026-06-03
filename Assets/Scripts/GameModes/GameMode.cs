using UnityEngine;
using BogatyriMoba.Core;

namespace BogatyriMoba.GameModes
{
    public enum GameModeType
    {
        GemGrab,
        Heist,
        Showdown,
        Bounty,
        BrawlBall
    }

    public abstract class GameMode : MonoBehaviour
    {
        [Header("Base Settings")]
        public GameModeType modeType;
        public float matchDuration = 180f;
        public int teamCount = 2;
        public int playersPerTeam = 3;

        protected float timer;
        protected bool matchActive;

        public System.Action<int> OnMatchEnded;
        public System.Action<float> OnTimerChanged;

        public virtual void Initialize()
        {
            timer = matchDuration;
            matchActive = true;
        }

        protected virtual void Update()
        {
            if (!matchActive) return;

            timer -= Time.deltaTime;
            OnTimerChanged?.Invoke(timer);

            if (timer <= 0)
            {
                timer = 0;
                OnTimeExpired();
            }
        }

        protected abstract void OnTimeExpired();
        public abstract void RegisterPlayer(BrawlerController player, int teamId);
        public abstract void UnregisterPlayer(BrawlerController player);
        public abstract bool CanRespawn(BrawlerController player);
        public abstract Vector3 GetRespawnPosition(int teamId);

        protected void EndMatch(int winningTeamId)
        {
            matchActive = false;
            OnMatchEnded?.Invoke(winningTeamId);
        }
    }
}
