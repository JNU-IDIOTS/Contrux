using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public enum WeaponGrade { Common, Rare, Epic, Unique, Legendary }
public enum WeaponType { GreatSword, OneHandSword, Spear, Hammer, Dagger, Chain, Gauntlet, Bow, Staff }
[System.Serializable]
public class WeaponPrefabData
{
    [Header("기본 정보")]
    public string weaponName;
    public WeaponGrade grade;     // 등급
    public WeaponType type;       // 무기 종류
    public int level;             // 무기 레벨 (10, 20, 30, 40, 50)
    public Sprite icon;           // 무기 아이콘
    public string description;    // 무기 설명

    [Header("상점 정보")]
    public int price;
    public int sellprice;

    [Header("요구 스탯")]
    public int requiredHp;
    public int requiredStr;
    public int requiredDex;
    public int requiredInt;
    public int requiredluk;

    [Header("전투 속성")]
    public float attackPower;     // 물리 or 마법 공격력
    public float attackSpeed;     // 공격 속도
    public float critChance;      // 크리티컬 확률 (%)
    public float critDamage;      // 크리티컬 배수 (%)
    public float range;           // 사거리
}
