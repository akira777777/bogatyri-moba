using System.Collections.Generic;
using UnityEngine;

namespace BogatyriMoba.Core
{
    /// <summary>
    /// Abstraction for brawler lookup and spatial queries.
    /// Allows mocking in tests.
    /// </summary>
    public interface IBrawlerRegistry
    {
        IReadOnlyList<BrawlerController> AllPlayers { get; }
        void Register(BrawlerController brawler);
        void Unregister(BrawlerController brawler);
        void Clear();
        BrawlerController FindByActorNumber(int actorNumber);
        BrawlerController FindNearestEnemy(Vector2 from, float maxRadiusSqr, BrawlerController self, int friendlyTeamId);
    }

    /// <summary>
    /// Default implementation wrapping the static BrawlerRegistry.
    /// </summary>
    public class BrawlerRegistryWrapper : IBrawlerRegistry
    {
        public IReadOnlyList<BrawlerController> AllPlayers => BrawlerRegistry.AllPlayers;

        public void Register(BrawlerController brawler) => BrawlerRegistry.Register(brawler);
        public void Unregister(BrawlerController brawler) => BrawlerRegistry.Unregister(brawler);
        public void Clear() => BrawlerRegistry.Clear();
        public BrawlerController FindByActorNumber(int actorNumber) => BrawlerRegistry.FindByActorNumber(actorNumber);
        public BrawlerController FindNearestEnemy(Vector2 from, float maxRadiusSqr, BrawlerController self, int friendlyTeamId)
            => BrawlerRegistry.FindNearestEnemy(from, maxRadiusSqr, self, friendlyTeamId);
    }
}
