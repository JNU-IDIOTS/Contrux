using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;
using System.Security.Cryptography.X509Certificates;

public class Attack_Damage : MonoBehaviour
{
    [Header("공격 설정")]
    [SerializeField] private Transform attackPoint;          // 기본 공격 기준 위치
    [SerializeField] private float attackRange = 0.5f;       // 기본 공격 반경
    [SerializeField] private LayerMask enemyLayer;           // 적 레이어 설정
    [SerializeField] public int BasicattackDamage = 30;          // 기본 공격 데미지
    [SerializeField] public int BasicstrongAttackDamage = 100;    // 강공격(차징) 데미지
    [SerializeField] public int BasicstrongUPAttackDamage = 60;    // 강공격(차징) 데미지
    [SerializeField] public int OverHeatBounsDamage = 2;    // 강공격(차징) 데미지
    public static float CriticalDamage = 0;
    public static float CritChance = 0;
    public static float MaxHPDamage = 0;
    public static float SoulBuffCriticalDamage = 0;
    public static float SoulBuffCritChance = 0;
    public static int Critrand;
    private float weaponcirdmg = 0f;
    private float luckstat;

    // ===== 내부에서 사용할 참조 =====
    public WeaponStatPackage currentWeaponData;
    private SteamPressureSystem steampunk;

    private void Awake()
    {
        steampunk = GetComponent<SteamPressureSystem>();
        if (gameObject.name == "Ark/mana")
        {
            OverHeatBounsDamage = 1;
        }
    }

    private void Update()
    {
        /*BasicattackDamage += + StrStat;
        BasicstrongAttackDamage += StrStat;
        BasicstrongUPAttackDamage += StrStat;
        Debug.Log($"소울 버프로 인한 크리티컬 데미지 : {SoulBuffCriticalDamage}, 소울 버프로 인한 크리티컬 확률 : {SoulBuffCritChance}");*/
        luckstat = Ark_stat.luck;
        weaponcirdmg = (currentWeaponData.critDamage / 100) - 1; 
        CritChance = (luckstat * 0.1f) + currentWeaponData.critChance + SoulBuffCritChance; 
        Debug.Log($"현재 적용중인 무기 데미지 {currentWeaponData.attackPower}");
    }

    // 헬퍼 메서드
    private void AttackWithModifiers(float baseAttackDamage)
    {
        // 1) 베이스 + 무기 + MaxHP 데미지
        float damage = baseAttackDamage
                     + MaxHPDamage
                     + currentWeaponData.attackPower + (Ark_stat.strength * 2);

        // 2) 버프 배수
        damage *= GetBuffMultiplier();

        // 3) 크리티컬 배수
        Critrand = UnityEngine.Random.Range(0, 100); //크리티컬 확률 계산을 위한 랜덤 수 받기
        if(CritChance >= Critrand)
        {
            damage *= 1 + (weaponcirdmg + SoulBuffCriticalDamage);
            Debug.Log("크리발생");
        }

        // 4) 오버히트 배수 (오버히트 상태일 때만)
        if (gameObject.name == "Ark/SteamPunk")
        {
            if (steampunk.isOverheated)
                damage *= OverHeatBounsDamage;
        }

        // 5) 실제 공격
        DoAttack(attackPoint.position, attackRange, damage);
    }
    public void NormalAttack()
    {
        AttackWithModifiers(BasicattackDamage);
    }
    public void DownCommand()
    {
        AttackWithModifiers(BasicstrongAttackDamage);
    }
    public void SideCommand()
    {
        AttackWithModifiers(BasicstrongUPAttackDamage);
    }

    private float GetBuffMultiplier()
    {
        if (TryGetComponent<SoulBuffAttack>(out var buff) && buff.IsBuffActive)
        {
            return buff.BuffMultiplier;
        }
        //Debug.Log("[버프 확인] 공격 시점에 버프 비활성화 → 기본 배수 1.0");
        return 1f;
    }
    private void DoAttack(Vector2 point, float range, float damage)
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(point, range, enemyLayer);
        foreach (var col in hitEnemies)
        {
            if (col.TryGetComponent<MonsterHP>(out MonsterHP monster))
            {
                //Debug.Log($"총 데미지: {damage} 체력 비례 데미지 증가 : {MaxHPDamage}");
                Debug.Log("적 때림");
                monster.Getdamage(damage);
                //Debug.Log($"[PerformAttack] 최종 데미지: {damage}, 배수: {GetBuffMultiplier()}");
                Debug.Log(damage);
            }
            else if (col.TryGetComponent<MagicGoblinAI>(out MagicGoblinAI goblin))
            {
                //Debug.Log($"총 데미지: {damage} 체력 비례 데미지 증가 : {MaxHPDamage}");
                goblin.TakeDamage(damage);
                Debug.Log($"[PerformAttack] 최종 데미지: {damage}, 배수: {GetBuffMultiplier()}");
                Debug.Log(damage);
            }
            else if (col.TryGetComponent<SandWorm_FSM>(out SandWorm_FSM Sandworm))
            {
                Debug.Log("샌드웜 때림");
                Sandworm.Getdamage(damage);
                
                Debug.Log(damage);
            }
            else if (col.TryGetComponent<Boss1_FSM>(out Boss1_FSM boss1))
            {
                Debug.Log("샌드웜 때림");
                boss1.Getdamage(damage);
                Debug.Log(damage);
            }

        }
    }

    public void SetWeaponStats(WeaponStatPackage stats)
    {
        currentWeaponData = stats;
    }

}
