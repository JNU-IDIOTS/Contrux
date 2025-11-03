using UnityEngine;

public class AttackState : IEnemyState
{
    private float StateCoolDown = 1f;
    private int decision = -1;
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
        enemy.PrepareForAttack();
        enemy.GetRigidbody().linearVelocity = new Vector2(0, enemy.GetRigidbody().linearVelocity.y);
        Debug.Log("공격준비중");

    }

    public void Update()
    {
        if (enemy.canAttack)
        {
            if (enemy.CompareTag("DwarfBuster")) BusterAttack();
            else if(enemy.CompareTag("Wolf")) WolfAttack();
            else ExecuteAttack();
        }

        if (!enemy.IsPlayerAttackable()) enemy.ChangeState(enemy.chaseState);
    }

    private void ExecuteAttack()
    {
        decision = Random.Range(1, 11);
        Debug.Log($"결정값{decision}");
        switch (decision)
        {
            case 1:
            case 2:
            case 3:
            case 5:
            case 8:
            case 9:
                enemy.FacetoPlayer();
                enemy.canAttack = false;
                enemy.GetRigidbody().linearVelocity = Vector2.zero;
                ani.SetMoving(false);
                ani.NAttack();
                enemy.cooldown = 2.2f;
                break;
            case 4:
            case 7:
            case 10:
                if (enemy.CompareTag("Wolf"))
                {
                    enemy.FacetoPlayer();
                    enemy.canAttack = false;
                    enemy.GetRigidbody().linearVelocity = Vector2.zero;
                    enemy.Jumpoo();
                    ani.SAttack();
                    enemy.cooldown = 3.5f;
                }
                else
                {
                    enemy.FacetoPlayer();
                    enemy.canAttack = false;
                    enemy.GetRigidbody().linearVelocity = Vector2.zero;
                    ani.SAttack();
                    enemy.cooldown = 3.5f;
                }
                break;
            case 6:
                enemy.FacetoPlayer();
                enemy.Backstep();
                enemy.cooldown = 3f;
                break;
        }

    }
    private void BusterAttack()
    {
        decision = Random.Range(1, 11);
        Debug.Log($"결정값{decision}");
        switch (decision)
        {
            case 1:
            case 2:
            case 3:
            case 4:
            case 5:
            case 6:
            case 8:
            case 9:
                enemy.canAttack = false;
                enemy.GetRigidbody().linearVelocity = Vector2.zero;
                ani.SetMoving(false);
                ani.NAttack();
                enemy.cooldown = 2f;
                break;

            case 7:
            case 10:
                enemy.canAttack = false;
                ani.Chaging();
                enemy.StartBusterCharge();
                break;
        }

    }
    private void WolfAttack()
    {
        decision = Random.Range(1, 11);
        Debug.Log($"결정값{decision}");
        switch (decision)
        {
            case 1:
            case 2:
            case 3:
            case 4:
            case 5:
            case 6:
            case 7:
            case 8:
            case 9:
            case 10:
                enemy.FacetoPlayer();
                enemy.canAttack = false;
                enemy.GetRigidbody().linearVelocity = Vector2.zero;
                enemy.Jumpoo();
                ani.NAttack();
                enemy.cooldown = 3.5f;
                break;
        }
    }

    public void Exit()
    {

    }

}
