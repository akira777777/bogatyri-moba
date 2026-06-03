using UnityEngine;

namespace BogatyriMoba.Core
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(PlayerInput))]
    public class BrawlerController : MonoBehaviour
    {
        [SerializeField] private BrawlerData data;
        [SerializeField] private Transform projectileSpawnPoint;
        [SerializeField] private GameObject visualContainer;

        // Identity
        public int ActorNumber { get; set; } = -1;
        public int TeamId { get; set; } = 0;

        // State
        public int CurrentHealth { get; private set; }
        public int MaxHealth { get; private set; }
        public int PowerLevel { get; set; } = 1;
        public int SuperCharge { get; private set; } = 0;
        public bool IsDead { get; private set; }
        public bool HasSuperReady => SuperCharge >= 100;
        public bool IsStunned { get; private set; }

        // Timers
        private float attackCooldownTimer;
        private float superCooldownTimer;
        private float stunTimer;
        private float shieldTimer;
        private int shieldAmount;

        // Components
        private Rigidbody2D rb;
        private PlayerInput input;
        private StealthComponent stealth;
        private BuffComponent buff;

        // Events
        public System.Action OnDeath;
        public System.Action<int, int> OnHealthChanged;
        public System.Action<int> OnSuperChargeChanged;
        public System.Action<int> OnDealDamage;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            input = GetComponent<PlayerInput>();
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

        private void Update()
        {
            if (IsDead) return;

            // Update timers
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
                rb.velocity = Vector2.zero;
                return;
            }

            HandleMovement();
            HandleAttack();
            HandleSuper();
            HandleGadget();
        }

        private void HandleMovement()
        {
            Vector2 move = input.MoveDirection;
            float speed = data.movementSpeed * GetSpeedMultiplier();
            rb.velocity = move * speed;

            if (move.x != 0 && visualContainer != null)
            {
                Vector3 scale = visualContainer.transform.localScale;
                scale.x = move.x > 0 ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
                visualContainer.transform.localScale = scale;
            }
        }

        private void HandleAttack()
        {
            if (!input.AttackPressed) return;
            if (attackCooldownTimer > 0) return;

            PerformAttack();
            input.ClearAttackFlag();
        }

        private void HandleSuper()
        {
            if (!input.SuperPressed) return;
            if (!HasSuperReady) return;
            if (superCooldownTimer > 0) return;

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
            attackCooldownTimer = data.attackReloadTime;
            int damage = Mathf.RoundToInt(data.GetDamageForPowerLevel(PowerLevel) * GetDamageMultiplier());
            SpawnProjectile(data.attackProjectilePrefab, damage, data.attackRange, false);
        }

        private void PerformSuper()
        {
            superCooldownTimer = data.ultimateAbility != null ? data.ultimateAbility.cooldown : 0.5f;
            SuperCharge = 0;
            OnSuperChargeChanged?.Invoke(SuperCharge);

            if (data.ultimateAbility != null)
            {
                data.ultimateAbility.Activate(this);
            }
            else
            {
                // Fallback: default projectile super
                SpawnProjectile(data.superProjectilePrefab, data.superDamage, data.superRange, true);
            }
        }

        private void SpawnProjectile(GameObject prefab, int damage, float range, bool isSuper)
        {
            if (prefab == null) return;

            Vector2 aimDir = input.AimDirection;
            if (aimDir == Vector2.zero)
                aimDir = visualContainer != null && visualContainer.transform.localScale.x < 0 ? Vector2.left : Vector2.right;

            Vector2 spawnPos = projectileSpawnPoint != null ? projectileSpawnPoint.position : transform.position;
            GameObject proj = Instantiate(prefab, spawnPos, Quaternion.identity);
            
            var projectile = proj.GetComponent<Projectile>();
            if (projectile != null)
            {
                projectile.Initialize(damage, data.projectileSpeed, aimDir, ActorNumber, isSuper, data);
            }

            float travelTime = range / data.projectileSpeed;
            Destroy(proj, travelTime);
        }

        public void TakeDamage(int damage)
        {
            if (IsDead) return;

            // Shield absorbs damage first
            if (shieldAmount > 0)
            {
                int absorbed = Mathf.Min(damage, shieldAmount);
                shieldAmount -= absorbed;
                damage -= absorbed;
                if (shieldAmount <= 0)
                    shieldTimer = 0f;
            }

            if (damage <= 0) return;

            // Stunned targets take extra damage
            if (IsStunned)
                damage = Mathf.RoundToInt(damage * 1.2f);

            CurrentHealth -= damage;
            if (CurrentHealth < 0) CurrentHealth = 0;

            OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);

            if (CurrentHealth <= 0)
            {
                Die();
            }
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
            SuperCharge = Mathf.Min(SuperCharge + amount, 100);
            OnSuperChargeChanged?.Invoke(SuperCharge);
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
            rb.velocity = Vector2.zero;
        }

        private float GetSpeedMultiplier()
        {
            float mult = 1f;
            if (stealth == null)
                stealth = GetComponent<StealthComponent>();
            if (stealth != null && stealth.IsStealthed)
                mult *= stealth.SpeedMultiplier;
            
            if (buff == null)
                buff = GetComponent<BuffComponent>();
            if (buff != null && buff.IsActive)
                mult *= buff.SpeedMultiplier;
            
            return mult;
        }

        private float GetDamageMultiplier()
        {
            float mult = 1f;
            if (stealth == null)
                stealth = GetComponent<StealthComponent>();
            if (stealth != null && stealth.IsStealthed)
                mult *= stealth.CritMultiplier;
            
            if (buff == null)
                buff = GetComponent<BuffComponent>();
            if (buff != null && buff.IsActive)
                mult *= buff.DamageMultiplier;
            
            return mult;
        }

        private void Die()
        {
            IsDead = true;
            rb.velocity = Vector2.zero;
            OnDeath?.Invoke();
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
