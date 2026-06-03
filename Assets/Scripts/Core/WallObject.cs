using UnityEngine;

namespace BogatyriMoba.Core
{
    public class WallObject : MonoBehaviour
    {
        [SerializeField] private Collider2D wallCollider;
        private float lifetime;
        private int teamId;
        private float timer;

        public void Initialize(float life, int team)
        {
            lifetime = life;
            teamId = team;
            timer = 0f;
        }

        private void Update()
        {
            timer += Time.deltaTime;
            if (timer >= lifetime)
            {
                Destroy(gameObject);
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            // Walls block everyone (including the team that placed them)
            // This is the Brawl Stars behavior for most wall abilities
        }
    }
}
