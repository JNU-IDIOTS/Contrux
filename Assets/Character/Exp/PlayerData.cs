using System.Collections.Generic;

[System.Serializable]
public class SerializableKeyValue
{
    public string key;
    public bool value;
}
[System.Serializable]
public class PlayerData
{
    public int playerLevel;
    public int playerExp;
    public int playerTotalExp;
    public int expToNextLevel;
    public int soulExp;
    public List<string> unlockedButtons = new(); // 기존 구조를 그대로 유지
                                                 // 저장 전용 리스트
    public List<SerializableKeyValue> appliedBuffList = new();

    // 런타임 전용 딕셔너리 (저장되지 않음)
    [System.NonSerialized]
    public Dictionary<string, bool> appliedBuffs = new();
}
