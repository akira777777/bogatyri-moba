using UnityEngine;
using System.Collections;

namespace BogatyriMoba.Core.Ultimates
{
    [CreateAssetMenu(fileName = "FireBreathUltimate", menuName = "Bogatyri/Ultimates/FireBreath")]
    public class FireBreathUltimate : UltimateAbility
    {
        [Header("Fire Breath Settings")]
        public float coneAngle = 0.8f; // radians (half-angle)
        public float range = 160f;
        public int damagePerTick = 400;
        public float tickRate = 0.3f;
        public float duration = 2f;
        public GameObject fireEffectPrefab;

        public override void Activate(BrawlerController owner)
        {
            owner.StartCoroutine(FireBreathRoutine(owner));
        }

        private IEnumerator FireBreathRoutine(BrawlerController owner)
        {
            float timer = 0f;
            float tickTimer = 0f;
            
            while (timer < duration)
            {
                float dt = Time.deltaTime;
                timer += dt;
                tickTimer += dt;
                
                Vector2 aimDir = GetAimDirection(owner);
                float baseAngle = Mathf.Atan2(aimDir.y, aimDir.x);
                
                // Spawn visual effect
                if (fireEffectPrefab != null && Random.value < 0.5f)
                {
                    float dist = Random.Range(20f, range);
                    float spread = Random.Range(-coneAngle, coneAngle);
                    Vector2 pos = (Vector2)owner.transform.position + new Vector2(
                        Mathf.Cos(baseAngle + spread) * dist,
                        Mathf.Sin(baseAngle + spread) * dist
                    );
                    var effect = Instantiate(fireEffectPrefab, pos, Quaternion.identity);
                    Destroy(effect, 0.5f);
                }
                
                // Damage tick
                if (tickTimer >= tickRate)
                {
                    tickTimer = 0f;
                    DamageEnemiesInCone(owner, baseAngle);
                }
                
                yield return null;
            }
        }

        private void DamageEnemiesInCone(BrawlerController owner, float baseAngle)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(owner.transform.position, range);
            foreach (var hit in hits)
            {
                var enemy = hit.GetComponent<BrawlerController>();
                if (enemy == null || enemy.TeamId == owner.TeamId || enemy.IsDead) continue;
                
                Vector2 toEnemy = enemy.transform.position - owner.transform.position;
                float enemyAngle = Mathf.Atan2(toEnemy.y, toEnemy.x);
                float angleDiff = Mathf.Abs(Mathf.DeltaAngle(baseAngle * Mathf.Rad2Deg, enemyAngle * Mathf.Rad2Deg) * Mathf.Deg2Rad);
                
                if (angleDiff < coneAngle)
                {
                    enemy.TakeDamage(damagePerTick);
                }
            }
        }

        private Vector2 GetAimDirection(BrawlerController owner)
        {
            var input = owner.GetComponent<PlayerInput>();
            if (input != null && input.AimDirection != Vector2.zero)
                return input.AimDirection.normalized;
            
            float dirX = owner.transform.localScale.x >= 0 ? 1f : -1f;
            return new Vector2(dirX, 0f);
        }
    }
}
