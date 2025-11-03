using System;
using System.Collections.Generic;
using UnityEngine;

public class SoulBuffManager
{
    public static bool P_dead = false;
    public static bool IsButtonUnlocked(string buttonId)
    {
        if (PlayerExpManager.PlayerData == null)
        {
            Debug.LogWarning("[BuffManager] PlayerData가 null임");
            return false;
        }

        if (PlayerExpManager.PlayerData.unlockedButtons == null)
        {
            Debug.LogWarning("[BuffManager] unlockedButtons가 null임");
            return false;
        }

        return PlayerExpManager.PlayerData.unlockedButtons.Contains(buttonId);
    }

    private static readonly HashSet<string> statBuffButtons = new()//스탯관련 역할을 하는 버튼들 적기
    {
        "attack0",
        "deffence0",
        "deffence4"
    };

    private static bool IsStatBuff(string buttonId)//스탯 관련 역할을 하는 버튼들 형식 변경
    {
        return statBuffButtons.Contains(buttonId);
    }

    private static Dictionary<string, Action> buffActions = new Dictionary<string, Action>()
    {
        { "attack0", ApplyAttackStatBonus.ApplyAttakStatBonus},
        { "attack1", ApplyAttackStatBonus.MoveSpeedUp },
        { "attack2", ApplyAttackStatBonus.CritChanceUp },
        { "attack3", SoulBuffDamageUp.MaxHpDamageUpReady },
        { "attack4", ApplyAttackStatBonus.CritDamageUP },
        { "attack5", SoulBuffAttack.ButtonUnlockScan },
        { "attack6", AttackSpeed.Attack6Button },
        { "deffence0", DamageDiscount.enemyAttackDis },
        { "deffence1", ApplyDeffenceStatBonus.applyDeffenceStatBonus},
        { "deffence2", HpGenerate.HealCoroutine },
        { "deffence3", NonDamage.PreNonDamage },
        { "deffence4", DamageDiscount.enemyAttackDis},
        { "deffence5", SoulBuffShield.ShieldReadyOn },
        { "deffence6", SoulBuffInvincibility.DButton6Clicked }

    };

    public static void ApplyBuffByButtonId(string buttonId)
    {
        if (P_dead == true)
        {
            // 플레이어가 죽은 경우, 해금된 전체 버튼의 버프를 모두 재적용
            foreach (var id in PlayerExpManager.PlayerData.unlockedButtons)
            {
                if (buffActions.TryGetValue(id, out var act))
                {
                    act.Invoke();  // 버프 실행
                    PlayerExpManager.PlayerData.appliedBuffs[id] = true; // 재적용 표시
                    //Debug.Log($"[BuffManager] 사망 후 재적용: {id}");
                }
            }

            SaveManager.Instance.SaveNow();   // 저장
            PlayerExpSave.SavedExp();         // 경험치 관련 저장

            P_dead = false;  // 재적용 완료 후 초기화
            return;          // 중복 처리 방지로 바로 리턴
        }
        // 중복 적용 방지용 체크
        if (IsStatBuff(buttonId) &&
            PlayerExpManager.PlayerData.appliedBuffs.TryGetValue(buttonId, out bool alreadyApplied) && alreadyApplied)
        {
            Debug.Log($"[버프] {buttonId} 효과는 이미 적용됨 (중복 실행 방지)");
            return;
        }

        if (buffActions.TryGetValue(buttonId, out var action))
        {
            if (IsStatBuff(buttonId))
            {
                PlayerExpManager.PlayerData.appliedBuffs[buttonId] = true;
                Debug.Log($"[BuffManager] {buttonId} 효과 적용 및 저장 완료");
                SaveManager.Instance.SaveNow();  // 저장
                PlayerExpSave.SavedExp();        // 저장
            }
            action.Invoke();
        }
        else
        {
            Debug.LogWarning($"버튼 ID '{buttonId}'에 연결된 효과가 없습니다.");
        }
        SaveManager.Instance.SaveNow();
        PlayerExpSave.SavedExp();
    }

    public static void ApplyAllUnlockedBuffs()
    {
        foreach (var buttonId in PlayerExpManager.PlayerData.unlockedButtons)
        {
            ApplyBuffByButtonId(buttonId);
            Debug.Log($"[BuffManager] 해금된 '{buttonId}' 효과 재적용 완료");
            var ui = UnityEngine.Object.FindFirstObjectByType<HealthBarUIController>();
            ui?.RefreshFromPlayerHealth();   // or SetHP(...)
        }
    }
}
