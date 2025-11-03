using System.Collections.Generic;

[System.Serializable]
public class DropTableData
{
    public string stageName;

    public float weaponDropRate = 100f;

    public float commonRate;
    public float rareRate;
    public float epicRate;
    public float uniqueRate;
    public float legendaryRate;
    public List<int> allowedLevels;
    public List<string> allowedTypeStrings;
    [System.NonSerialized]
    public List<WeaponType> allowedTypes;
}

[System.Serializable]
public class DropTableCollection
{
    public List<DropTableData> tables;
}
