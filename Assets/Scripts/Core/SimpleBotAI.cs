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
        private int obstacleMask;

        public void Initialize(BrawlerController brawler)
        {
            controller = brawler;
        }

        private void Awake()
        {
            if (controller == null)
                controller = GetComponent<BrawlerController>();
            input = GetComponent<PlayerInput>();
            obstacleMask = LayerMask.GetMask("Obstacles");
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
                float distSqr = Vector2.SqrMagnitude((Vector2)target.transform.position - (Vector2)transform.position);
                if (distSqr <= attackRange * attackRange && input != null)
                {
                    input.SetAimInput(currentAim);
                    input.OnAttackButtonDown();
                }

                float superRange = controller.GetData().superRange * 1.2f;
                if (controller.HasSuperReady && distSqr <= superRange * superRange && input != null)
                    input.OnSuperButtonDown();
            }
        }

        private void Think()
        {
            target = BrawlerRegistry.FindNearestEnemy(
                transform.position,
                float.MaxValue,
                controller,
                controller.TeamId);

            if (target == null)
            {
                currentMove = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
                return;
            }

            Vector2 toEnemy = (Vector2)target.transform.position - (Vector2)transform.position;
            currentAim = toEnemy.normalized;
            float dEnemySqr = toEnemy.sqrMagnitude;
            float dEnemy = Mathf.Sqrt(dEnemySqr);
            float attackRange = controller.GetData().attackRange;

            Gem nearestGem = null;
            float gemDistSqr = attackRange * attackRange * 9f;
            var gems = SpawnManager.Instance != null
                ? SpawnManager.Instance.ActiveGems
                : GameManager.Instance?.ActiveGems;
            if (gems != null)
            {
                for (int i = 0; i < gems.Count; i++)
                {
                    var g = gems[i];
                    if (g == null) continue;
                    float gd = Vector2.SqrMagnitude((Vector2)g.transform.position - (Vector2)transform.position);
                    if (gd < gemDistSqr)
                    {
                        gemDistSqr = gd;
                        nearestGem = g;
                    }
                }
            }

            float hpPct = (float)controller.CurrentHealth / controller.MaxHealth;
            bool hasGems = false;
            var mode = MatchManager.Instance != null
                ? MatchManager.Instance.currentGameMode
                : GameManager.Instance?.currentGameMode;
            if (mode is GemGrabMode gemMode)
                hasGems = gemMode.GetPlayerGems(controller) > 0;

            if (hasGems && hpPct < 0.5f)
            {
                currentMove = -toEnemy.normalized;
            }
            else if (hpPct < 0.3f)
            {
                currentMove = -toEnemy.normalized;
            }
            else if (nearestGem != null && gemDistSqr < attackRange * attackRange * 6.25f &&
                     dEnemySqr > attackRange * attackRange * 4f)
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

            RaycastHit2D hit = Physics2D.Raycast(transform.position, currentMove, 2f, obstacleMask);
            if (hit.collider != null)
            {
                Vector2 perp = new Vector2(-currentMove.y, currentMove.x);
                if (Physics2D.Raycast(transform.position, perp, 1f, obstacleMask).collider == null)
                    currentMove = perp;
                else
                    currentMove = -perp;
            }
        }
    }
}
