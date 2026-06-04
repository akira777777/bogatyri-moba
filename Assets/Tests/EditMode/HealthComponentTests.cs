using NUnit.Framework;
using UnityEngine;
using BogatyriMoba.Core;

namespace BogatyriMoba.Tests
{
    [TestFixture]
    public class HealthComponentTests
    {
        private GameObject go;
        private HealthComponent health;

        [SetUp]
        public void SetUp()
        {
            go = new GameObject("TestBrawler");
            go.AddComponent<Rigidbody2D>();
            health = go.AddComponent<HealthComponent>();
            health.Initialize(1000);
        }

        [TearDown]
        public void TearDown()
        {
            if (go != null)
                Object.DestroyImmediate(go);
        }

        [Test]
        public void Initialize_SetsMaxAndCurrentHealth()
        {
            Assert.AreEqual(1000, health.MaxHealth);
            Assert.AreEqual(1000, health.CurrentHealth);
            Assert.IsFalse(health.IsDead);
        }

        [Test]
        public void TakeDamage_ReducesHealth()
        {
            health.TakeDamage(200);
            Assert.AreEqual(800, health.CurrentHealth);
        }

        [Test]
        public void TakeDamage_ToZero_Kills()
        {
            health.TakeDamage(1000);
            Assert.AreEqual(0, health.CurrentHealth);
            Assert.IsTrue(health.IsDead);
        }

        [Test]
        public void TakeDamage_WhenDead_DoesNothing()
        {
            health.TakeDamage(1000);
            health.TakeDamage(100);
            Assert.AreEqual(0, health.CurrentHealth);
        }

        [Test]
        public void Heal_IncreasesHealth()
        {
            health.TakeDamage(400);
            health.Heal(200);
            Assert.AreEqual(800, health.CurrentHealth);
        }

        [Test]
        public void Heal_DoesNotExceedMax()
        {
            health.Heal(500);
            Assert.AreEqual(1000, health.CurrentHealth);
        }

        [Test]
        public void ApplyShield_AbsorbsDamage()
        {
            health.ApplyShield(300, 5f);
            health.TakeDamage(200);
            Assert.AreEqual(1000, health.CurrentHealth);
        }

        [Test]
        public void ApplyShield_PartialAbsorption()
        {
            health.ApplyShield(100, 5f);
            health.TakeDamage(250);
            Assert.AreEqual(850, health.CurrentHealth);
        }

        [Test]
        public void ApplyStun_SetsIsStunned()
        {
            health.ApplyStun(1f);
            Assert.IsTrue(health.IsStunned);
        }

        [Test]
        public void StunnedTarget_TakesBonusDamage()
        {
            health.ApplyStun(1f);
            health.TakeDamage(100);
            // 100 * 1.2 = 120
            Assert.AreEqual(880, health.CurrentHealth);
        }

        [Test]
        public void Respawn_ResetsHealthAndState()
        {
            health.TakeDamage(1000);
            health.Respawn(Vector3.zero, 1000);
            Assert.IsFalse(health.IsDead);
            Assert.AreEqual(1000, health.CurrentHealth);
            Assert.IsFalse(health.IsStunned);
        }

        [Test]
        public void OnHealthChanged_FiresOnDamage()
        {
            int current = -1, max = -1;
            health.OnHealthChanged += (c, m) => { current = c; max = m; };
            health.TakeDamage(100);
            Assert.AreEqual(900, current);
            Assert.AreEqual(1000, max);
        }

        [Test]
        public void OnDeath_FiresWhenKilled()
        {
            bool died = false;
            health.OnDeath += () => died = true;
            health.TakeDamage(1000);
            Assert.IsTrue(died);
        }
    }
}
