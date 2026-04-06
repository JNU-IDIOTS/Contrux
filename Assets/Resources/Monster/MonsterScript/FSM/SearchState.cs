using UnityEngine;

public class SearchState : IEnemyState
{
    private EnemyAI enemy;
    private MonsterAnimatorController ani;

    private float searchTimer = 0f;
    private float searchDuration = 3.0f;
    private bool arrivedAtLocation = false;
    private float moveTimeout = 0.0f;

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

        Vector2 randomOffset = Random.insideUnitCircle * 2.0f;
        searchTargetPos = enemy.lastKnownPos + new Vector3(randomOffset.x, randomOffset.y, 0);

        enemy.SetGhostMode(true);
        ani.SetMoving(true);
    }

    public void Update()
    {
        if (!arrivedAtLocation)
        {
            moveTimeout += Time.deltaTime;

            float distToTarget = Vector2.Distance(enemy.transform.position, searchTargetPos);

            if (distToTarget < 0.5f || moveTimeout > 3.0f)
            {
                arrivedAtLocation = true;
                enemy._enemyMovement.StopMoving(); // 다리 호출
                ani.SetMoving(false);

                if (moveTimeout > 3.0f)
                {
                    Debug.Log("[SearchState] 목표 지점에 도달하지 못했습니다. 수색을 종료합니다.");
                }
                else Debug.Log("[SearchState] 도착! 주변을 두리번거립니다...");
            }
            else
            {
                enemy._enemyMovement.MoveToTarget(searchTargetPos); // 다리 호출
            }
        }
        else
        {
            searchTimer += Time.deltaTime;
            if (searchTimer > 1.5f && searchTimer < 1.6f)
            {
                enemy._enemyMovement.FaceDirection(-enemy.transform.localScale.x > 0 ? 1 : -1); // 다리 호출
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
        enemy.SetGhostMode(false);
    }
}