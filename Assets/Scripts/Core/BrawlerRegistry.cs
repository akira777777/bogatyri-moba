using System.Collections.Generic;
using UnityEngine;

namespace BogatyriMoba.Core
{
    public static class BrawlerRegistry
    {
        private static readonly List<BrawlerController> Players = new List<BrawlerController>();

        public static IReadOnlyList<BrawlerController> AllPlayers => Players;

        public static void Register(BrawlerController brawler)
        {
            if (brawler != null && !Players.Contains(brawler))
                Players.Add(brawler);
        }

        public static void Unregister(BrawlerController brawler)
        {
            if (brawler != null)
                Players.Remove(brawler);
        }

        public static BrawlerController FindByActorNumber(int actorNumber)
        {
            for (int i = 0; i < Players.Count; i++)
            {
                if (Players[i] != null && Players[i].ActorNumber == actorNumber)
                    return Players[i];
            }
            return null;
        }

        public static BrawlerController FindNearestEnemy(
            Vector2 from,
            float maxRadiusSqr,
            BrawlerController self,
            int friendlyTeamId)
        {
            BrawlerController nearest = null;
            float nearestDistSqr = maxRadiusSqr;

            for (int i = 0; i < Players.Count; i++)
            {
                var p = Players[i];
                if (p == null || p == self || p.IsDead || p.TeamId == friendlyTeamId) continue;

                float d = Vector2.SqrMagnitude((Vector2)p.transform.position - from);
                if (d < nearestDistSqr)
                {
                    nearestDistSqr = d;
                    nearest = p;
                }
            }

            return nearest;
        }

        public static void Clear()
        {
            Players.Clear();
        }
    }
}
