using System.Collections.Generic;

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

        public static void Clear()
        {
            Players.Clear();
        }
    }
}
