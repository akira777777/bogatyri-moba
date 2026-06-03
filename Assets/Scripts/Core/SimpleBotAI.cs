using UnityEngine;
using System.Collections.Generic;

namespace BogatyriMoba.Core
{
    [RequireComponent(typeof(BrawlerController))]
    public class SimpleBotAI : MonoBehaviour
    {
        [SerializeField] private float reactionTime = 0.5f;
        [SerializeField] private float moveRandomness = 0.3f;
        
        private BrawlerController controller;
        private float reactionTimer;
        private Vector2 currentAim;
        private Vector2 currentMove;
        private BrawlerController target;

        public void Initialize(BrawlerController brawler)
        {
            controller = brawler;
        }

        private void Awake()
        {
            if (controller == null)
                controller = GetComponent<BrawlerController>();
        }

        private void Update()
        {
            if (controller == null || controller.IsDead) return;

            reactionTimer -= Time.deltaTime;
            if (reactionTimer <= 0)
            {
                reactionTimer = reactionTime;
                Think();
            }

            // Apply movement
            var rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                float speed = controller.GetData().movementSpeed;
                rb.velocity = currentMove * speed;
            }

            // Attack if target in range
            if (target != null && !target.IsDead)
            {
                float dist = Vector2.Distance(transform.position, target.transform.position);
                if (dist < controller.GetData().attackRange)
                {
                    // Simulate attack
                    var input = GetComponent<PlayerInput>();
                    if (input != null)
                    {
                        input.SetAimInput(currentAim);
                        input.OnAttackButtonDown();
                    }
                }

                if (controller.HasSuperReady && dist < controller.GetData().superRange * 1.2f)
                {
                    var input = GetComponent<PlayerInput>();
                    if (input != null)
                        input.OnSuperButtonDown();
                }
            }
        }

        private void Think()
        {
            // Find nearest enemy
            BrawlerController nearest = null;
            float nearestDist = float.MaxValue;
            
            var allPlayers = FindObjectsOfType<BrawlerController>();
            foreach (var p in allPlayers)
            {
                if (p == controller || p.IsDead || p.TeamId == controller.TeamId) continue;
                float d = Vector2.Distance(transform.position, p.transform.position);
                if (d < nearestDist)
                {
                    nearestDist = d;
                    nearest = p;
                }
            }
            target = nearest;

            if (nearest == null)
            {
                currentMove = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
                return;
            }

            Vector2 toEnemy = target.transform.position - transform.position;
            currentAim = toEnemy.normalized;
            float dEnemy = toEnemy.magnitude;

            // Find nearest gem
            Gem nearestGem = null;
            float gemDist = 200f;
            var allGems = FindObjectsOfType<Gem>();
            foreach (var g in allGems)
            {
                float gd = Vector2.Distance(transform.position, g.transform.position);
                if (gd < gemDist)
                {
                    gemDist = gd;
                    nearestGem = g;
                }
            }

            float hpPct = (float)controller.CurrentHealth / controller.MaxHealth;
            bool hasGems = false; // TODO: Track gems on bot

            if (hpPct < 0.3f)
            {
                // Retreat
                Vector2 retreatDir = -toEnemy.normalized;
                currentMove = retreatDir;
            }
            else if (nearestGem != null && gemDist < 150f && (nearest == null || nearestDist > 200f))
            {
                // Go for gem
                Vector2 toGem = nearestGem.transform.position - transform.position;
                currentMove = toGem.normalized;
            }
            else if (dEnemy < controller.GetData().attackRange * 0.6f)
            {
                // Too close, strafe
                currentMove = new Vector2(-toEnemy.y, toEnemy.x).normalized * moveRandomness;
            }
            else if (dEnemy > controller.GetData().attackRange * 0.8f)
            {
                // Approach
                currentMove = toEnemy.normalized;
            }
            else
            {
                // Hold
                currentMove = Vector2.zero;
            }

            // Obstacle avoidance using raycast
            RaycastHit2D hit = Physics2D.Raycast(transform.position, currentMove, 2f, LayerMask.GetMask("Obstacles"));
            if (hit.collider != null)
            {
                // Try to go around
                Vector2 perp = new Vector2(-currentMove.y, currentMove.x);
                if (Physics2D.Raycast(transform.position, perp, 1f, LayerMask.GetMask("Obstacles")).collider == null)
                    currentMove = perp;
                else
                    currentMove = -perp;
            }
        }
    }
}
