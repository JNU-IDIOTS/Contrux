using UnityEngine;

public class ChaseState : IEnemyState
{
    private EnemyAI enemy;
    private MonsterAnimatorController ani;
    private float cooldown = 1f;    //쿨다운 1초 설정
    private float timer; // 타이머 설정

    public ChaseState(EnemyAI enemy, MonsterAnimatorController ani) // 추적 모드
    {
        this.enemy = enemy;
        this.ani = ani;
    }

    public void Enter()
    {
        timer = 0f;
        if (enemy.CompareTag("Wolf"))
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
        }
    }
    public void Update()
    {
        timer += Time.deltaTime;

        if (timer >= cooldown)
        {
            // 우선순위 1: 플레이어가 공격 가능하면 공격 상태로 전환
            if (enemy.playerAttackable && enemy.IsPlayerInRange())
            {
                enemy.ChangeState(enemy.attackState);
                Debug.Log("공격 모드 전환");
                ani.SetMoving(false);
            }
            // 우선순위 2: 플레이어가 시야 안에만 있으면 추적 상태 유지
            else if (enemy.IsPlayerInRange())
            {
                enemy.moveSpeed = 1f;
                ani.SetMoving(true);
                enemy.MoveTowardsPlayer();
                enemy.GetAnimator().speed = 1.5f;
                Debug.Log("추적 중 (공격 불가)");
            }
            // 우선순위 3: 시야 밖이면 대기 상태 전환
            else
            {
                if (enemy.currentState != enemy.idleState)
                {
                    enemy.SetDetectionRadius(8f);
                    enemy.ChangeState(enemy.idleState);
                    //Debug.Log("대기 모드 전환");

                }
            }
            timer = 0f;
        }
    }



    public void Exit()
    {
        enemy.GetAnimator().speed = 1f;
    }
}
