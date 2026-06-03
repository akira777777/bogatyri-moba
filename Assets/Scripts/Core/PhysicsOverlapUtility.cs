using System.Collections.Generic;
using UnityEngine;

namespace BogatyriMoba.Core
{
    public static class PhysicsOverlapUtility
    {
        private static readonly List<Collider2D> Results = new List<Collider2D>(32);

        public static int OverlapCircle(Vector2 center, float radius, int layerMask = Physics2D.DefaultRaycastLayers)
        {
            Results.Clear();
            var filter = new ContactFilter2D();
            filter.SetLayerMask(layerMask);
            filter.useTriggers = true;
            return Physics2D.OverlapCircle(center, radius, filter, Results);
        }

        public static Collider2D GetHit(int index) => Results[index];
    }
}
