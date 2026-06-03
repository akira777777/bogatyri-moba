using UnityEngine;
using System.Collections.Generic;
using BogatyriMoba.Core;

namespace BogatyriMoba.GameModes
{
    public class HeistMode : GameMode
    {
        [Header("Heist Settings")]
        public int safeHealth = 30000;
        public GameObject safePrefab;
        public Transform blueSafePosition;
        public Transform redSafePosition;

        private Dictionary<int, int> teamSafeHealth = new Dictionary<int, int>();
        private Dictionary<int, Safe> teamSafes = new Dictionary<int, Safe>();

        public override void Initialize()
        {
            base.Initialize();
            teamSafeHealth.Clear();
            teamSafes.Clear();

            for (int i = 0; i < teamCount; i++)
                teamSafeHealth[i] = safeHealth;

            SpawnSafes();
        }

        private void SpawnSafes()
        {
            if (safePrefab == null) return;

            if (blueSafePosition != null)
                teamSafes[GameManager.TEAM_BLUE] = CreateSafe(blueSafePosition.position, GameManager.TEAM_BLUE);

            if (redSafePosition != null)
                teamSafes[GameManager.TEAM_RED] = CreateSafe(redSafePosition.position, GameManager.TEAM_RED);
        }

        private Safe CreateSafe(Vector3 position, int teamId)
        {
            var go = Instantiate(safePrefab, position, Quaternion.identity);
            var safe = go.GetComponent<Safe>();
            if (safe == null)
                safe = go.AddComponent<Safe>();
            safe.Configure(teamId, this);
            return safe;
        }

        protected override void OnTimeExpired()
        {
            int winningTeam = -1;
            int maxHealth = -1;
            bool draw = false;

            foreach (var kvp in teamSafeHealth)
            {
                if (kvp.Value > maxHealth)
                {
                    maxHealth = kvp.Value;
                    winningTeam = kvp.Key;
                    draw = false;
                }
                else if (kvp.Value == maxHealth)
                {
                    draw = true;
                }
            }

            EndMatch(draw ? -1 : winningTeam);
        }

        public void DamageSafe(int teamId, int damage)
        {
            if (!matchActive || !teamSafeHealth.ContainsKey(teamId)) return;

            teamSafeHealth[teamId] -= damage;
            if (teamSafeHealth[teamId] <= 0)
            {
                teamSafeHealth[teamId] = 0;
                int otherTeam = teamId == GameManager.TEAM_BLUE ? GameManager.TEAM_RED : GameManager.TEAM_BLUE;
                EndMatch(otherTeam);
            }
        }

        public int GetSafeHealth(int teamId)
        {
            return teamSafeHealth.ContainsKey(teamId) ? teamSafeHealth[teamId] : 0;
        }

        public override void RegisterPlayer(BrawlerController player, int teamId)
        {
            player.TeamId = teamId;
        }

        public override void UnregisterPlayer(BrawlerController player)
        {
        }

        public override bool CanRespawn(BrawlerController player)
        {
            return matchActive;
        }

        public override Vector3 GetRespawnPosition(int teamId)
        {
            if (GameManager.Instance == null) return Vector3.zero;

            if (teamId == GameManager.TEAM_BLUE && GameManager.Instance.team1SpawnPoints != null &&
                GameManager.Instance.team1SpawnPoints.Length > 0)
                return GameManager.Instance.team1SpawnPoints[0].position;

            if (teamId == GameManager.TEAM_RED && GameManager.Instance.team2SpawnPoints != null &&
                GameManager.Instance.team2SpawnPoints.Length > 0)
                return GameManager.Instance.team2SpawnPoints[0].position;

            return Vector3.zero;
        }
    }
}
