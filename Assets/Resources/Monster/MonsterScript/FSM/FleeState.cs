using UnityEngine;

public class FleeState : IEnemyState
{
    private EnemyAI enemy;
    private MonsterAnimatorController ani;
    private float localFleeSpeed;

    public FleeState(EnemyAI enemy, MonsterAnimatorController ani)
    {
        this.enemy = enemy;
        this.ani = ani;
    }

    public void Enter()
    {
        Debug.Log(enemy.name + "가 도망칩니다!");

        localFleeSpeed = enemy.speciesData.chaseSpeed * 1.2f;
        ani.SetMoving(true);
    }

    public void Update()
    {
        if (enemy.player == null) return;
        Vector3 playerPos = enemy.player;

        Vector3 fleeDir = (enemy.transform.position - playerPos).normalized;
        enemy.GetRigidbody().linearVelocity = new Vector2(fleeDir.x * localFleeSpeed, enemy.GetRigidbody().linearVelocity.y);

        if (Mathf.Abs(fleeDir.x) > 0.01f)
            enemy._enemyMovement.FaceDirection(fleeDir.x > 0 ? 1 : -1);

        float distance = Vector3.Distance(enemy.transform.position, playerPos);

        // 심장(Health) 호출로 변경
        if (enemy._enemyHealth.currentCourage >= (enemy.speciesData.maxCourage * 0.5f) &&
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