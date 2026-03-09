// 추적에 관한 스크립트
// 추적 및 포위 전술 로직
using Unity.VisualScripting;
using UnityEngine;

public class ChaseState : IEnemyState
{
    private EnemyAI enemy;
    private MonsterAnimatorController ani;
    private float cooldown = 0.2f;    //임시로 0.2초
    private float timer; 

    private float flankPathTimer = 0f; // 포위 경로 갱신 타이머
    private Vector3 currentFlankOffset; //계산된 목표지점을 저장해둘 변수
    private float rankUpdateTimer = 0f; //포위 전술 랭크(몬스터 순서) 갱신 타이머

    private LayerMask enemyLayerMask; 
    public ChaseState(EnemyAI enemy, MonsterAnimatorController ani) // ���� ���
    {
        this.enemy = enemy;
        this.ani = ani;
        enemyLayerMask = LayerMask.GetMask("Enemy");
    }

    public void Enter()
    {
        timer = 0f;
        /*if (enemy.CompareTag("Wolf"))
        {
            enemy.moveSpeed = 3.0f;
        }
        else if (enemy.CompareTag("Dwarf"))
        {
            enemy.moveSpeed = 2.3f;
        }
        else if (enemy.CompareTag("Dwarf_Hammer"))
        {
            enemy.moveSpeed = 2.3f;
        }
        else if (enemy.CompareTag("Goblin")) enemy.moveSpeed = 3f;
        else
        {
            enemy.moveSpeed = 2f;
        } 삭제 */
        enemy.moveSpeed = enemy.currentMoveSpeed;

        enemy.AlertNearbyAllies(); // 경보 발령

        if(enemy.speciesData.enableFlanking) //진입하자마자 일단 목표 지점 한번 계산 
        {
            currentFlankOffset = enemy.GetFlankingTargetPos() - enemy.player;
        }
    }
    
    
    public void Update()
    {
        if (enemy.player == null) return; 
        float distToPlayer = Vector2.Distance(enemy.transform.position, enemy.player);

        // --- 1. '결정' 로직 ---
        timer += Time.deltaTime;
        if (timer >= cooldown)
        {
            timer = 0f; 
            if (!enemy.justAlerted) enemy.AlertNearbyAllies(); 
            rankUpdateTimer += Time.deltaTime;
            // 순번 갱신 (가끔씩만)
            if (enemy.speciesData.enableFlanking && Random.value < 0.05f)
            {
                enemy.RecalculateSquadRank();
            }

            // 공격 결정 로직 수정
            bool canAttack = false;
            
            // A. 사거리 안에 있거나
            if (distToPlayer <= enemy.speciesData.attackRange) canAttack = true;
            
            // B. 포위 전술 중이면, '자리 잡기'가 끝났을 때 공격 허용 (사거리가 조금 모자라도 봐줌)
            if (enemy.speciesData.enableFlanking)
            {
                Vector3 targetPos = enemy.GetFlankingTargetPos();
                float distToTarget = Vector2.Distance(enemy.transform.position, targetPos);
                
                // "내 자리에 도착했다(0.5m 이내)" && "플레이어와 아주 멀진 않다(사거리 + 1m 이내)"
                if (distToTarget < 0.5f && distToPlayer <= enemy.speciesData.attackRange + 1.0f)
                {
                    canAttack = true; // 사거리가 닿는다고 쳐줌 (돌진 공격 유도)
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
            else if (distToPlayer > enemy.speciesData.loseDetectionRadius && !enemy.justAlerted)
            {
                enemy.lastKnownPos  = enemy.player; // 마지막 목격 위치 업데이트
                Debug.Log("[ChaseState] 플레이어를 놓쳤습니다! -> SearchState");
                enemy.ChangeState(enemy.searchState);
                
                return; 
            }
        }

        // --- 2. '행동' 로직 (떨림 방지 강화) ---
        
        bool inAttackRange = (distToPlayer <= enemy.speciesData.attackRange);
        bool inChaseRange = (distToPlayer <= enemy.speciesData.loseDetectionRadius);
        
        // 포위 전술 로직
        if (enemy.speciesData.enableFlanking)
        {
            Vector3 dynamicTargetPos = enemy.GetFlankingTargetPos();
            float distToTarget = Vector2.Distance(enemy.transform.position, dynamicTargetPos);

            // 히스테리시스 (움직일 땐 0.2, 멈췄을 땐 1.0)
            // (GetBool이 없으므로 속도로 체크하는 방식 사용)
            bool isMoving = enemy.GetRigidbody().linearVelocity.magnitude > 0.1f; 
            float tolerance = isMoving ? 0.2f : 1.0f; 

            // 전투 위치 사수 로직
            
            // "내가 지금 멈춰있고" && "플레이어가 내 공격 사거리(여유 +1m) 안에 있다"면?
            // -> 굳이 대형을 맞추러 이동하지 말고, 그 자리에서 딜을 넣어라!
            bool isInCombatRange = (distToPlayer <= enemy.speciesData.attackRange + 1.0f);
            
            if (!isMoving && isInCombatRange)
            {
                // "지금 위치도 나쁘지 않다. 굳이 움직여서 딜로스 내지 말자."
                enemy.SetPassThroughPlayer(true);
                enemy.GetRigidbody().linearVelocity = Vector2.zero;
                ani.SetMoving(false);
                enemy.FacetoPlayer();
                return; 
            }
           

            // (기존 이동 로직)
            if (distToTarget > tolerance) 
            {
                ani.SetMoving(true);
                enemy.GetAnimator().speed = 1.5f;
                enemy.SetPassThroughPlayer(true); 
                enemy.SetGhostMode(true); //이동시작이니 충돌 비활성화
                enemy.MoveToTarget(dynamicTargetPos); 
                return; 
            }
            else
            {
                enemy.SetPassThroughPlayer(true);
                enemy.SetGhostMode(false); //멈췄으니 충돌 활성화
                enemy.GetRigidbody().linearVelocity = Vector2.zero;
                ani.SetMoving(false);
                enemy.FacetoPlayer(); 
                return;
            }
        }

        // 일반 추적
        if (inAttackRange)
        {
            enemy.SetPassThroughPlayer(false);
            enemy.SetGhostMode(false); //멈췄으니 충돌 활성화
            ani.SetMoving(false);
            enemy.StopMoving();
        }
        else if (enemy.justAlerted || inChaseRange)
        {
            ani.SetMoving(true);
            enemy.GetAnimator().speed = 1.5f;
            enemy.SetPassThroughPlayer(false);
            enemy.SetGhostMode(true); //이동시작이니 충돌 비활성화
            enemy.MoveTowardsPlayer();
        }
        else
        {
            enemy.SetPassThroughPlayer(false);
            enemy.SetGhostMode(false); //멈췄으니 충돌 활성화
            ani.SetMoving(false);
            enemy.StopMoving();
        }
    }

    public void Exit()
    {
        enemy.GetAnimator().speed = 1f;

        enemy.SetPassThroughPlayer(false); // 상태 종료 시 플레이어와 충돌 활성화
        enemy.SetGhostMode(false); // 상태 종료 시 충돌 활성화
    }
}
