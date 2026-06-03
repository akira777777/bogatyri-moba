using UnityEngine;

namespace BogatyriMoba.Core.Ultimates
{
    [CreateAssetMenu(fileName = "WallUltimate", menuName = "Bogatyri/Ultimates/Wall")]
    public class WallUltimate : UltimateAbility
    {
        [Header("Wall Settings")]
        public GameObject wallPrefab;
        public float distance = 100f;
        public float lifetime = 5f;
        public float width = 80f;
        public float height = 20f;

        public override void Activate(BrawlerController owner)
        {
            Vector2 aimDir = GetAimDirection(owner);
            Vector2 spawnPos = (Vector2)owner.transform.position + aimDir * distance;
            
            GameObject wall = Instantiate(wallPrefab, spawnPos, Quaternion.identity);
            var wallObj = wall.GetComponent<WallObject>();
            if (wallObj != null)
            {
                wallObj.Initialize(lifetime, owner.TeamId);
            }
            else
            {
                Destroy(wall, lifetime);
            }
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
