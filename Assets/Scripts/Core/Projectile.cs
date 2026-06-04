using UnityEngine;
using BogatyriMoba.GameModes;

namespace BogatyriMoba.Core
{
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float lifeTime = 3f;

        private int damage;
        private float speed;
        private Vector2 direction;
        private int ownerActorNumber;
        private int ownerTeamId;
        private bool isSuper;
        private BrawlerData data;
        private bool piercing;
        private BrawlerController owner;

        private float spawnTime;

        public int Damage => damage;

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
        }

        public void SetPiercing(bool value)
        {
            piercing = value;
        }

        private void Update()
        {
            transform.Translate(direction * speed * Time.deltaTime, Space.World);

            if (Time.time - spawnTime > lifeTime)
                Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Obstacle") || other.GetComponent<WallObject>() != null)
            {
                Destroy(gameObject);
                return;
            }

            var safe = other.GetComponent<Safe>();
            if (safe != null)
            {
                if (owner != null && safe.GetTeamId() == ownerTeamId)
                    return;

                safe.TakeDamage(damage);
                if (!piercing)
                    Destroy(gameObject);
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

                if (!isSuper && owner != null && data != null)
                {
                    var ownerCombat = owner.GetComponent<CombatComponent>();
                    if (ownerCombat != null)
                        ownerCombat.ChargeSuper(data.superChargePerHit);
                }

                if (!piercing)
                    Destroy(gameObject);

                return;
            }

            var destructible = other.GetComponent<Destructible>();
            if (destructible != null)
            {
                destructible.takeDamage(damage, owner);
                if (!piercing)
                    Destroy(gameObject);
                return;
            }

            Destroy(gameObject);
        }
    }
}
