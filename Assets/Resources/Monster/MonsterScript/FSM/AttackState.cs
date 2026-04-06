using UnityEngine;
using System.Collections;

public class AttackState : IEnemyState
{
    //private float StateCoolDown = 1f;
    //private int decision = -1;
    private EnemyAI enemy;
    private MonsterAnimatorController ani;

    public AttackState(EnemyAI enemy, MonsterAnimatorController ani)
    {
        this.enemy = enemy;
        this.ani = ani;
        ani.SetAttackState(this);
    }

    public void Enter()
    {
        enemy._enemyMovement.PrepareForAttack();
        enemy.GetRigidbody().linearVelocity = new Vector2(0, enemy.GetRigidbody().linearVelocity.y);
        Debug.Log("공격준비완료");
        if (ani != null)
        {
            ani.SetMoving(false);
        }
    }

    public void Update()
    {
        if (enemy.canAttack)
        {
            // 무전기(Squad) 호출로 변경
            if (!enemy._enemySquad.justAlerted)
            {
                enemy._enemySquad.AlertNearbyAllies(false);
            }
            enemy.StartCoroutine(AttackRoutine());

            switch (enemy.speciesData.attackType)
            {
                case AttackPatternType.Buster:
                    BusterAttack();
                    break;
                case AttackPatternType.Wolf:
                    WolfAttack();
                    break;
                case AttackPatternType.Standard:
                default:
                    ExecuteAttack();
                    break;
            }
        }

        if (!enemy.IsPlayerAttackable()) enemy.ChangeState(enemy.chaseState);
    }

    private void ExecuteAttack()
    {
        enemy.canAttack = false;
        enemy.GetRigidbody().linearVelocity = Vector2.zero;
        ani.SetMoving(false);
        enemy._enemyMovement.FacetoPlayer();

        float roll = Random.Range(0f, 10f);

        if (roll < enemy.speciesData.aggression)
        {
            if (Random.value > 0.5f)
            {
                ani.SAttack();
                enemy.StartCooldown(enemy.speciesData.attackCooldown * 1.5f);
            }
            else
            {
                enemy._enemyMovement.Backstep();
                enemy.StartCooldown(enemy.speciesData.attackCooldown * 1.2f);
            }
        }
        else
        {
            ani.NAttack();
            enemy.StartCooldown(enemy.speciesData.attackCooldown);
        }
    }

    private void BusterAttack()
    {
        enemy.canAttack = false;
        float roll = Random.Range(0f, 10f);

        if (roll < enemy.speciesData.aggression)
        {
            ani.Chaging();
            enemy._enemyCombat.StartBusterCharge();
        }
        else
        {
            enemy.GetRigidbody().linearVelocity = Vector2.zero;
            ani.SetMoving(false);
            ani.NAttack();
            enemy.StartCooldown(enemy.speciesData.attackCooldown);
        }
    }

    private void WolfAttack()
    {
        enemy.canAttack = false;
        enemy.GetRigidbody().linearVelocity = Vector2.zero;
        enemy._enemyMovement.FacetoPlayer();
        enemy._enemyMovement.Jump();

        ani.NAttack();
        enemy.StartCooldown(enemy.speciesData.attackCooldown * 1.2f);
    }

    IEnumerator AttackRoutine()
    {
        float delay = 0.5f;
        yield return new WaitForSeconds(delay);

        switch (enemy.speciesData.attackType)
        {
            case AttackPatternType.Buster: BusterAttack(); break;
            case AttackPatternType.Wolf: WolfAttack(); break;
            default: ExecuteAttack(); break;
        }
    }

    public void Exit()
    {
    }
}