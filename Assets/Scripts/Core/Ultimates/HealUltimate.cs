using UnityEngine;

namespace BogatyriMoba.Core.Ultimates
{
    [CreateAssetMenu(fileName = "HealUltimate", menuName = "Bogatyri/Ultimates/Heal")]
    public class HealUltimate : UltimateAbility
    {
        [Header("Heal Settings")]
        public float radius = 5f;
        public int healAmount = 1500;
        public GameObject healEffectPrefab;
        public GameObject zoneEffectPrefab;

        public override void Activate(BrawlerController owner)
        {
            Vector2 center = owner.transform.position;

            if (zoneEffectPrefab != null)
            {
                var zone = Object.Instantiate(zoneEffectPrefab, center, Quaternion.identity);
                Object.Destroy(zone, 1f);
            }

            int hitCount = PhysicsOverlapUtility.OverlapCircle(center, radius);
            for (int i = 0; i < hitCount; i++)
            {
                var ally = PhysicsOverlapUtility.GetHit(i).GetComponent<BrawlerController>();
                if (ally == null || ally.TeamId != owner.TeamId || ally.IsDead) continue;

                ally.Heal(healAmount);

                if (healEffectPrefab != null)
                {
                    var effect = Object.Instantiate(healEffectPrefab, ally.transform.position, Quaternion.identity);
                    Object.Destroy(effect, 0.6f);
                }
            }
        }
    }
}
