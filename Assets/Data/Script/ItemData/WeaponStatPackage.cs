using UnityEngine;

[System.Serializable]
public class WeaponStatPackage
{
    public float attackPower;
    public float attackSpeed;
    public float critChance;
    public float critDamage;
    public float range;

    public WeaponStatPackage(float atk, float spd, float crit, float critDmg, float rng)
    {
        attackPower = atk;
        attackSpeed = spd;
        critChance = crit;
        critDamage = critDmg;
        range = rng;
    }
}
