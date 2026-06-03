using UnityEngine;

namespace BogatyriMoba.Core
{
    public static class AimHelper
    {
        public static Vector2 GetAimDirection(BrawlerController owner)
        {
            if (owner == null) return Vector2.right;

            var input = owner.GetComponent<PlayerInput>();
            if (input != null && input.AimDirection.sqrMagnitude > 0.01f)
                return input.AimDirection.normalized;

            return owner.GetFacingDirection();
        }
    }
}
