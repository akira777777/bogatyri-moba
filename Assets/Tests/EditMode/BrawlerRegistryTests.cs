using NUnit.Framework;
using UnityEngine;
using BogatyriMoba.Core;

namespace BogatyriMoba.Tests
{
    [TestFixture]
    public class BrawlerRegistryTests
    {
        private GameObject _goA, _goB;

        [SetUp]
        public void SetUp()
        {
            BrawlerRegistry.Clear();
            _goA = new GameObject("A");
            _goB = new GameObject("B");
        }

        [TearDown]
        public void TearDown()
        {
            BrawlerRegistry.Clear();
            Object.DestroyImmediate(_goA);
            Object.DestroyImmediate(_goB);
        }

        [Test]
        public void Register_AddsToAllPlayers()
        {
            var ca = _goA.AddComponent<BrawlerController>();
            BrawlerRegistry.Register(ca);
            Assert.AreEqual(1, BrawlerRegistry.AllPlayers.Count);
            Assert.AreEqual(ca, BrawlerRegistry.AllPlayers[0]);
        }

        [Test]
        public void Register_NoDuplicates()
        {
            var ca = _goA.AddComponent<BrawlerController>();
            BrawlerRegistry.Register(ca);
            BrawlerRegistry.Register(ca);
            Assert.AreEqual(1, BrawlerRegistry.AllPlayers.Count);
        }

        [Test]
        public void Unregister_RemovesEntity()
        {
            var ca = _goA.AddComponent<BrawlerController>();
            BrawlerRegistry.Register(ca);
            BrawlerRegistry.Unregister(ca);
            Assert.AreEqual(0, BrawlerRegistry.AllPlayers.Count);
        }

        [Test]
        public void FindByActorNumber_ReturnsCorrectEntity()
        {
            var ca = _goA.AddComponent<BrawlerController>();
            var cb = _goB.AddComponent<BrawlerController>();
            ca.ActorNumber = 42;
            cb.ActorNumber = 7;
            BrawlerRegistry.Register(ca);
            BrawlerRegistry.Register(cb);

            Assert.AreEqual(ca, BrawlerRegistry.FindByActorNumber(42));
            Assert.AreEqual(cb, BrawlerRegistry.FindByActorNumber(7));
        }

        [Test]
        public void FindByActorNumber_ReturnsNull_WhenNotFound()
        {
            Assert.IsNull(BrawlerRegistry.FindByActorNumber(999));
        }

        [Test]
        public void Clear_RemovesAll()
        {
            var ca = _goA.AddComponent<BrawlerController>();
            BrawlerRegistry.Register(ca);
            BrawlerRegistry.Clear();
            Assert.AreEqual(0, BrawlerRegistry.AllPlayers.Count);
        }

        [Test]
        public void Register_Null_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => BrawlerRegistry.Register(null));
        }
    }
}
