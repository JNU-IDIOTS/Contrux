using Unity.VisualScripting;
using UnityEngine;

public class ChaseState : IEnemyState
{
    private EnemyAI enemy;
    private MonsterAnimatorController ani;
    private float cooldown = 0.2f;
    private float timer;

    //private float flankPathTimer = 0f;
    private Vector3 currentFlankOffset;
    private float rankUpdateTimer = 0f;

    private LayerMask enemyLayerMask;
    public ChaseState(EnemyAI enemy, MonsterAnimatorController ani)
    {
        this.enemy = enemy;
        this.ani = ani;
        enemyLayerMask = LayerMask.GetMask("Enemy");
    }

    public void Enter()
    {
        timer = 0f;

        // 무전기(Squad) 호출로 변경
        bool isChaining = enemy._enemySquad.justAlerted;
        enemy._enemySquad.AlertNearbyAllies(isChaining);

        if (enemy.speciesData.enableFlanking)
        {
            currentFlankOffset = enemy._enemySquad.GetFlankingTargetPos() - enemy.player;
        }
    }

    public void Update()
    {
        if (enemy.player == null) return;
        float distToPlayer = Vector2.Distance(enemy.transform.position, enemy.player);

        timer += Time.deltaTime;
        if (timer >= cooldown)
        {
            timer = 0f;
            // 무전기(Squad) 호출로 변경
            if (!enemy._enemySquad.justAlerted) enemy._enemySquad.AlertNearbyAllies();
            rankUpdateTimer += Time.deltaTime;

            if (enemy.speciesData.enableFlanking && Random.value < 0.05f)
            {
                enemy._enemySquad.RecalculateSquadRank();
            }

            bool canAttack = false;
            if (distToPlayer <= enemy.speciesData.attackRange) canAttack = true;

            if (enemy.speciesData.enableFlanking)
            {
                Vector3 targetPos = enemy._enemySquad.GetFlankingTargetPos();
                float distToTarget = Vector2.Distance(enemy.transform.position, targetPos);

                if (distToTarget < 0.5f && distToPlayer <= enemy.speciesData.attackRange + 1.0f)
                {
                    canAttack = true;
                }
            }

            if (canAttack || enemy.IsPlayerAttackable())
            {
                float aggressionRoll = Random.Range(0f, 10f);
                if (aggressionRoll < enemy.speciesData.aggression)
                {
                    enemy.ChangeState(enemy.attackState);
                    return;
                }
            }
            // 무전기(Squad) 호출로 변경
            else if (distToPlayer > enemy.speciesData.loseDetectionRadius && !enemy._enemySquad.justAlerted)
            {
                enemy.lastKnownPos = enemy.player;
                Debug.Log("[ChaseState] 플레이어를 놓쳤습니다! -> SearchState");
                enemy.ChangeState(enemy.searchState);
                return;
            }
        }

        bool inAttackRange = (distToPlayer <= enemy.speciesData.attackRange);
        bool inChaseRange = (distToPlayer <= enemy.speciesData.loseDetectionRadius);

        if (enemy.speciesData.enableFlanking)
        {
            // 무전기(Squad) 호출로 변경
            Vector3 dynamicTargetPos = enemy._enemySquad.GetFlankingTargetPos();
            float distToTarget = Vector2.Distance(enemy.transform.position, dynamicTargetPos);

            bool isMoving = enemy.GetRigidbody().linearVelocity.magnitude > 0.1f;
            float tolerance = isMoving ? 0.2f : 1.0f;
            bool isInCombatRange = (distToPlayer <= enemy.speciesData.attackRange + 1.0f);

            if (!isMoving && isInCombatRange)
            {
                enemy.SetPassThroughPlayer(true);
                enemy.GetRigidbody().linearVelocity = Vector2.zero;
                ani.SetMoving(false);
                enemy._enemyMovement.FacetoPlayer();
                return;
            }

            if (distToTarget > tolerance)
            {
                ani.SetMoving(true);
                enemy.GetAnimator().speed = 1.5f;
                enemy.SetPassThroughPlayer(true);
                enemy.SetGhostMode(true);
                enemy._enemyMovement.MoveToTarget(dynamicTargetPos);
                return;
            }
            else
            {
                enemy.SetPassThroughPlayer(true);
                enemy.SetGhostMode(false);
                enemy.GetRigidbody().linearVelocity = Vector2.zero;
                ani.SetMoving(false);
                enemy._enemyMovement.FacetoPlayer();
                return;
            }
        }

        if (inAttackRange)
        {
            enemy.SetPassThroughPlayer(false);
            enemy.SetGhostMode(false);
            ani.SetMoving(false);
            enemy._enemyMovement.StopMoving();
        }
        // 무전기(Squad) 호출로 변경
        else if (enemy._enemySquad.justAlerted || inChaseRange)
        {
            ani.SetMoving(true);
            enemy.GetAnimator().speed = 1.5f;
            enemy.SetPassThroughPlayer(false);
            enemy.SetGhostMode(true);
            enemy._enemyMovement.MoveTowardsPlayer();
        }
        else
        {
            enemy.SetPassThroughPlayer(false);
            enemy.SetGhostMode(false);
            ani.SetMoving(false);
            enemy._enemyMovement.StopMoving();
        }
    }

    public void Exit()
    {
        enemy.GetAnimator().speed = 1f;
        enemy.SetPassThroughPlayer(false);
        enemy.SetGhostMode(false);
    }
}