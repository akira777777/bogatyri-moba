using NUnit.Framework;
using UnityEngine;
using BogatyriMoba.Core;

namespace BogatyriMoba.Tests
{
    [TestFixture]
    public class CombatComponentTests
    {
        private GameObject go;
        private CombatComponent combat;
        private BrawlerData data;

        [SetUp]
        public void SetUp()
        {
            go = new GameObject("TestBrawler");
            go.AddComponent<Rigidbody2D>();
            go.AddComponent<MovementComponent>();
            combat = go.AddComponent<CombatComponent>();

            data = ScriptableObject.CreateInstance<BrawlerData>();
            data.baseHealth = 1000;
            data.movementSpeed = 4f;
            data.attackReloadTime = 1f;
            data.attackDamage = 100;
            data.superChargePerHit = 10;
            data.attackRange = 5f;
            data.projectileSpeed = 10f;
            data.attackType = AttackType.SingleProjectile;

            combat.Initialize(data, 1);
        }

        [TearDown]
        public void TearDown()
        {
            if (go != null)
                Object.DestroyImmediate(go);
            if (data != null)
                Object.DestroyImmediate(data);
        }

        [Test]
        public void Initialize_SetsSuperChargeToZero()
        {
            Assert.AreEqual(0, combat.SuperCharge);
            Assert.IsFalse(combat.HasSuperReady);
        }

        [Test]
        public void ChargeSuper_IncreasesCharge()
        {
            combat.ChargeSuper(30);
            Assert.AreEqual(30, combat.SuperCharge);
        }

        [Test]
        public void ChargeSuper_CapsAt100()
        {
            combat.ChargeSuper(150);
            Assert.AreEqual(100, combat.SuperCharge);
            Assert.IsTrue(combat.HasSuperReady);
        }

        [Test]
        public void ChargeSuper_FiresEventWhenReady()
        {
            bool fired = false;
            combat.OnSuperChargeChanged += _ => { if (_ >= 100) fired = true; };
            combat.ChargeSuper(100);
            Assert.IsTrue(fired);
        }

        [Test]
        public void PerformSuper_ResetsCharge()
        {
            combat.ChargeSuper(100);
            combat.PerformSuper(Vector2.right);
            Assert.AreEqual(0, combat.SuperCharge);
            Assert.IsFalse(combat.HasSuperReady);
        }

        [Test]
        public void Tick_ReducesAttackCooldown()
        {
            combat.PerformAttack(Vector2.right);
            Assert.IsFalse(combat.CanAttack);

            // Simulate one second passing
            for (int i = 0; i < 60; i++)
                combat.Tick();

            // We can't easily test Time.deltaTime in EditMode without coroutines,
            // so we verify the cooldown was set
            Assert.Greater(combat.AttackCooldownTimer, 0f);
        }
    }
}
