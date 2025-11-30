// 이 스크립트는 PlayerAnimatorController에 대한 API 호출을 동기화하여 관리합니다.
using Unity.VisualScripting;
using UnityEngine;

public class PlayerAnimationSync : MonoBehaviour
{
    private PlayerAnimatorController _bodyController; // 🚨 컴포넌트 참조
    private PlayerAnimatorController _effectController; // 🚨 컴포넌트 참조
    private PlayerAnimatorController _weaponController; // 🚨 컴포넌트 참조
    private Animator _effectAnimator; // 🚨 컴포넌트 참조

    private void Awake()
    {
        // 하위 오브젝트에서 PlayerAnimatorController 컴포넌트들을 찾습니다.
        _bodyController = transform.Find("Body").GetComponent<PlayerAnimatorController>();
        _effectController = transform.Find("Effect").GetComponent<PlayerAnimatorController>();
        _weaponController = transform.Find("Weapon").GetComponent<PlayerAnimatorController>();

        _effectAnimator = _effectController.GetComponent<Animator>();
        if (_effectAnimator == null)
            Debug.LogError("[PlayerAnimationSync] Effect Animator를 찾을 수 없습니다!"); // 🚨 주석 정리

        // (대안 코드 주석 정리)
        // var ctrls = GetComponentsInChildren<PlayerAnimatorController>();
        // _bodyController  = ctrls.First(c => c.gameObject.name == "Body");
        // _armorController = ctrls.First(c => c.gameObject.name == "Armor");
        // _weaponController= ctrls.First(c => c.gameObject.name == "Weapon");
    }
    
    public AnimatorStateInfo GetCurrentStateInfo // 🚨 메서드명 규칙 적용
        => _effectAnimator.GetCurrentAnimatorStateInfo(0);

    // 공격: 일반 공격
    public void NomalAttack(int count)
    {
        _bodyController.NomalAttack(count);
        _effectController.NomalAttack(count);
        _weaponController.NomalAttack(count);
    }
    
    public void OverHitAttack(int count)
    {
        _bodyController.OverHitAttack(count);
        _effectController.OverHitAttack(count);
        _weaponController.OverHitAttack(count);
    }

    public void SideCommand()
    {
        _bodyController.SideCommand();
        _effectController.SideCommand();
        _weaponController.SideCommand();
    }

    public void DownCommand()
    {
        _bodyController.DownCommand();
        _effectController.DownCommand();
        _weaponController.DownCommand();
    }

    public void Guard()
    {
        _bodyController.Guard();
        _effectController.Guard();
        _weaponController.Guard();
    }

    public void Guarding()
    {
        _bodyController.Guarding();
        _effectController.Guarding();
        _weaponController.Guarding();
    }

    public void NotGuard()
    {
        _bodyController.NotGuard();
        _effectController.NotGuard();
        _weaponController.NotGuard();
    }

    public void GuardBreak()
    {
        _bodyController.GuardBreak();
        _effectController.GuardBreak();
        _weaponController.GuardBreak();
    }

    public void Jump()
    {
        _bodyController.Jump();
        _effectController.Jump();
        _weaponController.Jump();
    }

    public void DoubleJump()
    {
        _bodyController.DoubleJump();
        _effectController.DoubleJump();
        _weaponController.DoubleJump();
    }

    public void Dash()
    {
        _bodyController.Dash();
        _effectController.Dash();
        _weaponController.Dash();
    }

    public void IsGround(bool nowGrounded)
    {
        _bodyController.IsGround(nowGrounded);
        _effectController.IsGround(nowGrounded);
        _weaponController.IsGround(nowGrounded);
    }

    public void RunAttack()
    {
        _bodyController.RunAttack();
        _effectController.RunAttack();
        _weaponController.RunAttack();
    }

    public void AirSpeedY(float y)
    {
        _bodyController.AirSpeedY(y);
        _effectController.AirSpeedY(y);
        _weaponController.AirSpeedY(y);
    }

    public void IsWalking(bool moveInput)
    {
        _bodyController.IsWalking(moveInput);
        _effectController.IsWalking(moveInput);
        _weaponController.IsWalking(moveInput);
    }

    public void Die()
    {
        _bodyController.Die();
        _effectController.Die();
        _weaponController.Die();
    }

    public void ApplyAttackSpeed()
    {
        _bodyController.ApplyAttackSpeed();
        _effectController.ApplyAttackSpeed();
        _weaponController.ApplyAttackSpeed();
    }
}