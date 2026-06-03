using UnityEngine;
using BogatyriMoba.Core;

namespace BogatyriMoba.GameModes
{
    public class Safe : MonoBehaviour
    {
        [SerializeField] private int teamId; // which team owns this safe
        [SerializeField] private HeistMode heistMode;

        private void OnTriggerEnter2D(Collider2D other)
        {
            var projectile = other.GetComponent<Projectile>();
            if (projectile != null)
            {
                // Projectile hit the safe
                // In real game, projectiles would carry damage info
                // Here we delegate to HeistMode for damage calculation
            }
        }

        public void TakeDamage(int damage)
        {
            if (heistMode != null)
            {
                heistMode.DamageSafe(teamId, damage);
            }
        }

        public int GetTeamId() => teamId;
    }
}
