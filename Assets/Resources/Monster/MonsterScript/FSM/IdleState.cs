using UnityEngine;
using System.Collections;

public class IdleState : IEnemyState
{
    private EnemyAI enemy;
    private MonsterAnimatorController ani;

    private float movePhaseTimer = 0f;
    private float movePhaseDuration = 0f;
    private int moveDir = 0;

    private int groundLayer;
    private int floatGroundLayer;

    public IdleState(EnemyAI enemy, MonsterAnimatorController ani)
    {
        this.enemy = enemy;
        this.ani = ani;
    }

    public void Enter()
    {
        groundLayer = LayerMask.NameToLayer("Ground");
        floatGroundLayer = LayerMask.NameToLayer("FloatGround");

        movePhaseTimer = 0f;
        movePhaseDuration = Random.Range(1f, 2f);
        moveDir = 0;

        enemy._enemyMovement.StopMoving();
        ani.SetMoving(false);
    }

    public void Update()
    {
        bool hasGroundAbove = Physics2D.Raycast(enemy.transform.position, Vector2.up, 1f, LayerMask.GetMask("Ground")).collider != null;

        if (enemy.waitScore > 5 && hasGroundAbove)
        {
            enemy._enemyMovement.Jump();
        }
        else
        {
            MoveHorizontally();
        }

        float distance = Vector2.Distance(enemy.transform.position, enemy.player);
        if (distance < enemy.speciesData.detectionRadius)
        {
            // 심장(Health) 호출로 변경
            if (enemy._enemyHealth.currentCourage > (enemy.speciesData.maxCourage * 0.3f))
            {
                enemy.ChangeState(enemy.chaseState);
            }
            else if (enemy._enemyHealth.currentCourage <= 0)
            {
                enemy.ChangeState(enemy.fleeState);
            }
        }
    }

    private void MoveHorizontally()
    {
        movePhaseTimer += Time.deltaTime;

        if (movePhaseTimer >= movePhaseDuration)
        {
            movePhaseTimer = 0f;
            movePhaseDuration = Random.Range(1.5f, 4.0f);

            float decision = Random.value;
            if (decision < 0.4f)
            {
                moveDir = 0;
            }
            else if (decision < 0.7f)
            {
                moveDir = 1;
            }
            else
            {
                moveDir = -1;
            }
        }

        if (moveDir != 0)
        {
            bool onGround = false;
            Vector2 rayOrigin = new Vector2(
                enemy.transform.position.x + (enemy.speciesData.horizontalOffset * moveDir),
                enemy.transform.position.y - enemy.speciesData.verticalOffset
            );
            Vector2 rayDirection = Vector2.down;
            float distance = enemy.speciesData.raycastDistance;

            RaycastHit2D[] hits = Physics2D.RaycastAll(rayOrigin, rayDirection, distance, 1 << groundLayer | 1 << floatGroundLayer);

            if (hits.Length > 0)
            {
                onGround = true;
            }

            if (!onGround)
            {
                moveDir = 0;
                movePhaseTimer = 0f;
                movePhaseDuration = Random.Range(1f, 2f);
            }
        }

        if (moveDir == 0)
        {
            enemy.SetGhostMode(false);
            enemy.GetRigidbody().linearVelocity = new Vector2(0, enemy.GetRigidbody().linearVelocity.y);
            ani.SetMoving(false);
        }
        else
        {
            enemy.SetGhostMode(true);
            float enemyspeed = enemy.speciesData.patrolSpeed;
            enemy.GetRigidbody().linearVelocity = new Vector2(moveDir * enemyspeed, enemy.GetRigidbody().linearVelocity.y);
            ani.SetMoving(true);
            enemy._enemyMovement.FaceDirection(moveDir);
        }
    }

    public void Exit()
    {
        enemy.SetGhostMode(false);
    }
}