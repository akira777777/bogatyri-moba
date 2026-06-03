using UnityEngine;

namespace BogatyriMoba.Core
{
    public class Destructible : MonoBehaviour
    {
        [SerializeField] private int maxHealth = 1000;
        private int currentHealth;

        private void Awake()
        {
            currentHealth = maxHealth;
        }

        public void takeDamage(int damage, BrawlerController attacker)
        {
            if (damage <= 0) return;

            currentHealth -= damage;
            if (currentHealth <= 0)
            {
                Destroy(gameObject);
            }
        }
    }
}
