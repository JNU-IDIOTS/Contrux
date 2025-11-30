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

    // 🚀 [추가됨] JSON 파일에 기록될 최종 스탯들 (눈으로 확인용)
    public float saved_PhyAtk;
    public float saved_MagAtk;
    public float saved_Def;
    public float saved_MaxHP; // 최대 체력 정보
    public float saved_CritChance;
    public float saved_CritDmg;
    public float saved_AtkSpeed;
    public float saved_MoveSpeed;
    public float saved_SkillCool;
    public float saved_ItemCool;
}

public class StatDataManager : MonoBehaviour
{
    public static StatDataManager Instance;
    public PlayerCurrentData playerData;

    // [기본(깡통) 스탯 설정]
    [Header("=== [1. 플레이어 기본 스탯 (Base)] ===")]
    public float Base_Atk = 10f;
    public float Base_HP = 100f;
    public float Base_Def = 0f;
    public float Base_CritChance = 0f;     
    public float Base_CritDmg = 1.5f;      
    public float Base_MoveSpeed = 5.0f;
    public float Base_AtkSpeed = 1.0f;

    // [최종 합산 스탯]
    [Header("=== [4. 최종 계산 결과 (Final)] ===")]
    public float Final_HP;
    public float Final_Def;
    public float Final_PhyAtk;
    public float Final_MagAtk;
    public float Final_AtkSpeed;
    public float Final_MoveSpeed;
    public float Final_SkillCool;
    public float Final_ItemCool;
    public float Final_CritChance;
    public float Final_CritDmg;
    public float Final_DmgReduce;
    
    public bool Final_CanRevive;
    public float Final_ReviveHpPercent;
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

    // 🚀 [추가] 게임 종료 시 현재 상태 저장
    private void OnApplicationQuit()
    {
        SaveCurrentStatus();
    }

    public void SaveCurrentStatus()
    {
        // 플레이어의 현재 체력을 찾아와서 데이터에 갱신
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            var health = player.GetComponent<PlayerHealth>();
            if (health != null) playerData.currentHP = health.currentHealth;
        }
        SaveData();
    }

    [ContextMenu("스탯 강제 재계산")]
    public void CalculateFinalStats(PlayerStatus player = null)
    {
        var soul = (SoulDataManager.Instance != null) ? SoulDataManager.Instance.saveData.stats : new SoulStats();
        var item = (ItemDataManager.Instance != null) ? ItemDataManager.Instance.itemData : new ItemStats();

        if (player == null)
        {
            GameObject pObj = GameObject.FindGameObjectWithTag("Player");
            if (pObj != null) player = pObj.GetComponent<PlayerStatus>();
        }

        // --- [최종 계산] ---
        float soulAtk = soul.physicalattack * 0.05f;
        float finalPhyAtk = Base_Atk * (1.0f + soulAtk + item.bonusPhyAtk);
        float finalMagAtk = Base_Atk * (1.0f + soulAtk + item.bonusMagAtk);

        float finalHp = Base_HP + (soul.health * 5.0f) + item.bonusHp;
        float finalDef = Base_Def + (soul.defensivepower * 5.0f) + item.bonusDef;

        float finalCritChance = Base_CritChance + (soul.criticalchance * 0.01f) + item.bonusCritChance;
        float finalCritDmg = Base_CritDmg + (soul.criticalchance * 0.01f) + item.bonusCritDmg;

        float finalAtkSpeed = Base_AtkSpeed + (soul.attackspeed * 0.02f);
        float finalMoveSpeed = Base_MoveSpeed + (soul.movementspeed * 0.1f);

        float finalItemCool = 1.0f - (soul.itemcooldownspeed * 0.05f);
        float finalSkillCool = 1.0f - (soul.skillcooldownspeed * 0.10f);
        float finalDmgReduce = 1.0f - (soul.def_dmg_reduce * 0.05f);

        bool finalCanRevive = soul.def_revive > 0;
        float finalRevivePercent = (soul.def_revive == 1) ? 0.3f : (soul.def_revive >= 2 ? 0.5f : 0f);
        
        float finalSteamCost = 1.0f - (soul.sp_cost_reduce * 0.01f);
        float finalSteamSpeed = 1.0f + (soul.sp_cd_reduce * 0.05f);
        int finalDashCount = soul.sp_dash_stack;
        bool finalOverheat = soul.sp_overheat > 0;

        // 인스펙터 확인용 갱신
        Final_HP = finalHp;
        Final_PhyAtk = finalPhyAtk;
        Final_Def = finalDef;

        // 🚀 [저장용 데이터 업데이트] JSON에 기록하기 위해 변수에 넣음
        playerData.saved_PhyAtk = finalPhyAtk;
        playerData.saved_MagAtk = finalMagAtk;
        playerData.saved_MaxHP = finalHp;
        playerData.saved_Def = finalDef;
        playerData.saved_CritChance = finalCritChance;
        playerData.saved_CritDmg = finalCritDmg;
        playerData.saved_MoveSpeed = finalMoveSpeed;
        playerData.saved_AtkSpeed = finalAtkSpeed;
        playerData.saved_ItemCool = finalItemCool;
        playerData.saved_SkillCool = finalSkillCool;
        
        SaveData(); // 🔥 파일에 쓰기!

        // 플레이어에게 주입
        if (player != null)
        {
            player.AtkMultiplier = finalPhyAtk / Base_Atk;
            player.FinalMaxHP = finalHp;
            player.FinalDefense = finalDef;
            player.CritChanceBonus = finalCritChance;
            player.CritDamageBonus = finalCritDmg;
            player.ItemCooldownMult = finalItemCool;
            player.SkillCooldownMult = finalSkillCool;
            player.DamageReduceMult = finalDmgReduce;
            player.CanRevive = finalCanRevive;
            player.ReviveHpPercent = finalRevivePercent;
            player.SteamCostMult = finalSteamCost;
            player.SteamCoolSpeedMult = finalSteamSpeed;
            player.BonusDashCount = finalDashCount;
            player.IsOverheatEnhanced = finalOverheat;
        }
        
        Debug.Log($"📊 스탯 계산 및 저장 완료. (HP: {finalHp}, 공: {finalPhyAtk})");
    }

    // [UI 미리보기]
    public string GetStatPreview(string key, int lv)
    {
        switch (key) 
        {
            case "physicalattack": return $"{Base_Atk * (1.0f + (lv * 0.05f)):F1}";
            case "health": return $"{Base_HP + (lv * 5f)}";
            case "defensivepower": return $"{Base_Def + (lv * 5f)}";
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
        
        // 죽었을 때 기본 체력(기본+소울)으로 복구해서 저장
        float soulHp = SoulDataManager.Instance.saveData.stats.health * 5f;
        playerData.currentHP = Base_HP + soulHp;
        
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
            playerData.currentHP = Base_HP;
            SaveData();
        }
    }
}