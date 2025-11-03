using System;
using System.Collections.Generic;
using UnityEngine;

public class ApplyAttackStatBonus
{
    
    public static void ApplyAttakStatBonus()
    {
        if (PlayerExpManager.PlayerData.appliedBuffs.TryGetValue("attack0", out var done) && done) return;
        Debug.Log("[ApplyAttakStatBonus] 함수 호출됨");
        if (SoulBuffManager.IsButtonUnlocked("attack0"))
        {
            Debug.Log("attack0번 눌려서 실행됨");
            Ark_stat.strength += 1;
            Debug.Log($"Strength 스탯이 증가하였습니다 Strength{Ark_stat.strength}");
            SaveManager.Instance.SaveNow();
        }
    }

    public static void MoveSpeedUp()
    {
        Debug.Log("[MoveSpeedUp] 함수 호출됨");
        if (SoulBuffManager.IsButtonUnlocked("attack1"))
        {
            Debug.Log("attack1번 눌려서 실행됨");
            
            Player_move.speedUP += 0.1f;
        }
    }

    public static void CritChanceUp()
    {
        Debug.Log("[CritChanceUp] 함수 호출됨");
        if (SoulBuffManager.IsButtonUnlocked("attack2"))
        {
            Debug.Log("attack2번 눌려서 실행됨");

            Attack_Damage.SoulBuffCritChance = 20;
        }
        

    }

    public static void CritDamageUP()
    {
        Debug.Log("[CritDamageUp] 함수 호출됨");
        if (SoulBuffManager.IsButtonUnlocked("attack4"))
        {
            Debug.Log("attack4번 눌려서 실행됨");

            Attack_Damage.SoulBuffCriticalDamage = 0.2f;
        }
    }
}
