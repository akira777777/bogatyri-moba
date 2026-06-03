using UnityEngine;
using System.Collections.Generic;

namespace BogatyriMoba.Core.Ultimates
{
    [CreateAssetMenu(fileName = "BuffUltimate", menuName = "Bogatyri/Ultimates/Buff")]
    public class BuffUltimate : UltimateAbility
    {
        [Header("Buff Settings")]
        public float radius = 8.8f; // ~220px in world units
        public float speedMultiplier = 1.3f;
        public float damageMultiplier = 1.3f;
        public float duration = 5f;
        public GameObject buffZonePrefab;

        public override void Activate(BrawlerController owner)
        {
            Vector2 center = owner.transform.position;
            
            // Spawn zone visual
            GameObject zone = null;
            if (buffZonePrefab != null)
            {
                zone = Instantiate(buffZonePrefab, center, Quaternion.identity);
            }
            
            // Apply buff to allies in radius
            Collider2D[] hits = Physics2D.OverlapCircleAll(center, radius);
            foreach (var hit in hits)
            {
                var ally = hit.GetComponent<BrawlerController>();
                if (ally != null && ally.TeamId == owner.TeamId && !ally.IsDead)
                {
                    var buff = ally.gameObject.GetComponent<BuffComponent>();
                    if (buff == null)
                        buff = ally.gameObject.AddComponent<BuffComponent>();
                    
                    buff.Apply(speedMultiplier, damageMultiplier, duration);
                }
            }
            
            if (zone != null)
                Destroy(zone, duration);
        }
    }
}
