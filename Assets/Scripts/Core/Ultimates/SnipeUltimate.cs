using UnityEngine;

namespace BogatyriMoba.Core.Ultimates
{
    [CreateAssetMenu(fileName = "SnipeUltimate", menuName = "Bogatyri/Ultimates/Snipe")]
    public class SnipeUltimate : UltimateAbility
    {
        [Header("Snipe Settings")]
        public GameObject projectilePrefab;
        public int damageMultiplier = 3; // 2.5x ~ 3x
        public float spreadAngle = 0.08f; // radians between shots
        public int shotCount = 3;
        public bool piercing = true;
        public float projectileSpeed = 18f;

        public override void Activate(BrawlerController owner)
        {
            Vector2 aimDir = GetAimDirection(owner);
            float baseAngle = Mathf.Atan2(aimDir.y, aimDir.x);

            for (int i = -1; i <= 1; i++)
            {
                float angle = baseAngle + i * spreadAngle;
                Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                
                SpawnProjectile(owner, dir, i == 0 ? 0.1f : 0f); // Slight delay for outer shots
            }
        }

        private void SpawnProjectile(BrawlerController owner, Vector2 dir, float delay)
        {
            var data = owner.GetData();
            int damage = Mathf.RoundToInt(data.superDamage * (damageMultiplier / 2.5f));
            
            Vector2 spawnPos = owner.transform.position;
            GameObject proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
            
            var projectile = proj.GetComponent<Projectile>();
            if (projectile != null)
            {
                projectile.Initialize(damage, projectileSpeed, dir, owner.ActorNumber, true, data);
                projectile.SetPiercing(piercing);
            }
            
            float range = data.superRange;
            float travelTime = range / projectileSpeed;
            Destroy(proj, travelTime);
        }

        private Vector2 GetAimDirection(BrawlerController owner)
        {
            var input = owner.GetComponent<PlayerInput>();
            if (input != null && input.AimDirection != Vector2.zero)
                return input.AimDirection.normalized;
            
            float dirX = owner.transform.localScale.x >= 0 ? 1f : -1f;
            return new Vector2(dirX, 0f);
        }
    }
}
