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
            Vector2 aimDir = AimHelper.GetAimDirection(owner);
            Vector2 spawnPos = owner.transform.position;

            GameObject horseObj = Object.Instantiate(horsePrefab, spawnPos, Quaternion.identity);
            var horse = horseObj.GetComponent<HorseProjectile>();
            if (horse != null)
            {
                horse.Initialize(aimDir, owner, speed, (int)damage, knockback, lifetime);
            }
            else
            {
                var rb = horseObj.GetComponent<Rigidbody2D>();
                if (rb != null) rb.velocity = aimDir * speed;
                Object.Destroy(horseObj, lifetime);
            }
        }
    }
}
