using UnityEngine;

namespace BogatyriMoba.Core.Ultimates
{
    [CreateAssetMenu(fileName = "SnipeUltimate", menuName = "Bogatyri/Ultimates/Snipe")]
    public class SnipeUltimate : UltimateAbility
    {
        [Header("Snipe Settings")]
        public GameObject projectilePrefab;
        public int damageMultiplier = 3;
        public float spreadAngle = 0.08f;
        public int shotCount = 3;
        public bool piercing = true;
        public float projectileSpeed = 18f;

        public override void Activate(BrawlerController owner)
        {
            Vector2 aimDir = AimHelper.GetAimDirection(owner);
            float baseAngle = Mathf.Atan2(aimDir.y, aimDir.x);

            for (int i = -1; i <= 1; i++)
            {
                float angle = baseAngle + i * spreadAngle;
                Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                SpawnProjectile(owner, dir);
            }
        }

        private void SpawnProjectile(BrawlerController owner, Vector2 dir)
        {
            var data = owner.GetData();
            int damage = Mathf.RoundToInt(data.superDamage * (damageMultiplier / 2.5f));

            Vector2 spawnPos = owner.transform.position;
            GameObject proj = Object.Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

            var projectile = proj.GetComponent<Projectile>();
            if (projectile != null)
            {
                projectile.Initialize(damage, projectileSpeed, dir, owner, true, data);
                projectile.SetPiercing(piercing);
            }

            float travelTime = data.superRange / projectileSpeed;
            Object.Destroy(proj, travelTime);
        }
    }
}
