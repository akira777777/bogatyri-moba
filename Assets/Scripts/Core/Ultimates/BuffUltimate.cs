using UnityEngine;

namespace BogatyriMoba.Core.Ultimates
{
    [CreateAssetMenu(fileName = "BuffUltimate", menuName = "Bogatyri/Ultimates/Buff")]
    public class BuffUltimate : UltimateAbility
    {
        [Header("Buff Settings")]
        public float radius = 5.5f;
        public float speedMultiplier = 1.3f;
        public float damageMultiplier = 1.3f;
        public float duration = 5f;
        public GameObject buffZonePrefab;

        public override void Activate(BrawlerController owner)
        {
            Vector2 center = owner.transform.position;

            GameObject zone = null;
            if (buffZonePrefab != null)
                zone = Object.Instantiate(buffZonePrefab, center, Quaternion.identity);

            int hitCount = PhysicsOverlapUtility.OverlapCircle(center, radius);
            for (int i = 0; i < hitCount; i++)
            {
                var ally = PhysicsOverlapUtility.GetHit(i).GetComponent<BrawlerController>();
                if (ally == null || ally.TeamId != owner.TeamId || ally.IsDead) continue;

                var buff = ally.gameObject.GetComponent<BuffComponent>();
                if (buff == null)
                    buff = ally.gameObject.AddComponent<BuffComponent>();

                buff.Apply(speedMultiplier, damageMultiplier, duration);
            }

            if (zone != null)
                Object.Destroy(zone, duration);
        }
    }
}
