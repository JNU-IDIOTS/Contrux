// 플레이어를 놓친 후, 마지막으로 플레이어가 보였던 위치로 이동하여 주변을 수색하는 행동을 담당하는 스크립트
using UnityEngine;

public class SearchState : IEnemyState
{
    private EnemyAI enemy;
    private MonsterAnimatorController ani;
    
    private float searchTimer = 0f; //수색 대기 시간
    private float searchDuration = 3.0f; //두리번두리번 거리는 시간(후에 애니메이션 추가 예상)
    private bool arrivedAtLocation = false;
    private float moveTimeout = 0.0f; // 만약 수색 목표 위치가 몬스터가 못가는 곳에 있을경우 수색 무한 루프에 빠지는걸 방지하기 위한 시간 제한

    // 개별 목표 지점 변수 
    private Vector3 searchTargetPos; 

    public SearchState(EnemyAI enemy, MonsterAnimatorController ani)
    {
        this.enemy = enemy;
        this.ani = ani;
    }

    public void Enter()
    {
        Debug.Log($"[SearchState] 마지막 위치({enemy.lastKnownPos}) 수색 시작!");
        searchTimer = 0f;
        arrivedAtLocation = false;
        moveTimeout = 0.0f;

        // 똑같은 곳이 아니라, 반경 2m 내 랜덤한 곳으로 퍼지게 설정 
        Vector2 randomOffset = Random.insideUnitCircle * 2.0f; 
        searchTargetPos = enemy.lastKnownPos + new Vector3(randomOffset.x, randomOffset.y, 0);
        

        enemy.moveSpeed = enemy.speciesData.chaseSpeed; 
        //enemy.SetPassThroughPlayer(true); 플레이어 통과 함수 일단 혹시 모르니 주석처리
        enemy.SetGhostMode(true);
        ani.SetMoving(true);
        
    }

    public void Update()
    {
        if (!arrivedAtLocation)
        {
            moveTimeout += Time.deltaTime;
            
            float distToTarget = Vector2.Distance(enemy.transform.position, searchTargetPos);
            
            if (distToTarget < 0.5f || moveTimeout > 3.0f) //목표 지점에 도착했거나, 3초 이상 움직이지 못했을 경우
            {
                arrivedAtLocation = true;
                enemy.StopMoving(); 
                ani.SetMoving(false);
                

                if (moveTimeout > 3.0f)
                {
                    Debug.Log("[SearchState] 목표 지점에 도달하지 못했습니다. 수색을 종료합니다.");
                }
                else Debug.Log("[SearchState] 도착! 주변을 두리번거립니다...");
            }
            else
            {
                enemy.MoveToTarget(searchTargetPos);
            }
        }
        else
        {
            searchTimer += Time.deltaTime;
            if (searchTimer > 1.5f && searchTimer < 1.6f) //1.5초쯤 한번 뒤돌아보기(만약 애니메이션 추가되면 없앨 예정)
            {
                enemy.FaceDirection(-enemy.transform.localScale.x > 0 ? 1 : -1);
            }
            if (searchTimer >= searchDuration)
            {
                Debug.Log($"[SearchState] 수색 종료. 플레이어 없음. -> IdleState");
                enemy.ChangeState(enemy.idleState);
                return;
            }
        }

        if (enemy.player != null)
        {
            float distToPlayer = Vector2.Distance(enemy.transform.position, enemy.player);
            if (distToPlayer < enemy.speciesData.detectionRadius)
            {
                Debug.Log("[SearchState] 플레이어 발견! 다시 추격!");
                enemy.ChangeState(enemy.chaseState);
            }
        }
    }

    public void Exit()
    {
        ani.SetMoving(false);
        //enemy.SetPassThroughPlayer(false); 위의 경우와 동일한 이유로 주석처리
        enemy.SetGhostMode(false);
    }
}