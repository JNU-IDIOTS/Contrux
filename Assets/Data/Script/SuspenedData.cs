using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class SuspendedData
{
    public int playerLevel;
    public int playerExp;
    public int totalExp;
    public int expToNextLevel;
    public float currentHP;
    public string sceneName;
    public string savedTime;
    public List<string> unlockedButtons = new(); // 기존 구조를 그대로 유지
                                                 // 저장 전용 리스트
    public List<SerializableKeyValue> appliedBuffList = new();

    // 런타임 전용 딕셔너리 (저장되지 않음)
    [System.NonSerialized]
    public Dictionary<string, bool> appliedBuffs = new();
    private static string SavePath =>
        Path.Combine(Application.persistentDataPath, "Suspended_Run.json");

    public static void Save(SuspendedData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
        Debug.Log($"[SuspendedData] 저장 완료: {SavePath}");
    }

    public static SuspendedData Load()
    {
        if (!Exists()) return null;


        string json = File.ReadAllText(SavePath);
        return JsonUtility.FromJson<SuspendedData>(json);
    }

    public static void Delete()
    {
        if (Exists()) File.Delete(SavePath);
        Debug.Log("[SuspendedData] 삭제 완료");
    }

    public static bool Exists()
    {
        return File.Exists(SavePath);
    }
}