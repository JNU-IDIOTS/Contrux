using UnityEngine;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class SoulSaveData
{
    public int timeSand;
    public SoulStats stats;
}

[System.Serializable]
public class SoulStats 
{
    // 🚀 [기획서 기준 변수명 통일]
    // 공격
    [Header("[공격 관련]")] // 🚀 구역 나누기
    public int physicalattack;      // 물리 공격력
    public int magicattack;         // 마법 공격력
    public int criticalchance;      // 크리티컬 확률 (+데미지)
    public int itemcooldownspeed;   // 아이템 쿨타임
    public int skillcooldownspeed;  // 스킬 쿨타임

    // 방어
    [Header("[방어 관련]")] // 🚀 구역 나누기
    public int health;              // 체력
    public int defensivepower;      // 방어력
    public int def_dmg_reduce;      // 받는 피해 감소
    public int def_revive;          // 부활

    // 특수
    [Header("[특수 능력]")] // 🚀 구역 나누기
    public int sp_cost_reduce;      // 스팀 비용
    public int sp_cd_reduce;        // 스팀 쿨타임
    public int sp_dash_stack;       // 대쉬 스택
    public int sp_overheat;         // 오버히트
    
    // (혹시 몰라 예전 변수나 안 쓰는 변수도 에러 방지용으로 남겨둠 - 필요 없으면 삭제 가능)
    public int attackspeed;
    public int movementspeed;
}

public class SoulDataManager : MonoBehaviour
{
    public static SoulDataManager Instance;
    public SoulSaveData saveData;
    private string filePath;

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }

        filePath = Path.Combine(Application.persistentDataPath, "Ark_stat_v2.json");
        LoadGameData();
    }

    public void SaveGameData()
    {
        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(filePath, json);
#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif
    }

    public void LoadGameData()
    {
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            saveData = JsonUtility.FromJson<SoulSaveData>(json);
            if (saveData.stats == null) saveData.stats = new SoulStats();
        }
        else
        {
            saveData = new SoulSaveData { timeSand = 10000, stats = new SoulStats() };
            SaveGameData();
        }
    }

    public bool TryUpgradeStat(string statKey, int cost, int maxLevel)
    {
        var field = typeof(SoulStats).GetField(statKey);
        if (field == null) 
        {
            Debug.LogError($"❌ [SoulDataManager] '{statKey}' 변수를 찾을 수 없습니다!");
            return false;
        }

        int currentLv = (int)field.GetValue(saveData.stats);
        if (currentLv >= maxLevel) return false;

        if (saveData.timeSand >= cost)
        {
            saveData.timeSand -= cost;
            field.SetValue(saveData.stats, currentLv + 1);
            SaveGameData();
            
            if (StatDataManager.Instance != null)
                StatDataManager.Instance.CalculateFinalStats();
            
            return true;
        }
        return false;
    }

    public int GetStatLevel(string statKey)
    {
        var field = typeof(SoulStats).GetField(statKey);
        return (field != null) ? (int)field.GetValue(saveData.stats) : 0;
    }
}