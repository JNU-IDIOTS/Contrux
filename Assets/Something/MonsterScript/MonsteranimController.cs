using UnityEditor;
using UnityEngine;

public class MonsterAnimatorController : MonoBehaviour
{
    private Animator ani;
    private AttackState currentAttackState;

    private void Awake()
    {
        ani = GetComponent<Animator>();
    }
    public void NAttack()
    {
        ani.SetTrigger("NAttack");
    }
    public void SAttack()
    {
        ani.SetTrigger("SAttack");
    }
    public void MGoblinCharging()
    {
        ani.SetTrigger("MagicChaging");
    }
    public void Chaging()
    {
        ani.SetTrigger("Chaging");
    }
    public void SetMoving(bool value)
    {
        ani.SetBool("IsMoving", value);
    }
    public void Hurt()
    {
        ani.SetTrigger("Hurt");
    }
    public void Die()
    {
        ani.SetTrigger("IsDeath");
    }
    public float GetAnimationLength()
    {
        return ani.GetCurrentAnimatorStateInfo(0).length;
    }
    public void SetAttackState(AttackState attackState)
    {
        currentAttackState = attackState;
    }
}

