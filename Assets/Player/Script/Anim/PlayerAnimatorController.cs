using UnityEngine;

public class PlayerAnimatorController : MonoBehaviour
{
    private Animator _ani; // 🚨 컴포넌트 참조

    private void Awake()
    {
        _ani = GetComponent<Animator>();
    }

    private void Update()
    {
        // Debug.Log($"Current Attack Speed Multiplier: {Ark_stat.attackSpeedMultiplier}");
    }
    
    public void NomalAttack(int count) // 🚨 메서드명 규칙 유지
    {
        _ani.SetTrigger("Attack" + count);
    }
    
    public void OverHitAttack(int count)
    {
        _ani.SetTrigger("OverHit" + count);
    }
    
    public void SideCommand()
    {
        _ani.SetTrigger("SideCommand");
    }
    
    public void DownCommand()
    {
        _ani.SetTrigger("DownCommand");
    }
    
    public void Guard()
    {
        _ani.SetTrigger("Guard");
        _ani.SetBool("IsGuarding", true);
    }
    
    public void Guarding()
    {
        _ani.SetTrigger("Guarding");
    }
    
    public void NotGuard()
    {
        _ani.SetBool("IsGuarding", false);
    }
    
    public void GuardBreak()
    {
        _ani.SetTrigger("GuardBreak");
    }
    
    public void Jump()
    {
        _ani.SetTrigger("Jump");
    }
    
    public void DoubleJump()
    {
        _ani.ResetTrigger("doubleJump");
    }

    public void Dash()
    {
        _ani.SetTrigger("Dash");
    }

    public void IsGround(bool nowGrounded)
    {
        _ani.SetBool("IsGrounded", nowGrounded);
    }
    
    public bool GetIsGround() // 🚨 메서드명 규칙 유지
    {
        return _ani.GetBool("IsGrounded");
    }
    
    public void RunAttack()
    {
        _ani.ResetTrigger("RunAttack");
    }
    
    public void AirSpeedY(float y)
    {
        _ani.SetFloat("AirSpeedY", y);
    }
    
    public void IsWalking(bool moveInput)
    {
        _ani.SetBool("IsWalking", moveInput);
    }
    
    public void Die()
    {
        _ani.SetTrigger("Die");
    }
    
    public void ApplyAttackSpeed()
    {
        // _ani.speed = Ark_stat.GetAttackSpeed();
        // _ani.speed = Ark_stat.attackSpeedMultiplier;
    }
}