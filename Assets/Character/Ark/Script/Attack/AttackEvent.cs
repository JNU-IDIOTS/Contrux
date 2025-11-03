// PlayerAnimationEvents.cs (애니메이터 오브젝트에 붙이기)
using UnityEngine;

public class AttackEvent : MonoBehaviour
{
    private Attack_Damage attack;
    private Magic_Attack magicattack;

    void Start()
    {
        attack = GetComponentInParent<Attack_Damage>();
        magicattack = GetComponentInParent<Magic_Attack>();
    }

    public void NomalAttackEvent()
    {
        attack?.NormalAttack(); // 실제 로직 실행
    }
    public void DownCommandEvent()
    {
        attack?.DownCommand();
    }
    public void SideCommnadEvent()
    {
        attack?.SideCommand();
    }
    public void SideCommand_SwordAura()
    {
        magicattack.UseSlashSkill();
    }

}
