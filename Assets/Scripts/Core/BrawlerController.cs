using UnityEngine;

namespace BogatyriMoba.Core
{
    [DefaultExecutionOrder(100)]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(PlayerInput))]
    [RequireComponent(typeof(HealthComponent))]
    [RequireComponent(typeof(MovementComponent))]
    [RequireComponent(typeof(CombatComponent))]
    public class BrawlerController : MonoBehaviour
    {
        [SerializeField] private BrawlerData data;
        [SerializeField] private Transform visualContainer;

        public int ActorNumber { get; set; } = -1;

        private int _teamId = 0;
        public int TeamId
        {
            get => _teamId;
            set
            {
                _teamId = value;
                UpdateTeamVisuals();
            }
        }

        public int CurrentHealth => health != null ? health.CurrentHealth : 0;
        public int MaxHealth => health != null ? health.MaxHealth : 0;
        public int PowerLevel { get; set; } = 1;
        public int SuperCharge => combat != null ? combat.SuperCharge : 0;
        public bool IsDead => health != null && health.IsDead;
        public bool HasSuperReady => combat != null && combat.HasSuperReady;
        public bool IsStunned => health != null && health.IsStunned;

        // Legacy events — proxied from components for backward compatibility
        public event System.Action OnDeath
        {
            add { if (health != null) health.OnDeath += value; }
            remove { if (health != null) health.OnDeath -= value; }
        }

        public event System.Action<int, int> OnHealthChanged
        {
            add { if (health != null) health.OnHealthChanged += value; }
            remove { if (health != null) health.OnHealthChanged -= value; }
        }

        public event System.Action<int> OnSuperChargeChanged
        {
            add { if (combat != null) combat.OnSuperChargeChanged += value; }
            remove { if (combat != null) combat.OnSuperChargeChanged -= value; }
        }

        public System.Action<int> OnDealDamage;

        private PlayerInput input;
        private HealthComponent health;
        private MovementComponent movement;
        private CombatComponent combat;

        private void Awake()
        {
            input = GetComponent<PlayerInput>();
            health = GetComponent<HealthComponent>();
            movement = GetComponent<MovementComponent>();
            combat = GetComponent<CombatComponent>();

            if (visualContainer == null)
            {
                var visual = transform.Find("VisualContainer");
                if (visual != null)
                    visualContainer = visual;
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
                int maxHp = data.GetHealthForPowerLevel(PowerLevel);
                health?.Initialize(maxHp);
                combat?.Initialize(data, PowerLevel);
                movement?.Initialize(data.movementSpeed);
            }
            UpdateTeamVisuals();
        }

        public void SetData(BrawlerData brawlerData)
        {
            data = brawlerData;
            Initialize(PowerLevel);
        }

        public Vector2 GetFacingDirection()
        {
            return movement != null ? movement.GetFacingDirection() : Vector2.right;
        }

        private void Update()
        {
            if (IsDead) return;

            combat?.Tick();

            if (IsStunned)
            {
                movement?.Stop();
                return;
            }

            HandleMovement();
            HandleAttack();
            HandleSuper();
            HandleGadget();
        }

        private void HandleMovement()
        {
            if (movement == null || data == null) return;
            movement.MoveDirection = input.MoveDirection;
            movement.Tick();
        }

        private void HandleAttack()
        {
            if (combat == null || !input.AttackPressed || !combat.CanAttack) return;
            combat.PerformAttack(input.AimDirection);
            input.ClearAttackFlag();
        }

        private void HandleSuper()
        {
            if (combat == null || !input.SuperPressed || !combat.HasSuperReady || !combat.CanSuper) return;
            combat.PerformSuper(input.AimDirection);
            input.ClearSuperFlag();
        }

        private void HandleGadget()
        {
            if (!input.GadgetPressed) return;
            input.ClearGadgetFlag();
        }

        #region Legacy Proxy Methods

        [System.Obsolete("Use HealthComponent.TakeDamage instead")]
        public void TakeDamage(int damage, BrawlerController attacker = null)
        {
            health?.TakeDamage(damage, attacker);
        }

        [System.Obsolete("Use HealthComponent.Heal instead")]
        public void Heal(int amount)
        {
            health?.Heal(amount);
        }

        [System.Obsolete("Use CombatComponent.ChargeSuper instead")]
        public void ChargeSuper(int amount)
        {
            combat?.ChargeSuper(amount);
        }

        [System.Obsolete("Use HealthComponent.ApplyShield instead")]
        public void ApplyShield(int amount, float duration)
        {
            health?.ApplyShield(amount, duration);
        }

        [System.Obsolete("Use HealthComponent.ApplyStun instead")]
        public void ApplyStun(float duration)
        {
            health?.ApplyStun(duration);
        }

        #endregion

        public void Respawn(Vector3 position)
        {
            if (data != null)
            {
                int maxHp = data.GetHealthForPowerLevel(PowerLevel);
                health?.Respawn(position, maxHp);
            }
            combat?.Initialize(data, PowerLevel);
        }

        public BrawlerData GetData() => data;

        private void UpdateTeamVisuals()
        {
            var sr = GetComponent<SpriteRenderer>();
            if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
            {
                if (data != null && data.icon != null)
                {
                    sr.sprite = data.icon;
                    sr.color = Color.white;
                }
                else
                {
                    sr.color = (_teamId == 0) ? new Color(0.2f, 0.6f, 1f) : new Color(1f, 0.3f, 0.3f);
                    var knob = Resources.GetBuiltinResource<Sprite>("UI/Skin/Knob.psd");
                    if (knob != null)
                        sr.sprite = knob;
                }
            }
        }
    }
}
