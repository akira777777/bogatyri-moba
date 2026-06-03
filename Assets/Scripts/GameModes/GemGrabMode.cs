using UnityEngine;
using System.Collections.Generic;
using BogatyriMoba.Core;

namespace BogatyriMoba.GameModes
{
    public class GemGrabMode : GameMode
    {
        [Header("Gem Grab Settings")]
        public int gemsToWin = 10;
        public float gemSpawnInterval = 5f;
        public Transform[] gemSpawnPoints;
        public GameObject gemPrefab;

        private Dictionary<int, int> teamGems = new Dictionary<int, int>();
        private Dictionary<BrawlerController, int> playerGems = new Dictionary<BrawlerController, int>();
        private float gemSpawnTimer;

        public event System.Action OnTeamGemsChanged;

        public override void Initialize()
        {
            base.Initialize();
            teamGems.Clear();
            playerGems.Clear();
            gemSpawnTimer = gemSpawnInterval;

            for (int i = 0; i < teamCount; i++)
            {
                teamGems[i] = 0;
            }
        }

        protected override void Update()
        {
            base.Update();
            if (!matchActive) return;

            gemSpawnTimer -= Time.deltaTime;
            if (gemSpawnTimer <= 0)
            {
                GameManager.Instance?.SpawnGem();
                gemSpawnTimer = gemSpawnInterval;
            }
        }

        protected override void OnTimeExpired()
        {
            int winningTeam = -1;
            int maxGems = -1;
            bool draw = false;

            foreach (var kvp in teamGems)
            {
                if (kvp.Value > maxGems)
                {
                    maxGems = kvp.Value;
                    winningTeam = kvp.Key;
                    draw = false;
                }
                else if (kvp.Value == maxGems)
                {
                    draw = true;
                }
            }

            EndMatch(draw ? -1 : winningTeam);
        }

        public override void RegisterPlayer(BrawlerController player, int teamId)
        {
            player.TeamId = teamId;
            player.OnDeath += () => OnPlayerDeath(player);
            if (!playerGems.ContainsKey(player))
                playerGems[player] = 0;
        }

        public override void UnregisterPlayer(BrawlerController player)
        {
            if (playerGems.ContainsKey(player) && playerGems[player] > 0)
            {
                // Drop gems on disconnect
                for (int i = 0; i < playerGems[player]; i++)
                {
                    GameManager.Instance?.SpawnGem();
                }
                playerGems.Remove(player);
            }
        }

        public void CollectGem(BrawlerController player)
        {
            if (!matchActive || !playerGems.ContainsKey(player)) return;

            playerGems[player]++;
            teamGems[player.TeamId]++;
            OnTeamGemsChanged?.Invoke();

            // Check win
            if (teamGems[player.TeamId] >= gemsToWin)
            {
                EndMatch(player.TeamId);
            }
        }

        private void OnPlayerDeath(BrawlerController player)
        {
            if (!playerGems.ContainsKey(player)) return;

            int dropped = playerGems[player];
            if (dropped > 0)
            {
                teamGems[player.TeamId] -= dropped;
                playerGems[player] = 0;
                OnTeamGemsChanged?.Invoke();

                // Spawn dropped gems
                for (int i = 0; i < dropped; i++)
                {
                    GameManager.Instance?.SpawnGem();
                }
            }
        }

        public override bool CanRespawn(BrawlerController player)
        {
            return matchActive;
        }

        public override Vector3 GetRespawnPosition(int teamId)
        {
            if (teamId == 0 && GameManager.Instance != null && GameManager.Instance.team1SpawnPoints.Length > 0)
                return GameManager.Instance.team1SpawnPoints[0].position;
            if (teamId == 1 && GameManager.Instance != null && GameManager.Instance.team2SpawnPoints.Length > 0)
                return GameManager.Instance.team2SpawnPoints[0].position;
            return Vector3.zero;
        }

        public int GetTeamGems(int teamId)
        {
            return teamGems.ContainsKey(teamId) ? teamGems[teamId] : 0;
        }

        public int GetPlayerGems(BrawlerController player)
        {
            return playerGems.ContainsKey(player) ? playerGems[player] : 0;
        }
    }
}
