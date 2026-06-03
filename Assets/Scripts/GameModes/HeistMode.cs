using UnityEngine;
using System.Collections.Generic;
using BogatyriMoba.Core;

namespace BogatyriMoba.GameModes
{
    public class HeistMode : GameMode
    {
        [Header("Heist Settings")]
        public int safeHealth = 30000;
        public Transform[] safePositions; // one per team
        
        private Dictionary<int, int> teamSafeHealth = new Dictionary<int, int>();
        private Dictionary<int, GameObject> teamSafes = new Dictionary<int, GameObject>();

        public override void Initialize()
        {
            base.Initialize();
            teamSafeHealth.Clear();
            
            for (int i = 0; i < teamCount; i++)
            {
                teamSafeHealth[i] = safeHealth;
            }
        }

        protected override void OnTimeExpired()
        {
            // Time over: team with more safe health wins
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
                // Other team wins
                int otherTeam = teamId == 0 ? 1 : 0;
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
            return Vector3.zero; // TODO: spawn points
        }
    }
}
