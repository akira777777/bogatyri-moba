using UnityEngine;
using BogatyriMoba.GameModes;

namespace BogatyriMoba.Core
{
    /// <summary>
    /// Pool-enabled projectile. Replaces Instantiate/Destroy pattern.
    /// </summary>
    public class PooledProjectile : MonoBehaviour, IPoolable
    {
        [SerializeField] private float lifeTime = 3f;
        [SerializeField] private string poolKey = "projectile";

        private int damage;
        private float speed;
        private Vector2 direction;
        private int ownerActorNumber;
        private int ownerTeamId;
        private bool isSuper;
        private bool piercing;
        private BrawlerController owner;
        private BrawlerData data;

        private float spawnTime;
        private bool isReleased;
        private Transform cachedTransform;

        public string PoolKey => poolKey;
        public int OwnerActorNumber => ownerActorNumber;
        public int OwnerTeamId => ownerTeamId;

        public void OnPoolCreate()
        {
            cachedTransform = transform;
        }

        public void OnPoolGet()
        {
            isReleased = false;
            spawnTime = Time.time;
        }

        public void OnPoolRelease()
        {
            damage = 0;
            speed = 0;
            direction = Vector2.zero;
            owner = null;
            data = null;
            isReleased = true;
        }

        public void Initialize(int damage, float speed, Vector2 direction, BrawlerController owner, bool isSuper, BrawlerData data)
        {
            this.damage = damage;
            this.speed = speed;
            this.direction = direction.normalized;
            this.owner = owner;
            this.ownerActorNumber = owner != null ? owner.ActorNumber : -1;
            this.ownerTeamId = owner != null ? owner.TeamId : -1;
            this.isSuper = isSuper;
            this.data = data;
            spawnTime = Time.time;
            isReleased = false;
        }

        public void SetPiercing(bool value)
        {
            piercing = value;
        }

        private void Update()
        {
            if (isReleased) return;

            cachedTransform.Translate(direction * speed * Time.deltaTime, Space.World);

            if (Time.time - spawnTime > lifeTime)
                ReturnToPool();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isReleased) return;

            if (other.CompareTag("Obstacle") || other.GetComponent<WallObject>() != null)
            {
                ReturnToPool();
                return;
            }

            var safe = other.GetComponent<Safe>();
            if (safe != null)
            {
                if (owner != null && safe.GetTeamId() == ownerTeamId)
                    return;

                safe.TakeDamage(damage);
                if (!piercing)
                    ReturnToPool();
                return;
            }

            var target = other.GetComponent<BrawlerController>();
            if (target != null)
            {
                if (target.ActorNumber == ownerActorNumber || target.TeamId == ownerTeamId)
                    return;

                var targetHealth = target.GetComponent<HealthComponent>();
                if (targetHealth != null)
                    targetHealth.TakeDamage(damage, owner);

                EventBus.Publish(new DamageDealtEvent
                {
                    Attacker = owner,
                    Victim = target,
                    Damage = damage,
                    IsSuper = isSuper
                });

                if (!isSuper && owner != null && data != null)
                {
                    var ownerCombat = owner.GetComponent<CombatComponent>();
                    if (ownerCombat != null)
                        ownerCombat.ChargeSuper(data.superChargePerHit);
                }

                if (!piercing)
                    ReturnToPool();
                return;
            }

            var destructible = other.GetComponent<Destructible>();
            if (destructible != null)
            {
                destructible.takeDamage(damage, owner);
                if (!piercing)
                    ReturnToPool();
                return;
            }

            ReturnToPool();
        }

        private void ReturnToPool()
        {
            if (isReleased) return;
            if (PoolManager.Instance != null)
                PoolManager.Instance.Release(poolKey, this);
            else
                Destroy(gameObject);
        }
    }
}
