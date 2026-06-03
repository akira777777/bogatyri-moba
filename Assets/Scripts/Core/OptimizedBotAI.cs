using UnityEngine;
using System.Collections.Generic;
using BogatyriMoba.GameModes;

namespace BogatyriMoba.Core
{
    /// <summary>
    /// State-machine based AI with optimized spatial queries and tactical decision making.
    /// Uses SpatialHashGrid for O(1) neighbor queries instead of linear search.
    /// </summary>
    [DefaultExecutionOrder(50)]
    [RequireComponent(typeof(BrawlerController))]
    public class OptimizedBotAI : MonoBehaviour
    {
        public enum AIState
        {
            Idle,
            Patrol,
            Chase,
            Attack,
            Retreat,
            CollectGem
        }

        [Header("Settings")]
        [SerializeField] private float reactionTime = 0.3f;
        [SerializeField] private float stateUpdateInterval = 0.2f;
        [SerializeField] private float memoryDuration = 3f;
        [SerializeField] private float visionRadius = 12f;
        [SerializeField] private LayerMask obstacleMask;

        private BrawlerController controller;
        private PlayerInput input;
        private AIState currentState = AIState.Idle;
        private float stateTimer;
        private float reactionTimer;
        private Vector2 currentMove;
        private Vector2 currentAim;

        // Target tracking
        private BrawlerController targetEnemy;
        private Gem targetGem;
        private float lastTargetSeenTime;
        private Vector2 lastKnownEnemyPos;

        // Optimization
        private Transform cachedTransform;
        private float visionRadiusSqr;
        private static SpatialHashGrid _spatialGrid;
        private float _gridUpdateTimer;   // per-instance so bots don't all spike on the same frame
        private const float GridUpdateInterval = 0.5f;

        public void Initialize(BrawlerController brawler)
        {
            controller = brawler;
            cachedTransform = transform;
            visionRadiusSqr = visionRadius * visionRadius;
        }

        private void Awake()
        {
            if (controller == null)
                controller = GetComponent<BrawlerController>();
            input = GetComponent<PlayerInput>();
            cachedTransform = transform;
            obstacleMask = LayerMask.GetMask("Obstacles");
            visionRadiusSqr = visionRadius * visionRadius;

            if (_spatialGrid == null)
                _spatialGrid = new SpatialHashGrid(4f);

            // Stagger initial update so bots don't all spike on the same frame
            _gridUpdateTimer = Random.Range(0f, GridUpdateInterval);
        }

        private void OnEnable()
        {
            if (controller != null)
                _spatialGrid?.Insert(controller);
        }

        private void OnDisable()
        {
            if (controller != null)
                _spatialGrid?.Remove(controller);
        }

        private void Update()
        {
            if (controller == null || controller.IsDead) return;

            // Update spatial grid periodically
            _gridUpdateTimer -= Time.deltaTime;
            if (_gridUpdateTimer <= 0f)
            {
                _gridUpdateTimer = GridUpdateInterval;
                _spatialGrid?.UpdateEntity(controller);
            }

            stateTimer -= Time.deltaTime;
            reactionTimer -= Time.deltaTime;

            if (stateTimer <= 0f)
            {
                stateTimer = stateUpdateInterval;
                EvaluateState();
            }

            ExecuteCurrentState();

            if (reactionTimer <= 0f)
            {
                reactionTimer = reactionTime;
                Think();
            }

            ApplyInput();
        }

        private void EvaluateState()
        {
            var data = controller.GetData();
            if (data == null) return;

            float hpPct = (float)controller.CurrentHealth / controller.MaxHealth;
            bool hasGems = HasGems();

            // Priority 1: Retreat if low HP and has gems
            if (hasGems && hpPct < 0.4f)
            {
                TransitionTo(AIState.Retreat);
                return;
            }

            // Priority 2: Retreat if critical HP
            if (hpPct < 0.25f)
            {
                TransitionTo(AIState.Retreat);
                return;
            }

            // Find targets using spatial grid
            FindNearestEnemyOptimized();
            FindNearestGem();

            float attackRange = data.attackRange;

            if (targetEnemy != null && !targetEnemy.IsDead)
            {
                float distSqr = Vector2.SqrMagnitude(
                    (Vector2)targetEnemy.transform.position - (Vector2)cachedTransform.position);
                float attackRangeSqr = attackRange * attackRange;

                if (distSqr <= attackRangeSqr * 0.81f)
                {
                    TransitionTo(AIState.Attack);
                    return;
                }

                if (distSqr <= attackRangeSqr * 16f)
                {
                    TransitionTo(AIState.Chase);
                    return;
                }
            }

            // Priority 3: Collect gems if nearby and no immediate threat
            float collectRangeSqr = attackRange * attackRange * 4f;
            if (targetGem != null && (targetEnemy == null ||
                Vector2.SqrMagnitude((Vector2)targetGem.transform.position - (Vector2)cachedTransform.position) < collectRangeSqr))
            {
                TransitionTo(AIState.CollectGem);
                return;
            }

            // Default: Patrol
            TransitionTo(AIState.Patrol);
        }

        private void ExecuteCurrentState()
        {
            switch (currentState)
            {
                case AIState.Attack:
                    ExecuteAttack();
                    break;
                case AIState.Chase:
                    ExecuteChase();
                    break;
                case AIState.Retreat:
                    ExecuteRetreat();
                    break;
                case AIState.CollectGem:
                    ExecuteCollectGem();
                    break;
                case AIState.Patrol:
                    ExecutePatrol();
                    break;
            }
        }

        private void ExecuteAttack()
        {
            if (targetEnemy == null || targetEnemy.IsDead)
            {
                currentMove = Vector2.zero;
                return;
            }

            Vector2 toEnemy = (Vector2)targetEnemy.transform.position - (Vector2)cachedTransform.position;
            currentAim = toEnemy.normalized;
            float dist = toEnemy.magnitude;
            float attackRange = controller.GetData().attackRange;

            // Strafe at optimal distance
            if (dist < attackRange * 0.5f)
            {
                currentMove = -toEnemy.normalized * 0.5f;
            }
            else if (dist > attackRange * 0.85f)
            {
                currentMove = toEnemy.normalized * 0.7f;
            }
            else
            {
                // Strafe sideways
                Vector2 perp = new Vector2(-toEnemy.y, toEnemy.x).normalized;
                currentMove = perp * 0.6f;
            }

            // Use super if ready and close
            if (controller.HasSuperReady && dist < controller.GetData().superRange * 1.2f)
            {
                input?.OnSuperButtonDown();
            }
        }

        private void ExecuteChase()
        {
            if (targetEnemy == null)
            {
                currentMove = Vector2.zero;
                return;
            }

            Vector2 toEnemy = (Vector2)targetEnemy.transform.position - (Vector2)cachedTransform.position;
            currentAim = toEnemy.normalized;
            currentMove = NavigateAroundObstacles(toEnemy.normalized);
        }

        private void ExecuteRetreat()
        {
            Vector2 retreatDir = Vector2.zero;

            if (targetEnemy != null)
            {
                retreatDir = ((Vector2)cachedTransform.position - (Vector2)targetEnemy.transform.position).normalized;
            }
            else
            {
                // Retreat to spawn
                var spawnPoints = controller.TeamId == 0
                    ? SpawnManager.Instance?.team1SpawnPoints
                    : SpawnManager.Instance?.team2SpawnPoints;

                if (spawnPoints != null && spawnPoints.Length > 0)
                {
                    retreatDir = ((Vector2)spawnPoints[0].position - (Vector2)cachedTransform.position).normalized;
                }
            }

            currentAim = retreatDir;
            currentMove = NavigateAroundObstacles(retreatDir);
        }

        private void ExecuteCollectGem()
        {
            if (targetGem == null)
            {
                currentMove = Vector2.zero;
                return;
            }

            Vector2 toGem = (Vector2)targetGem.transform.position - (Vector2)cachedTransform.position;
            currentAim = toGem.normalized;
            currentMove = NavigateAroundObstacles(toGem.normalized);
        }

        private void ExecutePatrol()
        {
            // Random wandering with obstacle avoidance
            if (currentMove.sqrMagnitude < 0.01f || Random.value < 0.02f)
            {
                currentMove = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
            }
            currentAim = currentMove;
            currentMove = NavigateAroundObstacles(currentMove);
        }

        private Vector2 NavigateAroundObstacles(Vector2 desiredDir)
        {
            if (desiredDir.sqrMagnitude < 0.001f) return desiredDir;

            RaycastHit2D hit = Physics2D.Raycast(cachedTransform.position, desiredDir, 1.5f, obstacleMask);
            if (hit.collider == null) return desiredDir;

            // Try left and right
            Vector2 left = new Vector2(-desiredDir.y, desiredDir.x).normalized;
            Vector2 right = -left;

            bool leftClear = Physics2D.Raycast(cachedTransform.position, left, 1.5f, obstacleMask).collider == null;
            bool rightClear = Physics2D.Raycast(cachedTransform.position, right, 1.5f, obstacleMask).collider == null;

            if (leftClear && !rightClear) return left;
            if (rightClear && !leftClear) return right;
            if (leftClear && rightClear) return Random.value > 0.5f ? left : right;

            // Back up
            return -desiredDir;
        }

        private void Think()
        {
            if (targetEnemy != null && !targetEnemy.IsDead)
            {
                float dist = Vector2.Distance(cachedTransform.position, targetEnemy.transform.position);
                if (dist <= controller.GetData().attackRange && input != null)
                {
                    input.SetAimInput(currentAim);
                    input.OnAttackButtonDown();
                }
            }
        }

        private void ApplyInput()
        {
            if (input == null) return;
            input.SetMoveInput(currentMove);
            if (currentAim.sqrMagnitude > 0.01f)
                input.SetAimInput(currentAim);
        }

        private void FindNearestEnemyOptimized()
        {
            if (_spatialGrid == null)
            {
                FindNearestEnemyFallback();
                return;
            }

            var nearest = _spatialGrid.FindNearest(
                cachedTransform.position,
                visionRadius,
                p => p != controller && !p.IsDead && p.TeamId != controller.TeamId
            );

            if (nearest != null)
            {
                targetEnemy = nearest;
                lastKnownEnemyPos = nearest.transform.position;
                lastTargetSeenTime = Time.time;
            }
            else if (Time.time - lastTargetSeenTime > memoryDuration)
            {
                targetEnemy = null;
            }
        }

        private void FindNearestEnemyFallback()
        {
            var nearest = BrawlerRegistry.FindNearestEnemy(
                cachedTransform.position,
                visionRadiusSqr,
                controller,
                controller.TeamId);

            if (nearest != null)
            {
                targetEnemy = nearest;
                lastKnownEnemyPos = nearest.transform.position;
                lastTargetSeenTime = Time.time;
            }
            else if (Time.time - lastTargetSeenTime > memoryDuration)
            {
                targetEnemy = null;
            }
        }

        private void FindNearestGem()
        {
            Gem nearest = null;
            float nearestDist = 25f * 25f; // Max search range squared

            if (SpawnManager.Instance != null)
            {
                foreach (var g in SpawnManager.Instance.ActiveGems)
                {
                    if (g == null) continue;
                    float d = Vector2.SqrMagnitude((Vector2)g.transform.position - (Vector2)cachedTransform.position);
                    if (d < nearestDist)
                    {
                        nearestDist = d;
                        nearest = g;
                    }
                }
            }

            targetGem = nearest;
        }

        private bool HasGems()
        {
            if (MatchManager.Instance != null && MatchManager.Instance.currentGameMode is GemGrabMode gemMode)
                return gemMode.GetPlayerGems(controller) > 0;
            return false;
        }

        private void TransitionTo(AIState newState)
        {
            if (currentState == newState) return;
            currentState = newState;
        }

        private void OnDestroy()
        {
            if (controller != null)
                _spatialGrid?.Remove(controller);
        }
    }
}
