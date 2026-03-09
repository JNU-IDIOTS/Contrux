// Idle 상태에서의 행동과 로직을 담당하는 스크립트
using UnityEngine;
using System.Collections;

public class IdleState : IEnemyState
{
    private EnemyAI enemy;
    private MonsterAnimatorController ani;

    // --- 1. 타이머 변수 정리 ---
    // (기존 timer, moveTimer, idleDuration, moveDuration 등 삭제)
    private float movePhaseTimer = 0f;    // 행동/생각 타이머
    private float movePhaseDuration = 0f; // 행동/생각 지속시간
    private int moveDir = 0;              // 현재 이동 방향 (0:정지, 1:우, -1:좌)

    // --- 2. 지형 레이어 캐시 ---
    private int groundLayer;
    private int floatGroundLayer;

    // (isCooldown, cooldownCoroutine 변수 삭제)

    public IdleState(EnemyAI enemy, MonsterAnimatorController ani)
    {
        this.enemy = enemy;
        this.ani = ani;
    }

    public void Enter()
    {
        // --- 3. Enter 함수 정리 ---
        groundLayer = LayerMask.NameToLayer("Ground");
        floatGroundLayer = LayerMask.NameToLayer("FloatGround");
        
        movePhaseTimer = 0f;
        movePhaseDuration = Random.Range(1f, 2f);  // 첫 행동 결정까지의 시간
        moveDir = 0;
        
        enemy.StopMoving();
        ani.SetMoving(false);
    }

    public void Update()
    {
        // --- 4. Update 함수 정리 ---
        // (enemy.moveSpeed = 1.0f; 삭제)
        // (timer, moveTimer += Time.deltaTime; 삭제)

        // (점프 로직은 그대로 유지)
        if (enemy.waitScore > 5 && enemy.HasGroundAbove())
        {
            enemy.Jump();
        }
        else
        {
            MoveHorizontally();
        }

        // --- 5. 플레이어 탐지 로직 (Vector2.Distance 사용) ---
        float distance = Vector2.Distance(enemy.transform.position, enemy.player);
        if (distance < enemy.speciesData.detectionRadius)
        {
            if (enemy.currentCourage > (enemy.speciesData.maxCourage * 0.3f))
            {
                enemy.ChangeState(enemy.chaseState);
            }
            else if (enemy.currentCourage <= 0)
            {
                enemy.ChangeState(enemy.fleeState);
            }
        }
    }

    // --- 6. MoveHorizontally 함수 (로직 수정) ---
    private void MoveHorizontally()
    {
        // 1. 타이머는 항상 흐른다.
        movePhaseTimer += Time.deltaTime;

        // 2. 다음 행동을 '결정'할 시간이 되었는가?
        if (movePhaseTimer >= movePhaseDuration)
        {
            movePhaseTimer = 0f; // 타이머 리셋
            movePhaseDuration = Random.Range(1.5f, 4.0f); // 다음 결정까지 1.5~4초

            // 3. 다음 행동 결정 (이동 or 정지)
            float decision = Random.value; 
            if (decision < 0.4f)
            {
                moveDir = 0; // 40% 확률로 '정지'
                Debug.Log("[IdleState] 결정: 정지");
            }
            else if (decision < 0.7f)
            {
                moveDir = 1; // 30% 확률로 '오른쪽 이동'
                Debug.Log("[IdleState] 결정: 오른쪽");
            }
            else
            {
                moveDir = -1; // 30% 확률로 '왼쪽 이동'
                Debug.Log("[IdleState] 결정: 왼쪽");
            }
        }

        // --- (수정) '이동'과 '정지' 로직을 명확히 분리 ---

        // 4. '이동'이 결정된 상태인가?
        if (moveDir != 0)
        {
            // 4-1. 절벽 감지
            bool onGround = false;
            Vector2 rayOrigin = new Vector2(
                enemy.transform.position.x + (enemy.speciesData.horizontalOffset * moveDir),
                enemy.transform.position.y - enemy.speciesData.verticalOffset
            );
            Vector2 rayDirection = Vector2.down;
            float distance = enemy.speciesData.raycastDistance;

            RaycastHit2D[] hits = Physics2D.RaycastAll(rayOrigin, rayDirection, distance, 1 << groundLayer | 1 << floatGroundLayer); // ★수정★ 레이어마스크 사용
            
            if (hits.Length > 0)
            {
                onGround = true;
            }

            // 4-2. 절벽 감지! '즉시' 멈추고 '다음 프레임'에 다시 생각.
            if (!onGround)
            {
                Debug.Log("[IdleState] 절벽 감지! 멈춤!");
                moveDir = 0; // 이동 취소
                movePhaseTimer = 0f; // ★수정★ 타이머 리셋
                movePhaseDuration = Random.Range(1f, 2f); // ★수정★ 멈춰서 생각할 시간 (1~2초)
            }
        }
        
        // 5. 'moveDir' 값에 따라 '행동'
        if (moveDir == 0)
        {
            // 멈춤
            enemy.SetGhostMode(false); // 멈췄으니 충돌 활성화
            enemy.GetRigidbody().linearVelocity = new Vector2(0, enemy.GetRigidbody().linearVelocity.y);
            ani.SetMoving(false);
        }
        else
        {
            // 이동
            enemy.SetGhostMode(true); // 이동 중이니 충돌 비활성화
            float enemyspeed = enemy.speciesData.patrolSpeed;
            enemy.GetRigidbody().linearVelocity = new Vector2(moveDir * enemyspeed, enemy.GetRigidbody().linearVelocity.y);
            ani.SetMoving(true);
            enemy.FaceDirection(moveDir);
            
            // ★디버그★ (이동이 실행되는지 확인)
            // Debug.Log($"[IdleState] 이동 중... 방향: {moveDir}, 속도: {enemyspeed}");
        }
    }

    public void Exit()
    {
        enemy.SetGhostMode(false); // 상태 종료 시 충돌 활성화
    }
}