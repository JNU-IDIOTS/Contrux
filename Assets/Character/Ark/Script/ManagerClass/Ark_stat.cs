using System.Collections;
using System;
using UnityEngine;

[System.Serializable]
public static class Ark_stat
{
    public static int health;
    public static int strength;
    public static int dexterity;
    public static int intelligence;
    public static int luck;
    public static int remainingStatPoints;

    public static event Action StatsChanged;   // ★ 추가

    // --- 공격 속도 모델 ---
    // 장비/레벨 등으로 결정되는 "기본 공속"
    public static float BaseAttackSpeed = 1.3f;

    // 스택/패시브 등에서 설정하는 "내부 배수" (AttackSpeed.cs가 이 값만 건드림: 1.10~1.60 등)
    public static float attackSpeedMultiplier = 1.0f;

    // (선택) 포션, 오라 같은 외부 일시버프 배수
    public static float externalBuffMultiplier = 1.0f;

    // 최종 공속은 항상 여기서만 계산해서 넘김
    public static float GetAttackSpeed()
    {
        return BaseAttackSpeed * attackSpeedMultiplier * externalBuffMultiplier;
    }

    // 내부 배수만 설정(스택 로직이 호출)
    public static void SetAttackSpeedMultiplier(float multiplier)
    {
        attackSpeedMultiplier = Mathf.Max(0.0001f, multiplier);
    }

    // 외부 버프 배수 설정(필요 없으면 안 써도 됨)
    public static void SetExternalBuffMultiplier(float multiplier)
    {
        externalBuffMultiplier = Mathf.Max(0.0001f, multiplier);
    }

    // 공속 관련 리셋: 배수들을 1로, Base는 유지(장비가 바꾸는 값)
    public static void ResetAttackSpeed()
    {
        attackSpeedMultiplier = 1.0f;
        externalBuffMultiplier = 1.0f;
        // BaseAttackSpeed는 장비/레벨 시스템에서 관리
    }

    // ====== 저장/로드 ======
    public static void LoadFrom(statSaveData data)
    {
        health = data.health;
        strength = data.strength;
        dexterity = data.dexterity;
        intelligence = data.intelligence;
        luck = data.luck;                    // ★ 누락되어 있던 부분 추가
        remainingStatPoints = data.remainingStatPoints;

        // BaseAttackSpeed/배수는 보통 장비/버프에 의해 결정됨. 필요 시 data에 포함시켜도 됨.
    }

    public static void ApplyTo(statSaveData data)
    {
        data.health = health;
        data.strength = strength;
        data.dexterity = dexterity;
        data.intelligence = intelligence;
        data.luck = luck;                    // ★ 누락되어 있던 부분 추가
        data.remainingStatPoints = remainingStatPoints;

        // 필요 시 BaseAttackSpeed 등도 저장
    }

    public static void ResetStats()
    {
        health = 0;
        strength = 0;
        dexterity = 0;
        intelligence = 0;
        luck = 0;
        remainingStatPoints = 20;
        Debug.Log("플레이어 스탯이 초기화되었습니다.");
    }

    public static bool MeetsRequirement(WeaponPrefabData data)
    {
        return health >= data.requiredHp &&
               strength >= data.requiredStr &&
               dexterity >= data.requiredDex &&
               intelligence >= data.requiredInt &&
               luck >= data.requiredluk;
    }

    public enum StatType { Health, Strength, Dexterity, Intelligence, Luck }

    public static bool IncreaseStat(StatType type)
    {
        if (remainingStatPoints <= 0) return false;

        switch (type)
        {
            case StatType.Health: health++; break;
            case StatType.Strength: strength++; break;
            case StatType.Dexterity: dexterity++; break;
            case StatType.Intelligence: intelligence++; break;
            case StatType.Luck: luck++; break;
        }

        remainingStatPoints--;
        SaveManager.Instance.SaveNow();
        StatsChanged?.Invoke();  // ★ 스탯 바뀌면 알림 발사
        return true;
    }
}
