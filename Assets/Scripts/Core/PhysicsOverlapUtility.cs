using UnityEngine;

namespace BogatyriMoba.Core
{
    public static class PhysicsOverlapUtility
    {
        private static readonly Collider2D[] Buffer = new Collider2D[32];

        public static int OverlapCircle(Vector2 center, float radius, int layerMask = Physics2D.DefaultRaycastLayers)
        {
            return Physics2D.OverlapCircleNonAlloc(center, radius, Buffer, layerMask);
        }

        public static Collider2D GetHit(int index) => Buffer[index];
    }
}
