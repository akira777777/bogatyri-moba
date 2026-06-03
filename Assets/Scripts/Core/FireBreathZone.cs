using UnityEngine;
using System.Collections;

namespace BogatyriMoba.Core
{
    public class FireBreathZone : MonoBehaviour
    {
        [SerializeField] private float coneAngle = 0.8f;
        [SerializeField] private float range = 6f;
        [SerializeField] private int damagePerTick = 400;
        [SerializeField] private float tickRate = 0.3f;
        [SerializeField] private float duration = 2f;

        private BrawlerController owner;
        private float timer;
        private float tickTimer;

        public void Initialize(BrawlerController owner)
        {
            this.owner = owner;
            timer = 0f;
            tickTimer = 0f;
            StartCoroutine(FireRoutine());
        }

        private IEnumerator FireRoutine()
        {
            while (timer < duration)
            {
                float dt = Time.deltaTime;
                timer += dt;
                tickTimer += dt;

                if (owner == null || owner.IsDead)
                {
                    Destroy(gameObject);
                    yield break;
                }

                transform.position = owner.transform.position;
                Vector2 aimDir = AimHelper.GetAimDirection(owner);
                float baseAngle = Mathf.Atan2(aimDir.y, aimDir.x);
                transform.rotation = Quaternion.AngleAxis(baseAngle * Mathf.Rad2Deg, Vector3.forward);

                if (tickTimer >= tickRate)
                {
                    tickTimer = 0f;
                    DamageEnemies(baseAngle);
                }

                yield return null;
            }
            Destroy(gameObject);
        }

        private void DamageEnemies(float baseAngle)
        {
            int hitCount = PhysicsOverlapUtility.OverlapCircle(transform.position, range);
            for (int i = 0; i < hitCount; i++)
            {
                var enemy = PhysicsOverlapUtility.GetHit(i).GetComponent<BrawlerController>();
                if (enemy == null || enemy.TeamId == owner.TeamId || enemy.IsDead) continue;

                Vector2 toEnemy = enemy.transform.position - transform.position;
                float enemyAngle = Mathf.Atan2(toEnemy.y, toEnemy.x);
                float angleDiff = Mathf.Abs(Mathf.DeltaAngle(baseAngle * Mathf.Rad2Deg, enemyAngle * Mathf.Rad2Deg) * Mathf.Deg2Rad);

                if (angleDiff < coneAngle)
                    enemy.TakeDamage(damagePerTick);
            }
        }
    }
}
