using UnityEngine;

namespace BogatyriMoba.Core
{
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float lifeTime = 3f;
        
        private int damage;
        private float speed;
        private Vector2 direction;
        private int ownerActorNumber;
        private bool isSuper;
        private BrawlerData data;
        private bool piercing = false;

        private float spawnTime;

        public void Initialize(int damage, float speed, Vector2 direction, int ownerActorNumber, bool isSuper, BrawlerData data)
        {
            this.damage = damage;
            this.speed = speed;
            this.direction = direction.normalized;
            this.ownerActorNumber = ownerActorNumber;
            this.isSuper = isSuper;
            this.data = data;
            spawnTime = Time.time;
        }

        public void SetPiercing(bool value)
        {
            piercing = value;
        }

        private void Update()
        {
            transform.Translate(direction * speed * Time.deltaTime, Space.World);

            if (Time.time - spawnTime > lifeTime)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Check if hit a wall/obstacle first
            if (other.CompareTag("Obstacle") || other.GetComponent<WallObject>() != null)
            {
                Destroy(gameObject);
                return;
            }

            var target = other.GetComponent<BrawlerController>();
            if (target != null)
            {
                // Don't hit self
                if (target.ActorNumber != ownerActorNumber)
                {
                    target.TakeDamage(damage);
                    
                    if (!isSuper)
                    {
                        // Charge super for owner
                        var owner = FindBrawlerByActor(ownerActorNumber);
                        if (owner != null)
                        {
                            owner.ChargeSuper(data.superChargePerHit);
                        }
                    }

                    if (!piercing)
                    {
                        Destroy(gameObject);
                    }
                }
            }
            else
            {
                // Hit destructible
                var destructible = other.GetComponent<Destructible>();
                if (destructible != null)
                {
                    destructible.takeDamage(damage, null);
                    if (!piercing)
                    {
                        Destroy(gameObject);
                    }
                }
                else
                {
                    // Hit wall or obstacle
                    Destroy(gameObject);
                }
            }
        }

        private BrawlerController FindBrawlerByActor(int actorNumber)
        {
            // Simple find — can be optimized with a registry
            var all = FindObjectsOfType<BrawlerController>();
            foreach (var b in all)
            {
                if (b.ActorNumber == actorNumber)
                    return b;
            }
            return null;
        }
    }
}
