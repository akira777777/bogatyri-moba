using UnityEngine;

namespace BogatyriMoba.Core
{
    /// <summary>
    /// Handles attacks, projectiles, super charge, and cooldowns.
    /// Decoupled from health and movement logic.
    /// </summary>
    public class CombatComponent : MonoBehaviour
    {
        private const float SpreadAngleRadians = 0.25f;
        private const float MeleeRadiusMultiplier = 0.65f;
        private const float AoERadiusMultiplier = 0.85f;

        [SerializeField] private Transform projectileSpawnPoint;

        private BrawlerData data;
        private MovementComponent movement;
        private int powerLevel = 1;
        private float attackCooldownTimer;
        private float superCooldownTimer;
        private int superCharge;

        public int SuperCharge => superCharge;
        public bool HasSuperReady => superCharge >= 100;
        public bool CanAttack => attackCooldownTimer <= 0f;
        public bool CanSuper => superCooldownTimer <= 0f && HasSuperReady;
        public float AttackCooldownTimer => attackCooldownTimer;
        public float SuperCooldownTimer => superCooldownTimer;

        public System.Action<int> OnSuperChargeChanged;
        public System.Action<int, int> OnDealDamage;

        private void Awake()
        {
            movement = GetComponent<MovementComponent>();

            if (projectileSpawnPoint == null)
            {
                var visual = transform.Find("VisualContainer");
                if (visual != null)
                {
                    var spawn = visual.Find("ProjectileSpawnPoint");
                    if (spawn != null)
                        projectileSpawnPoint = spawn;
                }
            }
        }

        public void Initialize(BrawlerData brawlerData, int initialPowerLevel)
        {
            data = brawlerData;
            powerLevel = Mathf.Clamp(initialPowerLevel, 1, 11);
            superCharge = 0;
            attackCooldownTimer = 0f;
            superCooldownTimer = 0f;
            OnSuperChargeChanged?.Invoke(superCharge);
        }

        public void SetData(BrawlerData brawlerData)
        {
            data = brawlerData;
        }

        public void Tick()
        {
            if (attackCooldownTimer > 0)
                attackCooldownTimer -= Time.deltaTime;
            if (superCooldownTimer > 0)
                superCooldownTimer -= Time.deltaTime;
        }

        public void PerformAttack(Vector2 aimDirection)
        {
            if (data == null || attackCooldownTimer > 0) return;

            attackCooldownTimer = data.attackReloadTime;
            int damage = Mathf.RoundToInt(data.GetDamageForPowerLevel(powerLevel) * (movement != null ? movement.GetDamageMultiplier() : 1f));
            Vector2 dir = aimDirection.sqrMagnitude < 0.01f ? (movement != null ? movement.GetFacingDirection() : Vector2.right) : aimDirection.normalized;

            switch (data.attackType)
            {
                case AttackType.Melee:
                    PerformMeleeAttack(damage, dir);
                    break;
                case AttackType.Spread:
                    PerformSpreadAttack(damage, dir);
                    break;
                case AttackType.AoE:
                    PerformAoEAttack(damage, dir);
                    break;
                default:
                    SpawnProjectile(data.attackProjectilePrefab, damage, data.attackRange, dir, false);
                    break;
            }
        }

        public void PerformSuper(Vector2 aimDirection)
        {
            if (data == null || !HasSuperReady || superCooldownTimer > 0) return;

            superCooldownTimer = data.ultimateAbility != null ? data.ultimateAbility.cooldown : 0.5f;
            superCharge = 0;
            OnSuperChargeChanged?.Invoke(superCharge);

            if (data.ultimateAbility != null)
            {
                var owner = GetComponent<BrawlerController>();
                data.ultimateAbility.Activate(owner);
            }
            else
            {
                Vector2 dir = aimDirection.sqrMagnitude < 0.01f ? (movement != null ? movement.GetFacingDirection() : Vector2.right) : aimDirection.normalized;
                SpawnProjectile(data.superProjectilePrefab, data.superDamage, data.superRange, dir, true);
            }
        }

        public void ChargeSuper(int amount)
        {
            var health = GetComponent<HealthComponent>();
            if (health != null && health.IsDead) return;

            bool wasReady = HasSuperReady;
            superCharge = Mathf.Min(superCharge + amount, 100);
            OnSuperChargeChanged?.Invoke(superCharge);

            if (!wasReady && HasSuperReady && data?.ultimateAbility != null)
            {
                var owner = GetComponent<BrawlerController>();
                EventBus.Publish(new SuperActivatedEvent
                {
                    Player = owner,
                    AbilityName = data.ultimateAbility.abilityName
                });
            }
        }

        private void PerformMeleeAttack(int damage, Vector2 aimDir)
        {
            float radius = data.attackRange * MeleeRadiusMultiplier;
            Vector2 center = (Vector2)transform.position + aimDir * radius * 0.5f;
            int hitCount = PhysicsOverlapUtility.OverlapCircle(center, radius);
            for (int i = 0; i < hitCount; i++)
            {
                var enemy = PhysicsOverlapUtility.GetHit(i).GetComponent<BrawlerController>();
                if (enemy == null || enemy.IsDead) continue;
                var enemyHealth = enemy.GetComponent<HealthComponent>();
                if (enemyHealth == null) continue;
                
                var owner = GetComponent<BrawlerController>();
                if (enemy.TeamId == owner.TeamId) continue;
                
                enemyHealth.TakeDamage(damage, owner);
                ChargeSuper(data.superChargePerHit);
            }
        }

        private void PerformSpreadAttack(int damage, Vector2 aimDir)
        {
            float baseAngle = Mathf.Atan2(aimDir.y, aimDir.x);
            for (int i = -1; i <= 1; i++)
            {
                float angle = baseAngle + i * SpreadAngleRadians;
                Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                SpawnProjectile(data.attackProjectilePrefab, damage, data.attackRange, dir, false);
            }
        }

        private void PerformAoEAttack(int damage, Vector2 aimDir)
        {
            float radius = data.attackRange * AoERadiusMultiplier;
            Vector2 center = (Vector2)transform.position + aimDir * radius;
            int hitCount = PhysicsOverlapUtility.OverlapCircle(center, radius);
            for (int i = 0; i < hitCount; i++)
            {
                var enemy = PhysicsOverlapUtility.GetHit(i).GetComponent<BrawlerController>();
                if (enemy == null || enemy.IsDead) continue;
                var enemyHealth = enemy.GetComponent<HealthComponent>();
                if (enemyHealth == null) continue;

                var owner = GetComponent<BrawlerController>();
                if (enemy.TeamId == owner.TeamId) continue;

                enemyHealth.TakeDamage(damage, owner);
                ChargeSuper(data.superChargePerHit);
            }
        }

        private void SpawnProjectile(GameObject prefab, int damage, float range, Vector2 aimDir, bool isSuper)
        {
            if (prefab == null) return;

            Vector2 spawnPos = projectileSpawnPoint != null ? (Vector2)projectileSpawnPoint.position : (Vector2)transform.position;
            var owner = GetComponent<BrawlerController>();

            var pooled = PoolManager.Instance?.Get<PooledProjectile>("projectile");
            if (pooled != null)
            {
                pooled.transform.position = spawnPos;
                pooled.transform.rotation = Quaternion.identity;
                pooled.Initialize(damage, data.projectileSpeed, aimDir, owner, isSuper, data);
                return;
            }

            GameObject proj = Instantiate(prefab, spawnPos, Quaternion.identity);
            var projectile = proj.GetComponent<Projectile>();
            if (projectile != null)
                projectile.Initialize(damage, data.projectileSpeed, aimDir, owner, isSuper, data);

            float travelTime = range / data.projectileSpeed;
            Destroy(proj, travelTime);
        }
    }
}
