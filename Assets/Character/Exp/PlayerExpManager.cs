using UnityEngine;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System;

public class PlayerExpManager
{
    // 외부 시스템 참조(다른 스크립트에서 사용)
    public static PlayerHealth PlayerHealth;
    public static PlayerData PlayerData;

    // 초기 파일 생성시에만 참조하는 기본값(런타임 로직에 의존 X)
    public static int currentLevel = 1;
    public static int currentExp = 0;
    public static int totalExp = 0;
    public static int expToNextLevel = 100;

    public static int MaxLevel = 50;

    // UI 갱신 이벤트
    public static event Action OnExpChanged;                             // 단순 갱신 트리거
    public static event Action<int, int, int> OnExpChangedWithValue;     // (current, max, level)

    // 필요 없다면 제거 가능
    public UIController uiController;

    /// <summary>
    /// Player_Data.json 존재 유무에 따라 신규 생성 또는 로드.
    /// 저장 값이 0/음수로 잘못 들어온 경우도 안전하게 보정.
    /// 부팅 시점(첫 씬 로드 전)에 반드시 한 번 호출되도록 구성 권장.
    /// </summary>
    public static void InitPlayerData()
    {
        string path = Path.Combine(Application.persistentDataPath, "Player_Data.json");

        if (!File.Exists(path))
        {
            var data = new PlayerData
            {
                playerLevel = Mathf.Max(1, currentLevel),
                playerExp = Mathf.Max(0, currentExp),
                soulExp = 0,
                playerTotalExp = Mathf.Max(0, totalExp),
                expToNextLevel = Mathf.Max(1, expToNextLevel), // 0 방지
                unlockedButtons = new List<string>()
            };

            PlayerData = data;
            File.WriteAllText(path, JsonUtility.ToJson(data, true), Encoding.UTF8);
            Debug.Log("초기 Player_Data.json 파일 생성 완료");
        }
        else
        {
            LoadPlayerData(); // 이미 파일 있으면 로드
        }

        // UI 초기값 반영
        OnExpChanged?.Invoke();
        OnExpChangedWithValue?.Invoke(PlayerData.playerExp, PlayerData.expToNextLevel, PlayerData.playerLevel);
    }

    /// <summary>
    /// 경험치 획득
    /// </summary>
    public static void AddExp(int amount)
    {
        if (PlayerData == null) InitPlayerData();

        PlayerData.playerExp += amount;
        PlayerData.playerTotalExp += amount;

        Debug.Log($"경험치 +{amount}, 현재 경험치: {PlayerData.playerExp}/{PlayerData.expToNextLevel}, 총 경험치:{PlayerData.playerTotalExp}");

        Level_Up();

        OnExpChanged?.Invoke();
    }

    /// <summary>
    /// 레벨업 처리(여러 레벨 동시 상승 안전)
    /// expToNextLevel이 0/음수가 되면 무한루프가 나므로 항상 1 이상으로 보정.
    /// </summary>
    public static void Level_Up()
    {
        if (PlayerData == null) InitPlayerData();

        if (PlayerData.playerLevel >= MaxLevel) return;

        // 잘못 저장된 값 방어
        if (PlayerData.expToNextLevel <= 0)
            PlayerData.expToNextLevel = 100;

        int safety = 0; // 무한루프 방지
        while (PlayerData.playerExp >= PlayerData.expToNextLevel)
        {
            PlayerData.playerExp -= PlayerData.expToNextLevel;
            PlayerData.playerLevel++;
            PlayerData.expToNextLevel = Mathf.Max(1, Mathf.RoundToInt(PlayerData.expToNextLevel * 1.2f));

            Ark_stat.remainingStatPoints += 4;

            Debug.Log($"레벨업! 현재 레벨 : {PlayerData.playerLevel}");

            // 상한 도달 시 추가 루프 방지
            if (PlayerData.playerLevel >= MaxLevel)
            {
                PlayerData.playerExp = 0;
                PlayerData.expToNextLevel = int.MaxValue;
                break;
            }

            if (++safety > 1000)
            {
                Debug.LogError("Level_Up safety break: exp/requirement 값 점검 필요");
                break;
            }
        }

        OnExpChanged?.Invoke();
        OnExpChangedWithValue?.Invoke(PlayerData.playerExp, PlayerData.expToNextLevel, PlayerData.playerLevel);
    }

    public static void MaxUp()
    {
        if (PlayerData == null) InitPlayerData();

        if (PlayerData.playerLevel == MaxLevel)
        {
            Debug.Log($"당신의 레벨:{PlayerData.playerLevel} 최고 레벨:{MaxLevel} 더 이상 올릴 수 없습니다.");
        }
    }

    /// <summary>
    /// 사망 처리(소울 경험치 환산 등)
    /// </summary>
    public static void PlayerDead()
    {
        if (PlayerData == null) InitPlayerData();

        Debug.Log("PlayerDead 함수호출");

        // 총 경험치의 10% + 나머지 반올림
        PlayerData.soulExp += PlayerData.playerTotalExp / 10;
        float semiresult = PlayerData.playerTotalExp % 10;
        int rounded = Mathf.RoundToInt(semiresult);
        PlayerData.soulExp += rounded;

        // 리셋
        PlayerData.playerLevel = 1;
        PlayerData.playerExp = 0;
        PlayerData.playerTotalExp = 0;
        PlayerData.expToNextLevel = 100;

        PlayerHealth.discountDamage = 0;
        PlayerHealth.maxHP = 100; // 의도된 값인지 확인 필요(기본 500과 불일치 가능)

        Ark_stat.ResetStats();

        Debug.Log($"소울 경험치:{PlayerData.soulExp}");

        SoulBuffManager.P_dead = true;
        SoulBuffManager.ApplyBuffByButtonId("deathTrigger");

        SaveSystemManager.SaveOnDeath();

        OnExpChanged?.Invoke();
        OnExpChangedWithValue?.Invoke(PlayerData.playerExp, PlayerData.expToNextLevel, PlayerData.playerLevel);
    }

    public static void SavePlayerData()
    {
        if (PlayerData == null) return;
        PlayerExpSave.SavedExp();
    }

    public static void LoadPlayerData()
    {
        PlayerExpSave.LoadExp();
        PlayerDataSaveSystem.Load();

        if (PlayerData == null) PlayerData = new PlayerData();

        // 깨진 세이브 방어
        if (PlayerData.playerLevel <= 0) PlayerData.playerLevel = 1;
        if (PlayerData.expToNextLevel <= 0) PlayerData.expToNextLevel = 100;
        if (PlayerData.playerExp < 0) PlayerData.playerExp = 0;
        if (PlayerData.playerTotalExp < 0) PlayerData.playerTotalExp = 0;
        if (PlayerData.unlockedButtons == null) PlayerData.unlockedButtons = new List<string>();
    }
}

/// <summary>
/// 게임 부팅 시 가장 먼저 PlayerExp 데이터 초기화 보장
/// (원치 않으면 이 클래스는 삭제해도 됩니다)
/// </summary>
public static class GameBoot_PlayerExp
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Boot()
    {
        PlayerExpManager.InitPlayerData();
    }
}
