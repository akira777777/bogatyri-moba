using UnityEngine;
using System.Collections;

namespace BogatyriMoba.Core.Ultimates
{
    [CreateAssetMenu(fileName = "FireBreathUltimate", menuName = "Bogatyri/Ultimates/FireBreath")]
    public class FireBreathUltimate : UltimateAbility
    {
        [Header("Fire Breath Settings")]
        public float coneAngle = 0.8f;
        public float range = 6f;
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

                Vector2 aimDir = AimHelper.GetAimDirection(owner);
                float baseAngle = Mathf.Atan2(aimDir.y, aimDir.x);

                if (fireEffectPrefab != null && Random.value < 0.5f)
                {
                    float dist = Random.Range(1f, range);
                    float spread = Random.Range(-coneAngle, coneAngle);
                    Vector2 pos = (Vector2)owner.transform.position + new Vector2(
                        Mathf.Cos(baseAngle + spread) * dist,
                        Mathf.Sin(baseAngle + spread) * dist
                    );
                    var effect = Object.Instantiate(fireEffectPrefab, pos, Quaternion.identity);
                    Object.Destroy(effect, 0.5f);
                }

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
            int hitCount = PhysicsOverlapUtility.OverlapCircle(owner.transform.position, range);
            for (int i = 0; i < hitCount; i++)
            {
                var enemy = PhysicsOverlapUtility.GetHit(i).GetComponent<BrawlerController>();
                if (enemy == null || enemy.TeamId == owner.TeamId || enemy.IsDead) continue;

                Vector2 toEnemy = enemy.transform.position - owner.transform.position;
                float enemyAngle = Mathf.Atan2(toEnemy.y, toEnemy.x);
                float angleDiff = Mathf.Abs(Mathf.DeltaAngle(baseAngle * Mathf.Rad2Deg, enemyAngle * Mathf.Rad2Deg) * Mathf.Deg2Rad);

                if (angleDiff < coneAngle)
                    enemy.TakeDamage(damagePerTick);
            }
        }
    }
}
