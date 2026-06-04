using NUnit.Framework;
using UnityEngine;
using BogatyriMoba.Core.Networking;

namespace BogatyriMoba.Tests
{
    [TestFixture]
    public class ServerValidatorTests
    {
        #region ValidateDamage

        [Test]
        public void ValidateDamage_Valid_ReturnsTrue()
        {
            bool result = ServerValidator.ValidateDamage(
                100, Vector3.zero, new Vector3(3f, 0f, 0f), 10f);
            Assert.IsTrue(result);
        }

        [Test]
        public void ValidateDamage_NegativeDamage_ReturnsFalse()
        {
            bool result = ServerValidator.ValidateDamage(
                -1, Vector3.zero, Vector3.zero, 10f);
            Assert.IsFalse(result);
        }

        [Test]
        public void ValidateDamage_ExcessiveDamage_ReturnsFalse()
        {
            bool result = ServerValidator.ValidateDamage(
                6000, Vector3.zero, Vector3.zero, 10f);
            Assert.IsFalse(result);
        }

        [Test]
        public void ValidateDamage_TooFar_ReturnsFalse()
        {
            bool result = ServerValidator.ValidateDamage(
                100, Vector3.zero, new Vector3(100f, 0f, 0f), 1f);
            Assert.IsFalse(result);
        }

        #endregion

        #region ValidatePosition

        [Test]
        public void ValidatePosition_ValidMovement_ReturnsTrue()
        {
            bool result = ServerValidator.ValidatePosition(
                new Vector3(5f, 0f, 0f), Vector3.zero, 1f);
            Assert.IsTrue(result);
        }

        [Test]
        public void ValidatePosition_TooFast_ReturnsFalse()
        {
            // 100 units in 0.1s => 1000 speed, way above MAX_SPEED (30)
            bool result = ServerValidator.ValidatePosition(
                new Vector3(100f, 0f, 0f), Vector3.zero, 0.1f);
            Assert.IsFalse(result);
        }

        [Test]
        public void ValidatePosition_ZeroDeltaTime_ReturnsTrue()
        {
            bool result = ServerValidator.ValidatePosition(
                new Vector3(999f, 0f, 0f), Vector3.zero, 0f);
            Assert.IsTrue(result);
        }

        #endregion

        #region ValidateSuper

        [Test]
        public void ValidateSuper_ReadyAndNotStunned_ReturnsTrue()
        {
            bool result = ServerValidator.ValidateSuper(100, false);
            Assert.IsTrue(result);
        }

        [Test]
        public void ValidateSuper_NotEnoughCharge_ReturnsFalse()
        {
            bool result = ServerValidator.ValidateSuper(50, false);
            Assert.IsFalse(result);
        }

        [Test]
        public void ValidateSuper_Stunned_ReturnsFalse()
        {
            bool result = ServerValidator.ValidateSuper(100, true);
            Assert.IsFalse(result);
        }

        [Test]
        public void ValidateSuper_CustomRequiredCharge_ReturnsFalse()
        {
            bool result = ServerValidator.ValidateSuper(75, false, 80);
            Assert.IsFalse(result);
        }

        [Test]
        public void ValidateSuper_CustomRequiredCharge_ReturnsTrue()
        {
            bool result = ServerValidator.ValidateSuper(80, false, 80);
            Assert.IsTrue(result);
        }

        #endregion
    }
}
