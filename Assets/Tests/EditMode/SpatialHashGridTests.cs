using NUnit.Framework;
using UnityEngine;
using BogatyriMoba.Core;

namespace BogatyriMoba.Tests
{
    // SpatialHashGrid is a pure-C# class; tests run without a scene.
    // BrawlerController is a MonoBehaviour so we create lightweight stand-ins via
    // new GameObject() — allowed in EditMode tests (destroyed in TearDown).

    [TestFixture]
    public class SpatialHashGridTests
    {
        private SpatialHashGrid _grid;
        private GameObject _goA, _goB, _goC;

        [SetUp]
        public void SetUp()
        {
            _grid = new SpatialHashGrid(4f);
            _goA = new GameObject("A");
            _goB = new GameObject("B");
            _goC = new GameObject("C");
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_goA);
            Object.DestroyImmediate(_goB);
            Object.DestroyImmediate(_goC);
        }

        private BrawlerController AddController(GameObject go, Vector2 pos)
        {
            var c = go.AddComponent<BrawlerController>();
            go.transform.position = new Vector3(pos.x, pos.y, 0);
            return c;
        }

        [Test]
        public void Insert_EntityAppearsInQuery()
        {
            var ca = AddController(_goA, Vector2.zero);
            _grid.Insert(ca);

            var results = _grid.Query(Vector2.zero, 5f);
            Assert.Contains(ca, results);
        }

        [Test]
        public void Query_ReturnsOnlyEntitiesWithinRadius()
        {
            var close = AddController(_goA, new Vector2(1f, 0f));
            var far   = AddController(_goB, new Vector2(50f, 0f));
            _grid.Insert(close);
            _grid.Insert(far);

            var results = _grid.Query(Vector2.zero, 10f);
            Assert.Contains(close, results);
            Assert.IsFalse(results.Contains(far));
        }

        [Test]
        public void Remove_EntityNoLongerAppearsInQuery()
        {
            var ca = AddController(_goA, Vector2.zero);
            _grid.Insert(ca);
            _grid.Remove(ca);

            var results = _grid.Query(Vector2.zero, 10f);
            Assert.IsFalse(results.Contains(ca));
        }

        [Test]
        public void UpdateEntity_TracksNewCellAfterMove()
        {
            var ca = AddController(_goA, Vector2.zero);
            _grid.Insert(ca);

            // Move entity far away and update
            _goA.transform.position = new Vector3(100f, 100f, 0);
            _grid.UpdateEntity(ca);

            var atOrigin = _grid.Query(Vector2.zero, 5f);
            var atNewPos = _grid.Query(new Vector2(100f, 100f), 5f);

            Assert.IsFalse(atOrigin.Contains(ca), "Entity should have left origin cell");
            Assert.Contains(ca, atNewPos);
        }

        [Test]
        public void Clear_RemovesAllEntities()
        {
            var ca = AddController(_goA, Vector2.zero);
            var cb = AddController(_goB, new Vector2(1f, 1f));
            _grid.Insert(ca);
            _grid.Insert(cb);
            _grid.Clear();

            var results = _grid.Query(Vector2.zero, 100f);
            Assert.AreEqual(0, results.Count);
        }

        [Test]
        public void FindNearest_ReturnsClosestMatchingFilter()
        {
            var ca = AddController(_goA, new Vector2(2f, 0f));
            var cb = AddController(_goB, new Vector2(5f, 0f));
            var cc = AddController(_goC, new Vector2(8f, 0f));
            ca.TeamId = 0;
            cb.TeamId = 1;
            cc.TeamId = 1;
            _grid.Insert(ca);
            _grid.Insert(cb);
            _grid.Insert(cc);

            // From origin, nearest enemy (TeamId != 0)
            var nearest = _grid.FindNearest(Vector2.zero, 20f, e => e.TeamId != 0);
            Assert.AreEqual(cb, nearest);
        }

        [Test]
        public void UpdateEntity_NoSpike_WhenCellUnchanged()
        {
            // Calling UpdateEntity many times without moving should not create duplicate entries
            var ca = AddController(_goA, new Vector2(1f, 1f));
            _grid.Insert(ca);
            for (int i = 0; i < 100; i++)
                _grid.UpdateEntity(ca);

            var results = _grid.Query(new Vector2(1f, 1f), 5f);
            int count = 0;
            foreach (var e in results)
                if (e == ca) count++;

            Assert.AreEqual(1, count, "Entity should appear exactly once even after many UpdateEntity calls");
        }

        [Test]
        public void GetKey_NegativeCoordinates_NoCollision()
        {
            // Insert entities at symmetric negative/positive positions and confirm no cross-contamination
            var ca = AddController(_goA, new Vector2(-10f, -10f));
            var cb = AddController(_goB, new Vector2(10f, 10f));
            _grid.Insert(ca);
            _grid.Insert(cb);

            var nearA = _grid.Query(new Vector2(-10f, -10f), 2f);
            var nearB = _grid.Query(new Vector2(10f, 10f), 2f);

            Assert.Contains(ca, nearA);
            Assert.IsFalse(nearA.Contains(cb));
            Assert.Contains(cb, nearB);
            Assert.IsFalse(nearB.Contains(ca));
        }
    }
}
