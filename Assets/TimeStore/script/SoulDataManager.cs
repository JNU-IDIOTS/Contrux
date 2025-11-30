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
    public int health;              
    public int defensivepower;      
    public int physicalattack;      
    public int magicattack;         
    public int attackspeed;         
    public int movementspeed;       
    public int skillcooldownspeed;  
    public int itemcooldownspeed;   
    public int criticalchance;      
    public int criticaldamage;      
    public int def_dmg_reduce;      
    public int def_revive;          
    public int sp_cost_reduce;      
    public int sp_cd_reduce;        
    public int sp_dash_stack;       
    public int sp_overheat;         
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

        // 🚀 [수정됨] 파일 이름을 직관적으로 변경
        filePath = Path.Combine(Application.persistentDataPath, "SoulShop_Data.json");
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
        if (field == null) return false;

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