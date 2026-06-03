using UnityEngine;

namespace BogatyriMoba.Core.Ultimates
{
    [CreateAssetMenu(fileName = "HealUltimate", menuName = "Bogatyri/Ultimates/Heal")]
    public class HealUltimate : UltimateAbility
    {
        [Header("Heal Settings")]
        public float radius = 8f;
        public int healAmount = 1500;
        public GameObject healEffectPrefab;
        public GameObject zoneEffectPrefab;

        public override void Activate(BrawlerController owner)
        {
            Vector2 center = owner.transform.position;
            
            // Show zone visual
            if (zoneEffectPrefab != null)
            {
                var zone = Instantiate(zoneEffectPrefab, center, Quaternion.identity);
                Destroy(zone, 1f);
            }
            
            Collider2D[] hits = Physics2D.OverlapCircleAll(center, radius);
            foreach (var hit in hits)
            {
                var ally = hit.GetComponent<BrawlerController>();
                if (ally != null && ally.TeamId == owner.TeamId && !ally.IsDead)
                {
                    ally.Heal(healAmount);
                    
                    if (healEffectPrefab != null)
                    {
                        var effect = Instantiate(healEffectPrefab, ally.transform.position, Quaternion.identity);
                        Destroy(effect, 0.6f);
                    }
                }
            }
        }
    }
}
