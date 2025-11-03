using Unity.VisualScripting;
using UnityEngine;
using System.Collections;
using static UnityEngine.RuleTile.TilingRuleOutput;
using UnityEngine.EventSystems;
using System.Diagnostics;

public class IdleState : IEnemyState
{
    private EnemyAI enemy;
    private MonsterAnimatorController ani;
    private float idleDuration;
    private float timer;
    private float moveTimer;
    private float moveDuration;

    // === 이동 동작 제어용 변수들 ===
    private float movePhaseTimer = 0f;
    private float movePhaseDuration = 0f;
    private int moveDir = 0;
    private float originalspeed = 0f;
    private int groundLayer;
    private int floatGroundLayer;

    private bool isCooldown = false;
    private Coroutine cooldownCoroutine;

    [Header("낙하 감지 설정")]
    [SerializeField] public static float raycastDistance = 0.5f;
    [SerializeField] public static float horizontalOffset = 0.2f;
    [SerializeField] public static float verticalOffset = 0.1f;

    public IdleState(EnemyAI enemy, MonsterAnimatorController ani)
    {
        this.enemy = enemy;
        this.ani = ani;
    }

    public void Enter()
    {
        timer = 0f;
        moveTimer = 0f;
        idleDuration = Random.Range(1f, 3f);
        moveDuration = Random.Range(1f, 3f);
        enemy.moveSpeed = originalspeed;

        groundLayer = LayerMask.NameToLayer("Ground");
        floatGroundLayer = LayerMask.NameToLayer("FloatGround");
        movePhaseTimer = 0f;
        movePhaseDuration = Random.Range(1f, 2f);  // 첫 상태 지속시간
        moveDir = 0;
        enemy.moveSpeed = 1f;
        enemy.StopMoving();
        ani.SetMoving(false);
    }

    public void Update()
    {
        enemy.moveSpeed = 1.0f;

        timer += Time.deltaTime;
        moveTimer += Time.deltaTime;

        if (enemy.waitScore > 5 && enemy.HasGroundAbove())
        {
            enemy.Jump();  // 점프를 하는 로직은 그대로 유지
        }
        else
        {
            MoveHorizontally();
        }

        // 플레이어가 가까이 오면 추적 상태로 전환
        if (enemy.IsPlayerInRange() && enemy.currentState != enemy.chaseState)
        {
            enemy.ChangeState(enemy.chaseState);
            UnityEngine.Debug.Log("추적모드 전환");
            return;
        }
    }

    //private void MoveHorizontally()
    //{
    //    if (isCooldown)
    //    {
    //        enemy.GetRigidbody().linearVelocity = new Vector2(0, enemy.GetRigidbody().linearVelocity.y);
    //        ani.SetMoving(false);  // 확실하게 Idle 모드
    //        return;
    //    }

    //    float enemyspeed = 0.7f;
    //    movePhaseTimer += Time.deltaTime;

    //    if (movePhaseTimer >= movePhaseDuration)
    //    {
    //        movePhaseTimer = 0f;
    //        movePhaseDuration = Random.Range(1f, 3f);
    //        if (moveDir == -1 || moveDir == 1)
    //        {
    //            moveDir = 0;
    //            enemy.GetRigidbody().linearVelocity = new Vector2(0, enemy.GetRigidbody().linearVelocity.y);
    //            ani.SetMoving(false);
    //            enemy.StartCooldown(1f);
    //            //UnityEngine.Debug.Log("이동 후 정지 상태로 진입");
    //            return;
    //        }
    //        float rand = Random.value;
    //        if (rand < 0.5f)
    //        {
    //            moveDir = 0;
    //            //UnityEngine.Debug.Log("생각 중 (아무 방향도 선택 안 함)");
    //        }
    //        else
    //        {
    //            int newDir;
    //            do
    //            {
    //                newDir = Random.Range(0, 2) * 2 - 1;
    //            } while (newDir == moveDir && Random.value < 0.5f);

    //            moveDir = newDir;
    //            //UnityEngine.Debug.Log("이동 방향 선택: " + moveDir);
    //        }
    //    }

    //    // ✅ moveDir에 따라 이동/정지 처리 분리
    //    if (moveDir != 0)
    //    {
    //        // 1. 자식 오브젝트 중 "FallingDetection" 찾기
    //        UnityEngine.Transform fallingDetection = enemy.transform.Find("FallingDetection");

    //        if (fallingDetection != null)
    //        {
    //            // 2. 해당 오브젝트에 붙은 BoxCollider2D 가져오기
    //            BoxCollider2D collider = fallingDetection.GetComponent<BoxCollider2D>();

    //            if (collider != null)
    //            {
    //                // 3. groundLayer 레이어 마스크 준비
    //                LayerMask groundLayer = LayerMask.GetMask("Ground");

    //                // 4. 땅과 닿아 있지 않다면 방향 반전
    //                if (!collider.IsTouchingLayers(groundLayer))
    //                {
    //                    UnityEngine.Debug.Log($" 변경 전{moveDir}");
    //                    moveDir *= -1;
    //                    UnityEngine.Debug.Log($" 변경 후{moveDir}");
    //                }
    //            }
    //            else
    //            {
    //                UnityEngine.Debug.LogWarning("FallingDetection에는 BoxCollider2D가 없습니다.");
    //            }
    //        }
    //        else
    //        {
    //            //UnityEngine.Debug.LogWarning("FallingDetection 자식 오브젝트를 찾을 수 없습니다.");
    //        }
    //        enemy.GetRigidbody().linearVelocity = new Vector2(moveDir * enemyspeed, enemy.GetRigidbody().linearVelocity.y);
    //        ani.SetMoving(true);
    //        enemy.FaceDirection(moveDir);
    //    }
    //    else
    //    {
    //        enemy.GetRigidbody().linearVelocity = new Vector2(0, enemy.GetRigidbody().linearVelocity.y);
    //        ani.SetMoving(false);
    //        //UnityEngine.Debug.Log("Idle 이동 중");
    //    }
    //}

    private void MoveHorizontally()
    {

        if (isCooldown)
        {
            enemy.GetRigidbody().linearVelocity = new Vector2(0, enemy.GetRigidbody().linearVelocity.y);
            ani.SetMoving(false);  // 확실하게 Idle 모드
            return;
        }

        float enemyspeed = 0.7f;
        movePhaseTimer += Time.deltaTime;

        if (movePhaseTimer >= movePhaseDuration)
        {
            movePhaseTimer = 0f;
            movePhaseDuration = Random.Range(1f, 3f);
            if (moveDir == -1 || moveDir == 1)
            {
                moveDir = 0;
                enemy.GetRigidbody().linearVelocity = new Vector2(0, enemy.GetRigidbody().linearVelocity.y);
                ani.SetMoving(false);
                enemy.StartCooldown(1f);
                //UnityEngine.Debug.Log("이동 후 정지 상태로 진입");
                return;
            }
            float rand = Random.value;
            if (rand < 0.5f)
            {
                moveDir = 0;
                UnityEngine.Debug.Log("생각 중 (아무 방향도 선택 안 함)");
            }
            else
            {
                int newDir;
                do
                {
                    newDir = Random.Range(0, 2) * 2 - 1;
                } while (newDir == moveDir && Random.value < 0.5f);

                moveDir = newDir;
                UnityEngine.Debug.Log("이동 방향 선택: " + moveDir);
            }
        }

        // ✅ moveDir에 따라 이동/정지 처리 분리
        if (moveDir != 0)
        {
            bool onGround = false;
            Vector2 rayOrigin = new Vector2(
                enemy.transform.position.x + (horizontalOffset * moveDir),
                enemy.transform.position.y - verticalOffset
            );

            Vector2 rayDirection = Vector2.down;

            UnityEngine.Debug.DrawRay(rayOrigin, rayDirection * raycastDistance, Color.red, 0.1f);
            //Vector2 rayOrigin = new Vector2(transform.position.x, transform.position.y - rayOriginOffsetY);

            // 바닥을 향한 Ray 발사
            //RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, rayDistance, groundMask);
            //RaycastHit2D hit = Physics2D.Raycast(rayOrigin, rayDirection, raycastDistance);
            RaycastHit2D[] hits = Physics2D.RaycastAll(rayOrigin, rayDirection, raycastDistance);
            // Scene 뷰에서 디버그 Ray 표시 (초록: 바닥 닿음, 빨강: 공중)
            UnityEngine.Debug.DrawRay(rayOrigin, Vector2.down * raycastDistance, hits.Length > 0 ? Color.green : Color.red);

            foreach (var hit in hits)
            {
                if (hit.collider != null
                    && hit.collider.gameObject.layer == groundLayer || hit.collider.gameObject.layer == floatGroundLayer)
                {
                    onGround = true;
                    break;
                }
            }
            // 닿았는지 여부 판단

            // 4. 땅과 닿아 있지 않다면 방향 반전
            if (!onGround)// 변경
            {
                UnityEngine.Debug.Log("안닿음");
                UnityEngine.Debug.Log($"[FallingDetection Raycast] 변경 전: {moveDir}");
                moveDir *= -1;
                UnityEngine.Debug.Log($"[FallingDetection Raycast] 변경 후: {moveDir}");

                UnityEngine.Debug.DrawRay(rayOrigin, rayDirection * raycastDistance, Color.yellow, 0.1f);
            }
            else
            {
                UnityEngine.Debug.DrawRay(rayOrigin, rayDirection * raycastDistance, Color.green, 0.1f); // 변경
            }
            enemy.GetRigidbody().linearVelocity = new Vector2(moveDir * enemyspeed, enemy.GetRigidbody().linearVelocity.y);
            ani.SetMoving(true);
            enemy.FaceDirection(moveDir);
        }
        else
        {
            enemy.GetRigidbody().linearVelocity = new Vector2(0, enemy.GetRigidbody().linearVelocity.y);
            ani.SetMoving(false);
            //UnityEngine.Debug.Log("Idle 이동 중");
        }
    }
    public void Exit()
    {

    }
}
