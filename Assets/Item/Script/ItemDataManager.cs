using UnityEngine;
using System.IO;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

// [Item_data.json 구조]
[System.Serializable]
public class ItemStats
{
    public float bonusHp;
    public float bonusDef;
    public float bonusPhyAtk;
    public float bonusMagAtk;
    public float bonusCritChance;
    public float bonusCritDmg;
    
    public List<string> inventory = new List<string>();
}

public class ItemDataManager : MonoBehaviour
{
    public static ItemDataManager Instance;
    public ItemStats itemData;
    private string filePath;

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }

        // 🚀 Item_data.json (죽으면 초기화됨)
        filePath = Path.Combine(Application.persistentDataPath, "Item_data.json");
        LoadData();
    }

    public void SaveData()
    {
        string json = JsonUtility.ToJson(itemData, true);
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
            itemData = JsonUtility.FromJson<ItemStats>(json);
        }
        else
        {
            ResetData();
        }
    }

    // 💀 사망 시 호출 -> 초기화
    public void ResetData()
    {
        Debug.Log("💀 아이템 데이터 리셋 (Item_data.json 초기화)");
        itemData = new ItemStats(); // 0으로 리셋
        SaveData();
    }

    // 아이템 획득
    public void AddItem(string itemName, float hp, float def, float phyAtk, float magAtk, float crit)
    {
        itemData.inventory.Add(itemName);
        itemData.bonusPhyAtk += phyAtk;
        itemData.bonusHp += hp;
        itemData.bonusDef += def;
        itemData.bonusCritChance += crit;
        
        SaveData();
        
        // 🚀 획득 후 스탯 총괄 매니저에게 "재계산해!" 요청
        if (StatDataManager.Instance != null)
            StatDataManager.Instance.CalculateFinalStats();
    }
}