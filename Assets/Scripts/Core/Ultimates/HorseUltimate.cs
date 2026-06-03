using UnityEngine;

namespace BogatyriMoba.Core.Ultimates
{
    [CreateAssetMenu(fileName = "HorseUltimate", menuName = "Bogatyri/Ultimates/Horse")]
    public class HorseUltimate : UltimateAbility
    {
        [Header("Horse Settings")]
        public GameObject horsePrefab;
        public float speed = 8f;
        public float damage = 400f;
        public float knockback = 300f;
        public float lifetime = 2.5f;

        public override void Activate(BrawlerController owner)
        {
            Vector2 aimDir = GetAimDirection(owner);
            Vector2 spawnPos = owner.transform.position;
            
            GameObject horseObj = Instantiate(horsePrefab, spawnPos, Quaternion.identity);
            var horse = horseObj.GetComponent<HorseProjectile>();
            if (horse != null)
            {
                horse.Initialize(aimDir, owner, speed, damage, knockback, lifetime);
            }
            else
            {
                // Fallback: just move the object if no script attached
                var rb = horseObj.GetComponent<Rigidbody2D>();
                if (rb != null) rb.velocity = aimDir * speed;
                Destroy(horseObj, lifetime);
            }
        }

        private Vector2 GetAimDirection(BrawlerController owner)
        {
            var input = owner.GetComponent<PlayerInput>();
            if (input != null && input.AimDirection != Vector2.zero)
                return input.AimDirection.normalized;
            
            // Default to facing direction
            float dirX = owner.transform.localScale.x >= 0 ? 1f : -1f;
            return new Vector2(dirX, 0f);
        }
    }
}
