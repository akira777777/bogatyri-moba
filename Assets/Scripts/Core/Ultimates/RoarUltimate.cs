using UnityEngine;
using System.Collections.Generic;

namespace BogatyriMoba.Core.Ultimates
{
    [CreateAssetMenu(fileName = "RoarUltimate", menuName = "Bogatyri/Ultimates/Roar")]
    public class RoarUltimate : UltimateAbility
    {
        [Header("Roar Settings")]
        public float radius = 180f;
        public float stunDuration = 1.5f;
        public int damage = 500;
        public int shieldAmount = 2000;
        public float shieldDuration = 4f;
        public GameObject roarEffectPrefab;

        public override void Activate(BrawlerController owner)
        {
            // Apply shield to self
            owner.ApplyShield(shieldAmount, shieldDuration);

            // Stun and damage nearby enemies
            Collider2D[] hits = Physics2D.OverlapCircleAll(owner.transform.position, radius);
            foreach (var hit in hits)
            {
                var enemy = hit.GetComponent<BrawlerController>();
                if (enemy != null && enemy.TeamId != owner.TeamId && !enemy.IsDead)
                {
                    enemy.ApplyStun(stunDuration);
                    enemy.TakeDamage(damage);
                }
            }

            // Spawn visual effect
            if (roarEffectPrefab != null)
            {
                var effect = Instantiate(roarEffectPrefab, owner.transform.position, Quaternion.identity);
                Destroy(effect, 1f);
            }
        }
    }
}
