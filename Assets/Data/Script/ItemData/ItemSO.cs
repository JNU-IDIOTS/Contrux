using UnityEngine;

public enum ItemType { Weapon, Armor, Accessory }

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
public class ItemSO : ScriptableObject
{
    [Header("기본 정보")]
    public string itemName;
    public Sprite icon;
    public ItemType itemType;
    [TextArea]
    public string description;

    [Header("공통 능력치")]
    public int attackPower;
    public float attackSpeed;
    public int defense;
    public int magicPower;
    public float criticalChance;
    public float cooldownReduction;

    [Header("기타효과")]
    public int hpBonus;
    public int mpBonus;
}
