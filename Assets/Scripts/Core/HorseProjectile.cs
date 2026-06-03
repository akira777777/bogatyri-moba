using UnityEngine;
using BogatyriMoba.GameModes;

namespace BogatyriMoba.Core
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class HorseProjectile : MonoBehaviour
    {
        private Vector2 direction;
        private float speed;
        private int damage;
        private float knockbackForce;
        private float lifetime;
        private int teamId;
        private int ownerActorNumber;
        private BrawlerData data;

        private float timer;
        private Rigidbody2D rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        public void Initialize(Vector2 dir, BrawlerController owner, float spd, int dmg, float kb, float life)
        {
            direction = dir.normalized;
            speed = spd;
            damage = dmg;
            knockbackForce = kb;
            lifetime = life;
            teamId = owner.TeamId;
            ownerActorNumber = owner.ActorNumber;
            data = owner.GetData();
            timer = 0f;

            if (rb != null)
                rb.linearVelocity = direction * speed;

            // Rotate to face direction
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }

        private void Update()
        {
            timer += Time.deltaTime;
            if (timer >= lifetime)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var safe = other.GetComponent<Safe>();
            if (safe != null)
            {
                if (safe.GetTeamId() != teamId)
                    safe.TakeDamage(damage);
                return;
            }

            var enemy = other.GetComponent<BrawlerController>();
            if (enemy != null && enemy.TeamId != teamId && !enemy.IsDead)
            {
                enemy.TakeDamage(damage);
                
                // Apply knockback
                var enemyRb = enemy.GetComponent<Rigidbody2D>();
                if (enemyRb != null)
                {
                    enemyRb.AddForce(direction * knockbackForce, ForceMode2D.Impulse);
                }
            }
            else if (other.GetComponent<Destructible>() != null || other.CompareTag("Obstacle"))
            {
                // Stop on walls? Or continue? Brawl Stars style: horses pass through walls
                // For now, we don't destroy on walls
            }
        }
    }
}
