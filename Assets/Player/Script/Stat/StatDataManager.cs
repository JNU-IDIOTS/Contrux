using UnityEngine;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class PlayerCurrentData
{
    public int playerLevel = 1;
    public float currentExp = 0;
    public float currentHP;
}

public class StatDataManager : MonoBehaviour
{
    public static StatDataManager Instance;
    public PlayerCurrentData playerData;

    // 🚀 [추가됨] 플레이어 "기본(깡통)" 스탯 (여기서 설정)
    [Header("=== [1. 플레이어 기본 스탯 (Base)] ===")]
    public float Base_Atk = 10f;
    public float Base_HP = 100f;
    public float Base_Def = 0f;
    public float Base_CritChance = 0f;     // 0%
    public float Base_CritDmg = 1.5f;      // 150%
    public float Base_MoveSpeed = 5.0f;
    public float Base_AtkSpeed = 1.0f;

    [Header("📊 최종 합산 스탯 (확인용)")]
    
    [Header("⚔️ 공격")]
    public float Final_PhyAtk;
    public float Final_MagAtk;
    public float Final_CritChance;
    public float Final_CritDmg;
    public float Final_AtkSpeed;

    [Header("🛡️ 방어/생존")]
    public float Final_HP;
    public float Final_Def;
    public float Final_DmgReduce;
    public bool Final_CanRevive;
    public float Final_ReviveHpPercent;

    [Header("⚡ 유틸리티")]
    public float Final_MoveSpeed;
    public float Final_SkillCool;
    public float Final_ItemCool;
    public float Final_SteamCost;
    public float Final_SteamSpeed;
    public int Final_DashCount;
    public bool Final_Overheat;

    private string filePath;

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }

        filePath = Path.Combine(Application.persistentDataPath, "Player_data.json");
        LoadData();
    }

    private void Start()
    {
        CalculateFinalStats();
    }

    [ContextMenu("스탯 강제 재계산")]
    public void CalculateFinalStats()
    {
        var soul = (SoulDataManager.Instance != null) ? SoulDataManager.Instance.saveData.stats : new SoulStats();
        var item = (ItemDataManager.Instance != null) ? ItemDataManager.Instance.itemData : new ItemStats();

        // 1. 공격력 (물리+마법 묶음 강화)
        float soulAtkBonus = soul.physicalattack * 0.05f; 
        Final_PhyAtk = 10f * (1.0f + soulAtkBonus + item.bonusPhyAtk);
        Final_MagAtk = 10f * (1.0f + soulAtkBonus + item.bonusMagAtk);

        // 2. 체력 & 방어력
        Final_HP = 100f + (soul.health * 5.0f) + item.bonusHp;
        Final_Def = (soul.defensivepower * 5.0f) + item.bonusDef;

        // 3. 크리티컬
        float critLvl = soul.criticalchance;
        Final_CritChance = (critLvl * 0.01f) + item.bonusCritChance;
        Final_CritDmg = 1.5f + (critLvl * 0.01f) + item.bonusCritDmg;

        // 4. 쿨타임 & 피해 감소
        Final_ItemCool = 1.0f - (soul.itemcooldownspeed * 0.05f);
        Final_SkillCool = 1.0f - (soul.skillcooldownspeed * 0.10f);
        Final_DmgReduce = 1.0f - (soul.def_dmg_reduce * 0.05f);

        // 5. 특수
        Final_CanRevive = soul.def_revive > 0;
        Final_ReviveHpPercent = (soul.def_revive == 1) ? 0.3f : (soul.def_revive >= 2 ? 0.5f : 0f);
        
        Final_SteamCost = 1.0f - (soul.sp_cost_reduce * 0.01f);
        Final_SteamSpeed = 1.0f + (soul.sp_cd_reduce * 0.05f);
        Final_DashCount = soul.sp_dash_stack;
        Final_Overheat = soul.sp_overheat > 0;

        // PlayerStatus에 주입
        UpdatePlayerStatus();
    }

    private void UpdatePlayerStatus()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;
        
        PlayerStatus status = player.GetComponent<PlayerStatus>();
        if (status == null) return;

        status.AtkMultiplier = Final_PhyAtk / 10f;
        status.FinalMaxHP = Final_HP;
        status.FinalDefense = Final_Def;
        status.CritChanceBonus = Final_CritChance;
        status.CritDamageBonus = Final_CritDmg;
        
        status.ItemCooldownMult = Final_ItemCool;
        status.SkillCooldownMult = Final_SkillCool;
        status.DamageReduceMult = Final_DmgReduce;

        status.CanRevive = Final_CanRevive;
        status.ReviveHpPercent = Final_ReviveHpPercent;
        
        status.SteamCostMult = Final_SteamCost;
        status.SteamCoolSpeedMult = Final_SteamSpeed;
        status.BonusDashCount = Final_DashCount;
        status.IsOverheatEnhanced = Final_Overheat;
    }

    // 🚀 [이동됨] UI가 물어볼 때 대답해주는 함수
    // [StatDataManager.cs 내부]
    // UI가 물어볼 때 대답해주는 함수
    public string GetStatPreview(string key, int lv)
    {
        float baseDmg = 10f; float baseHp = 100f; 
        
        switch (key) 
        {
            case "physicalattack": return $"{baseDmg * (1.0f + (lv * 0.05f)):F1}";
            case "health": return $"{baseHp + (lv * 5f)}";
            case "defensivepower": return $"{lv * 5}";
            case "criticalchance": return $"{lv}%";
            case "itemcooldownspeed": return $"-{lv * 5}%";
            case "skillcooldownspeed": return $"-{lv * 10}%";
            case "def_dmg_reduce": return $"-{lv * 5}%";
            case "sp_cost_reduce": return $"-{lv * 1}%";
            case "sp_cd_reduce": return $"+{lv * 5}%";
            case "sp_dash_stack": return $"{2 + lv}회";
            case "sp_overheat": return lv > 0 ? "적용됨" : "미적용";
            case "def_revive": return lv == 0 ? "없음" : (lv == 1 ? "30% 부활" : "50% 부활");
            default: return "-";
        }
    }

    public void OnPlayerDead()
    {
        if (ItemDataManager.Instance != null) ItemDataManager.Instance.ResetData();
        playerData.currentHP = 100f + (SoulDataManager.Instance.saveData.stats.health * 5f);
        SaveData();
        CalculateFinalStats();
    }

    public void SaveData()
    {
        string json = JsonUtility.ToJson(playerData, true);
        File.WriteAllText(filePath, json);
#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif
    }

    public void LoadData()
    {
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            playerData = JsonUtility.FromJson<PlayerCurrentData>(json);
        }
        else
        {
            playerData = new PlayerCurrentData();
            SaveData();
        }
    }
}