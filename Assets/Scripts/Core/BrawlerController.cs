using UnityEngine;

namespace BogatyriMoba.Core
{
    [DefaultExecutionOrder(100)]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(PlayerInput))]
    public class BrawlerController : MonoBehaviour
    {
        private const float SpreadAngleRadians = 0.25f;
        private const float MeleeRadiusMultiplier = 0.65f;
        private const float AoERadiusMultiplier = 0.85f;

        [SerializeField] private BrawlerData data;
        [SerializeField] private Transform projectileSpawnPoint;
        [SerializeField] private GameObject visualContainer;

        public int ActorNumber { get; set; } = -1;
        public int TeamId { get; set; } = 0;

        public int CurrentHealth { get; private set; }
        public int MaxHealth { get; private set; }
        public int PowerLevel { get; set; } = 1;
        public int SuperCharge { get; private set; }
        public bool IsDead { get; private set; }
        public bool HasSuperReady => SuperCharge >= 100;
        public bool IsStunned { get; private set; }

        private float attackCooldownTimer;
        private float superCooldownTimer;
        private float stunTimer;
        private float shieldTimer;
        private int shieldAmount;

        private Rigidbody2D rb;
        private PlayerInput input;
        private StealthComponent stealth;
        private BuffComponent buff;

        public System.Action OnDeath;
        public System.Action<int, int> OnHealthChanged;
        public System.Action<int> OnSuperChargeChanged;
        public System.Action<int> OnDealDamage;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            input = GetComponent<PlayerInput>();
            stealth = GetComponent<StealthComponent>();
            buff = GetComponent<BuffComponent>();

            if (visualContainer == null)
            {
                var visual = transform.Find("VisualContainer");
                if (visual != null)
                    visualContainer = visual;
            }

            if (projectileSpawnPoint == null && visualContainer != null)
            {
                var spawn = visualContainer.Find("ProjectileSpawnPoint");
                if (spawn != null)
                    projectileSpawnPoint = spawn;
            }
        }

        private void OnDestroy()
        {
            BrawlerRegistry.Unregister(this);
        }

        private void Start()
        {
            Initialize(PowerLevel);
        }

        public void Initialize(int powerLevel)
        {
            PowerLevel = Mathf.Clamp(powerLevel, 1, 11);
            if (data != null)
            {
                MaxHealth = data.GetHealthForPowerLevel(PowerLevel);
                CurrentHealth = MaxHealth;
                OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
            }
            IsDead = false;
            IsStunned = false;
            SuperCharge = 0;
            OnSuperChargeChanged?.Invoke(SuperCharge);
        }

        public void SetData(BrawlerData brawlerData)
        {
            data = brawlerData;
            Initialize(PowerLevel);
        }

        public Vector2 GetFacingDirection()
        {
            if (visualContainer != null)
                return visualContainer.transform.localScale.x < 0 ? Vector2.left : Vector2.right;

            return transform.localScale.x < 0 ? Vector2.left : Vector2.right;
        }

        private void Update()
        {
            if (IsDead) return;

            if (attackCooldownTimer > 0)
                attackCooldownTimer -= Time.deltaTime;
            if (superCooldownTimer > 0)
                superCooldownTimer -= Time.deltaTime;
            if (stunTimer > 0)
            {
                stunTimer -= Time.deltaTime;
                if (stunTimer <= 0)
                    IsStunned = false;
            }
            if (shieldTimer > 0)
            {
                shieldTimer -= Time.deltaTime;
                if (shieldTimer <= 0)
                    shieldAmount = 0;
            }

            if (IsStunned)
            {
                rb.linearVelocity = Vector2.zero;
                return;
            }

            HandleMovement();
            HandleAttack();
            HandleSuper();
            HandleGadget();
        }

        private void HandleMovement()
        {
            if (data == null) return;

            Vector2 move = input.MoveDirection;
            float speed = data.movementSpeed * GetSpeedMultiplier();
            rb.linearVelocity = move * speed;

            if (move.x != 0 && visualContainer != null)
            {
                Vector3 scale = visualContainer.transform.localScale;
                scale.x = move.x > 0 ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
                visualContainer.transform.localScale = scale;
            }
        }

        private void HandleAttack()
        {
            if (!input.AttackPressed || attackCooldownTimer > 0) return;

            PerformAttack();
            input.ClearAttackFlag();
        }

        private void HandleSuper()
        {
            if (!input.SuperPressed || !HasSuperReady || superCooldownTimer > 0) return;

            PerformSuper();
            input.ClearSuperFlag();
        }

        private void HandleGadget()
        {
            if (!input.GadgetPressed) return;
            input.GadgetPressed = false;
        }

        private void PerformAttack()
        {
            if (data == null) return;

            attackCooldownTimer = data.attackReloadTime;
            int damage = Mathf.RoundToInt(data.GetDamageForPowerLevel(PowerLevel) * GetDamageMultiplier());
            Vector2 aimDir = GetAttackDirection();

            switch (data.attackType)
            {
                case AttackType.Melee:
                    PerformMeleeAttack(damage, aimDir);
                    break;
                case AttackType.Spread:
                    PerformSpreadAttack(damage, aimDir);
                    break;
                case AttackType.AoE:
                    PerformAoEAttack(damage, aimDir);
                    break;
                default:
                    SpawnProjectile(data.attackProjectilePrefab, damage, data.attackRange, aimDir, false);
                    break;
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
                if (enemy == null || enemy.TeamId == TeamId || enemy.IsDead) continue;
                enemy.TakeDamage(damage);
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
                if (enemy == null || enemy.TeamId == TeamId || enemy.IsDead) continue;
                enemy.TakeDamage(damage);
                ChargeSuper(data.superChargePerHit);
            }
        }

        private void PerformSuper()
        {
            superCooldownTimer = data.ultimateAbility != null ? data.ultimateAbility.cooldown : 0.5f;
            SuperCharge = 0;
            OnSuperChargeChanged?.Invoke(SuperCharge);

            if (data.ultimateAbility != null)
                data.ultimateAbility.Activate(this);
            else
                SpawnProjectile(data.superProjectilePrefab, data.superDamage, data.superRange, GetAttackDirection(), true);
        }

        private Vector2 GetAttackDirection()
        {
            Vector2 aimDir = input.AimDirection;
            if (aimDir.sqrMagnitude < 0.01f)
                aimDir = GetFacingDirection();
            return aimDir.normalized;
        }

        private void SpawnProjectile(GameObject prefab, int damage, float range, Vector2 aimDir, bool isSuper)
        {
            if (prefab == null) return;

            Vector2 spawnPos = projectileSpawnPoint != null ? (Vector2)projectileSpawnPoint.position : (Vector2)transform.position;

            // Try pooled projectile first
            var pooled = PoolManager.Instance?.Get<PooledProjectile>("projectile");
            if (pooled != null)
            {
                pooled.transform.position = spawnPos;
                pooled.transform.rotation = Quaternion.identity;
                pooled.Initialize(damage, data.projectileSpeed, aimDir, this, isSuper, data);
                return;
            }

            // Fallback to instantiate
            GameObject proj = Instantiate(prefab, spawnPos, Quaternion.identity);
            var projectile = proj.GetComponent<Projectile>();
            if (projectile != null)
                projectile.Initialize(damage, data.projectileSpeed, aimDir, this, isSuper, data);

            float travelTime = range / data.projectileSpeed;
            Destroy(proj, travelTime);
        }

        public void TakeDamage(int damage, BrawlerController attacker = null)
        {
            if (IsDead) return;

            if (shieldAmount > 0)
            {
                int absorbed = Mathf.Min(damage, shieldAmount);
                shieldAmount -= absorbed;
                damage -= absorbed;
                if (shieldAmount <= 0)
                    shieldTimer = 0f;
            }

            if (damage <= 0) return;

            if (IsStunned)
                damage = Mathf.RoundToInt(damage * 1.2f);

            CurrentHealth -= damage;
            if (CurrentHealth < 0) CurrentHealth = 0;

            OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);

            if (attacker != null)
            {
                EventBus.Publish(new DamageDealtEvent
                {
                    Attacker = attacker,
                    Victim = this,
                    Damage = damage,
                    IsSuper = false
                });
            }

            if (CurrentHealth <= 0)
                Die(attacker);
        }

        public void Heal(int amount)
        {
            if (IsDead) return;
            CurrentHealth = Mathf.Min(CurrentHealth + amount, MaxHealth);
            OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
        }

        public void ChargeSuper(int amount)
        {
            if (IsDead) return;
            bool wasReady = HasSuperReady;
            SuperCharge = Mathf.Min(SuperCharge + amount, 100);
            OnSuperChargeChanged?.Invoke(SuperCharge);

            if (!wasReady && HasSuperReady && data?.ultimateAbility != null)
            {
                EventBus.Publish(new SuperActivatedEvent
                {
                    Player = this,
                    AbilityName = data.ultimateAbility.abilityName
                });
            }
        }

        public void ApplyShield(int amount, float duration)
        {
            shieldAmount = amount;
            shieldTimer = duration;
        }

        public void ApplyStun(float duration)
        {
            IsStunned = true;
            stunTimer = duration;
            rb.linearVelocity = Vector2.zero;
        }

        private float GetSpeedMultiplier()
        {
            float mult = 1f;
            if (stealth != null && stealth.IsStealthed)
                mult *= stealth.SpeedMultiplier;
            if (buff != null && buff.IsActive)
                mult *= buff.SpeedMultiplier;
            return mult;
        }

        private float GetDamageMultiplier()
        {
            float mult = 1f;
            if (stealth != null && stealth.IsStealthed)
                mult *= stealth.CritMultiplier;
            if (buff != null && buff.IsActive)
                mult *= buff.DamageMultiplier;
            return mult;
        }

        private void Die(BrawlerController killer = null)
        {
            IsDead = true;
            rb.linearVelocity = Vector2.zero;
            OnDeath?.Invoke();

            EventBus.Publish(new PlayerDeathEvent
            {
                Player = this,
                Killer = killer,
                TeamId = TeamId
            });

            gameObject.SetActive(false);
        }

        public void Respawn(Vector3 position)
        {
            IsDead = false;
            IsStunned = false;
            CurrentHealth = MaxHealth;
            transform.position = position;
            gameObject.SetActive(true);
            OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
        }

        public BrawlerData GetData() => data;
    }
}
