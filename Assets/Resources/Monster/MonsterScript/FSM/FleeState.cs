// 용기가 일정 이하로 떨어졌을 때 도망치는 행동을 담당하는 스크립트
using UnityEngine;

public class FleeState : IEnemyState
{
    private EnemyAI enemy;
    private MonsterAnimatorController ani;

    public FleeState(EnemyAI enemy, MonsterAnimatorController ani)
    {
        this.enemy = enemy;
        this.ani = ani;
    }

    public void Enter()
    {
        Debug.Log(enemy.name + "가 도망칩니다!");
        
        // 도주 속도 = 추격 속도 * 1.2배 (데이터 기반)
        enemy.moveSpeed = enemy.speciesData.chaseSpeed * 1.2f; 
        ani.SetMoving(true);
    }

    public void Update()
    {
        // 1. 플레이어 위치 확인
        if (enemy.player == null) return;
        Vector3 playerPos = enemy.player;

        // 2. 플레이어 반대 방향으로 이동
        Vector3 fleeDir = (enemy.transform.position - playerPos).normalized;
        enemy.rb.linearVelocity = new Vector2(fleeDir.x * enemy.moveSpeed, enemy.rb.linearVelocity.y);
        
        // 3. 방향 전환
        if (Mathf.Abs(fleeDir.x) > 0.01f)
            enemy.FaceDirection(fleeDir.x > 0 ? 1 : -1);

        // 4. 상태 복귀 조건 확인
        float distance = Vector3.Distance(enemy.transform.position, playerPos);
        
        // (EnemyAI.Update에서 용기가 회복됨)
        // 용기가 절반 이상 회복되고, 플레이어와 충분히 멀어지면 '순찰'로
        if (enemy.currentCourage >= (enemy.speciesData.maxCourage * 0.5f) && 
            distance > enemy.speciesData.detectionRadius * 1.5f)
        {
            enemy.ChangeState(enemy.idleState);
        }
    }

    public void Exit()
    {
        ani.SetMoving(false);
    }
}