using UnityEngine;
using BogatyriMoba.GameModes;

namespace BogatyriMoba.Core
{
    [DefaultExecutionOrder(50)]
    [RequireComponent(typeof(BrawlerController))]
    public class SimpleBotAI : MonoBehaviour
    {
        [SerializeField] private float reactionTime = 0.5f;
        [SerializeField] private float moveRandomness = 0.3f;

        private BrawlerController controller;
        private PlayerInput input;
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
            input = GetComponent<PlayerInput>();
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

            if (input != null)
            {
                input.SetMoveInput(currentMove);
                if (currentAim.sqrMagnitude > 0.01f)
                    input.SetAimInput(currentAim);
            }

            if (target != null && !target.IsDead)
            {
                float attackRange = controller.GetData().attackRange;
                float dist = Vector2.Distance(transform.position, target.transform.position);
                if (dist <= attackRange && input != null)
                {
                    input.SetAimInput(currentAim);
                    input.OnAttackButtonDown();
                }

                if (controller.HasSuperReady && dist < controller.GetData().superRange * 1.2f && input != null)
                    input.OnSuperButtonDown();
            }
        }

        private void Think()
        {
            BrawlerController nearest = null;
            float nearestDist = float.MaxValue;

            foreach (var p in BrawlerRegistry.AllPlayers)
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

            Vector2 toEnemy = (Vector2)target.transform.position - (Vector2)transform.position;
            currentAim = toEnemy.normalized;
            float dEnemy = toEnemy.magnitude;
            float attackRange = controller.GetData().attackRange;

            Gem nearestGem = null;
            float gemDist = attackRange * 3f;
            if (GameManager.Instance != null)
            {
                foreach (var g in GameManager.Instance.ActiveGems)
                {
                    if (g == null) continue;
                    float gd = Vector2.Distance(transform.position, g.transform.position);
                    if (gd < gemDist)
                    {
                        gemDist = gd;
                        nearestGem = g;
                    }
                }
            }

            float hpPct = (float)controller.CurrentHealth / controller.MaxHealth;
            bool hasGems = false;
            if (GameManager.Instance != null && GameManager.Instance.currentGameMode is GemGrabMode gemMode)
                hasGems = gemMode.GetPlayerGems(controller) > 0;

            if (hasGems && hpPct < 0.5f)
            {
                currentMove = -toEnemy.normalized;
            }
            else if (hpPct < 0.3f)
            {
                currentMove = -toEnemy.normalized;
            }
            else if (nearestGem != null && gemDist < attackRange * 2.5f && (nearest == null || nearestDist > attackRange * 2f))
            {
                Vector2 toGem = (Vector2)nearestGem.transform.position - (Vector2)transform.position;
                currentMove = toGem.normalized;
            }
            else if (dEnemy < attackRange * 0.6f)
            {
                currentMove = new Vector2(-toEnemy.y, toEnemy.x).normalized * moveRandomness;
            }
            else if (dEnemy > attackRange * 0.85f)
            {
                currentMove = toEnemy.normalized;
            }
            else
            {
                currentMove = Vector2.zero;
            }

            RaycastHit2D hit = Physics2D.Raycast(transform.position, currentMove, 2f, LayerMask.GetMask("Obstacles"));
            if (hit.collider != null)
            {
                Vector2 perp = new Vector2(-currentMove.y, currentMove.x);
                if (Physics2D.Raycast(transform.position, perp, 1f, LayerMask.GetMask("Obstacles")).collider == null)
                    currentMove = perp;
                else
                    currentMove = -perp;
            }
        }
    }
}
