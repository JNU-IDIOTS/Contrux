using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Inventory/ItemData")]
public class ItemData : ScriptableObject
{
    public int id;
    public string itemName;

    public int itemType; // 시너지 혹은 장비 종류 구분용
    public Sprite icon;
    public int count = 1;

    // 아이템 스탯 들어갈 자리
    public int InhanceLevel;
    public int HP;
    public int MagicDeal;
    public int PhysicalDeal;
    public int Defense;
    [TextArea]
    public string description;
    [TextArea]
    public string performance;
}
