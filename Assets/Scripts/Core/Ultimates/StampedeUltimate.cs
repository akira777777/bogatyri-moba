using UnityEngine;

namespace BogatyriMoba.Core.Ultimates
{
    [CreateAssetMenu(fileName = "StampedeUltimate", menuName = "Bogatyri/Ultimates/Stampede")]
    public class StampedeUltimate : UltimateAbility
    {
        [Header("Stampede Settings")]
        public GameObject horsePrefab;
        public float speed = 8f;
        public int damage = 400;
        public float knockback = 300f;
        public float lifetime = 2.5f;
        public float spreadAngle = 0.35f;
        public int horseCount = 3;

        public override void Activate(BrawlerController owner)
        {
            Vector2 aimDir = AimHelper.GetAimDirection(owner);
            float baseAngle = Mathf.Atan2(aimDir.y, aimDir.x);

            for (int i = -(horseCount / 2); i <= horseCount / 2; i++)
            {
                float angle = baseAngle + i * spreadAngle;
                Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                SpawnHorse(owner, dir);
            }
        }

        private void SpawnHorse(BrawlerController owner, Vector2 dir)
        {
            Vector2 spawnPos = owner.transform.position;
            GameObject horseObj = Object.Instantiate(horsePrefab, spawnPos, Quaternion.identity);

            var horse = horseObj.GetComponent<HorseProjectile>();
            if (horse != null)
            {
                horse.Initialize(dir, owner, speed, damage, knockback, lifetime);
            }
            else
            {
                var rb = horseObj.GetComponent<Rigidbody2D>();
                if (rb != null) rb.linearVelocity = dir * speed;
                Object.Destroy(horseObj, lifetime);
            }
        }
    }
}
