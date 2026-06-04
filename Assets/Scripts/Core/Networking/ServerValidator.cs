using UnityEngine;

namespace BogatyriMoba.Core.Networking
{
    /// <summary>
    /// Server-side validation for game actions. In single-player/offline mode
    /// this runs locally. In multiplayer it should run only on the server/host.
    /// </summary>
    public static class ServerValidator
    {
        private const float MAX_DAMAGE_PER_HIT = 5000f;
        private const float MAX_SPEED = 30f;
        private const float POSITION_THRESHOLD = 5f;

        /// <summary>
        /// Validates a damage request. Returns true if the damage is within acceptable bounds.
        /// </summary>
        public static bool ValidateDamage(int damage, Vector3 attackerPos, Vector3 targetPos, float attackerMaxRange)
        {
            if (damage < 0 || damage > MAX_DAMAGE_PER_HIT)
            {
                Debug.LogWarning($"[ServerValidator] Damage {damage} out of bounds.");
                return false;
            }

            float distance = Vector3.Distance(attackerPos, targetPos);
            if (distance > attackerMaxRange * POSITION_THRESHOLD)
            {
                Debug.LogWarning($"[ServerValidator] Distance {distance:F2} exceeds allowed range ({attackerMaxRange * POSITION_THRESHOLD:F2}).");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates a position update from a client.
        /// </summary>
        public static bool ValidatePosition(Vector3 newPosition, Vector3 previousPosition, float deltaTime)
        {
            if (deltaTime <= 0f) return true; // First frame or paused

            float distance = Vector3.Distance(newPosition, previousPosition);
            float maxDistance = MAX_SPEED * deltaTime;

            if (distance > maxDistance)
            {
                Debug.LogWarning($"[ServerValidator] Movement too fast: {distance:F2} in {deltaTime:F3}s (max {maxDistance:F2}).");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates super ability usage (cooldown, charge threshold).
        /// </summary>
        public static bool ValidateSuper(float currentCharge, bool isStunned, int requiredCharge = 100)
        {
            if (isStunned)
            {
                Debug.LogWarning("[ServerValidator] Super blocked — player is stunned.");
                return false;
            }

            if (currentCharge < requiredCharge)
            {
                Debug.LogWarning($"[ServerValidator] Super blocked — charge {currentCharge}/{requiredCharge}.");
                return false;
            }

            return true;
        }
    }
}
