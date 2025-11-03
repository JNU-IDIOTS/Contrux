using UnityEngine;

public class PlayerAnimatorController : MonoBehaviour
{
    private Animator ani;

    private void Awake()
    {
        ani = GetComponent<Animator>();
    }

    private void Update()
    {
        Debug.Log($"현재 공속은 {Ark_stat.attackSpeedMultiplier}");
    }
    public void NomalAttack(int count)
    {
        ani.SetTrigger("Attack" + count);
    }
    public void OverHitAttack(int count)
    {
        ani.SetTrigger("OverHit" + count);
    }
    public void SideCommand()
    {
        ani.SetTrigger("SideCommand");
    }
    public void DownCommand()
    {
        ani.SetTrigger("DownCommand");
    }
    public void Guard()
    {
        ani.SetTrigger("Guard");
        ani.SetBool("IsGuarding", true);
    }
    public void Guarding()
    {
        ani.SetTrigger("Guarding");
    }
    public void NotGuard()
    {
        ani.SetBool("IsGuarding", false);
    }
    public void GuardBreak()
    {
        ani.SetTrigger("GuardBreak");
    }
    public void Jump()
    {
        ani.SetTrigger("Jump");
    }
    public void DoubleJump()
    {
        ani.ResetTrigger("doubleJump");
    }

    public void Dash()
    {
        ani.SetTrigger("Dash");
    }

    public void IsGround(bool nowGrounded)
    {
        ani.SetBool("IsGrounded", nowGrounded);
    }
    public bool GetIsGround()
    {
        return ani.GetBool("IsGrounded");
    }
    public void RunAttack()
    {
        ani.ResetTrigger("RunAttack");
    }
    public void AirSpeedY(float y)
    {
        ani.SetFloat("AirSpeedY",y);
    }
    public void IsWalking(bool moveInput)
    {
        ani.SetBool("IsWalking", moveInput);
    }
    public void Die()
    {
        ani.SetTrigger("Die");
    }
    public void ApplyAttackSpeed()
    {
        //ani.speed = Ark_stat.GetAttackSpeed();
        ani.speed = Ark_stat.attackSpeedMultiplier;
    }
}

