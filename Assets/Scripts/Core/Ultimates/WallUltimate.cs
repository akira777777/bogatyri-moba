using UnityEngine;

namespace BogatyriMoba.Core.Ultimates
{
    [CreateAssetMenu(fileName = "WallUltimate", menuName = "Bogatyri/Ultimates/Wall")]
    public class WallUltimate : UltimateAbility
    {
        [Header("Wall Settings")]
        public GameObject wallPrefab;
        public float distance = 4f;
        public float lifetime = 5f;
        public float width = 2f;
        public float height = 0.5f;

        public override void Activate(BrawlerController owner)
        {
            Vector2 aimDir = AimHelper.GetAimDirection(owner);
            Vector2 spawnPos = (Vector2)owner.transform.position + aimDir * distance;

            GameObject wall = Object.Instantiate(wallPrefab, spawnPos, Quaternion.identity);
            var wallObj = wall.GetComponent<WallObject>();
            if (wallObj != null)
                wallObj.Initialize(lifetime, owner.TeamId);
            else
                Object.Destroy(wall, lifetime);
        }
    }
}
