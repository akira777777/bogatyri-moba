using UnityEngine;

namespace BogatyriMoba.Core
{
    /// <summary>
    /// Handles health, shields, stun, death and respawn for a brawler.
    /// Decoupled from movement and combat logic.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class HealthComponent : MonoBehaviour
    {
        public int CurrentHealth { get; private set; }
        public int MaxHealth { get; private set; }
        public bool IsDead { get; private set; }
        public bool IsStunned { get; private set; }

        public System.Action OnDeath;
        public System.Action<int, int> OnHealthChanged;
        public System.Action<BrawlerController> OnDeathWithKiller;

        private float stunTimer;
        private float shieldTimer;
        private int shieldAmount;
        private Rigidbody2D rb;
        private BrawlerController owner;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            owner = GetComponent<BrawlerController>();
        }

        public void Initialize(int maxHealth)
        {
            MaxHealth = maxHealth;
            CurrentHealth = MaxHealth;
            IsDead = false;
            IsStunned = false;
            shieldAmount = 0;
            shieldTimer = 0f;
            stunTimer = 0f;
            OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
        }

        private void Update()
        {
            if (IsDead) return;

            if (stunTimer > 0)
            {
                stunTimer -= Time.deltaTime;
                if (stunTimer <= 0)
                {
                    IsStunned = false;
                    if (rb != null)
                        rb.linearVelocity = Vector2.zero;
                }
            }

            if (shieldTimer > 0)
            {
                shieldTimer -= Time.deltaTime;
                if (shieldTimer <= 0)
                    shieldAmount = 0;
            }

            if (IsStunned && rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }
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

            if (CurrentHealth <= 0)
                Die(attacker);
        }

        public void Heal(int amount)
        {
            if (IsDead) return;
            CurrentHealth = Mathf.Min(CurrentHealth + amount, MaxHealth);
            OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
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
            if (rb != null)
                rb.linearVelocity = Vector2.zero;
        }

        private void Die(BrawlerController killer = null)
        {
            IsDead = true;
            if (rb != null)
                rb.linearVelocity = Vector2.zero;

            OnDeath?.Invoke();
            OnDeathWithKiller?.Invoke(killer);
        }

        public void Respawn(Vector3 position, int maxHealth)
        {
            IsDead = false;
            IsStunned = false;
            MaxHealth = maxHealth;
            CurrentHealth = MaxHealth;
            transform.position = position;
            shieldAmount = 0;
            shieldTimer = 0f;
            stunTimer = 0f;
            gameObject.SetActive(true);
            OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
        }
    }
}
