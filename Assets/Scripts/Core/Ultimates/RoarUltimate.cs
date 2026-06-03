using UnityEngine;

namespace BogatyriMoba.Core.Ultimates
{
    [CreateAssetMenu(fileName = "RoarUltimate", menuName = "Bogatyri/Ultimates/Roar")]
    public class RoarUltimate : UltimateAbility
    {
        [Header("Roar Settings")]
        public float radius = 5f;
        public float stunDuration = 1.5f;
        public int damage = 500;
        public int shieldAmount = 2000;
        public float shieldDuration = 4f;
        public GameObject roarEffectPrefab;

        public override void Activate(BrawlerController owner)
        {
            owner.ApplyShield(shieldAmount, shieldDuration);

            int hitCount = PhysicsOverlapUtility.OverlapCircle(owner.transform.position, radius);
            for (int i = 0; i < hitCount; i++)
            {
                var enemy = PhysicsOverlapUtility.GetHit(i).GetComponent<BrawlerController>();
                if (enemy == null || enemy.TeamId == owner.TeamId || enemy.IsDead) continue;

                enemy.ApplyStun(stunDuration);
                enemy.TakeDamage(damage);
            }

            if (roarEffectPrefab != null)
            {
                var effect = Object.Instantiate(roarEffectPrefab, owner.transform.position, Quaternion.identity);
                Object.Destroy(effect, 1f);
            }
        }
    }
}
