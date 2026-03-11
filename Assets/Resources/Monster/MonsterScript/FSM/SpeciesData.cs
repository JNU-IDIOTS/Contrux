// 몬스터의 종족별 데이터를 정의하는 ScriptableObject
// 유니티 툴 내에서 데이터 추가 방법 : Assets -> Create -> AI -> SpeciesData
using UnityEngine;
public enum AttackPatternType 
{
    Standard, // 일반 공격 (ExecuteAttack)
    Buster,   // 드워프 버스터 (BusterAttack)
    Wolf      // 늑대 (WolfAttack)
}
[CreateAssetMenu(fileName = "NewSpeciesData", menuName = "AI/SpeciesData")]
public class SpeciesData : ScriptableObject
{
    //참고!! 여기 있는 값들은 최초의 기본 값이기에 실제 게임 내에서 몬스터가 가지는 값들과는 다를 수 있음
    //실제 게임 내에서 몬스터가 가지는 값들은 SpeciesData를 복사하여 몬스터별로 다른 값을 가지게 설계된 코드
    [Header("1. 식별 정보")]
    public string speciesTag; // "Wolf", "Dwarf" 등 유니티 태그와 일치

    [Header("2. 열망(Desire) 스탯")]
    [Range(1f, 10f)]
    public float maxCourage = 10f; // 최대 용기

    public float aggression = 5f;  // 공격성 (1~10). 높을수록 SAttack 확률 증가

    [Header("3. 기본 스탯 (EnemyAI.cs에서 가져옴)")]
    public int maxHP = 100;
    public float attackRange = 2f;
    [Range(0f, 360f)]
    public float attackAngle = 360f;
    public Color indicatorColor = new Color(1f, 0f, 0f, 0.3f);    
    public int NAttackDamage = 20;
    public int SAttackDamage = 40;
    public float attackCooldown = 1.5f;

    [Header("4. 이동 스탯 (ChaseState/IdleState에서 가져옴)")]
    public float patrolSpeed = 1.0f; //순찰 속도 (IdleState용)
    public float chaseSpeed = 3.0f; //추격 속도 (ChaseState용)

    [Header("5. 탐지 스탯 (EnemyAI/IdleState에서 가져옴)")]
    public float detectionRadius = 8f; // 기본 탐지 범위
    public float loseDetectionRadius = 10f; // 탐지 상실 범위
    public float raycastDistance = 0.3f; // 낙하 방지 레이 길이
    public float horizontalOffset = 0.75f;
    public float verticalOffset = 0f;

    [Header("6. 공격 패턴")]
    public AttackPatternType attackType = AttackPatternType.Standard;
    
    [Header("7. 피격반응(TakeDamage 연동)")]
    public float defense = 1.0f;
    public float courageDamagePerHit = 2.0f;

    [Header("8. UI 설정")]
    public Vector3 healthBarOffset = Vector3.zero;

    [Header("9. 무리 본능 (Pack Mentality)")]
    public float alertRadius = 15f;          // 1. 경보를 울릴 범위 (경보 시스템)
    public float allyCheckRadius = 5f;         // 2. 아군으로 인식할 범위 (용기 보너스)
    public float courageBonusPerAlly = 0.2f; // 3. 아군 1명당 용기 보너스
    public float speedBonusPerAlly = 0.5f;    // 3. 아군 1명당 속도 보너스
    public float cooldownReductionPerAlly = 0.1f; // 4. 아군 1명당 쿨타임 감소 보너스
    public float MaxPackBonusCount = 5f;      // 5. 최대 보너스 적용 아군 수    

    [Header("10. 특수 행동 스탯")]
    public float backStepForce = 1f;
    public float busterChasingTime = 1.5f;
    public float busterAttackSpeed = 5f;

    [Header("11. 협동 전술(Flanking)")]
    public bool enableFlanking = false; // 협동 전술 사용 여부
    public float flankSpacing = 2.0f; // 측면 공격 거리

    [Header("리더 전술")]
    public bool isLeader = false;
    public float leaderBuffDamageMultiplier = 1.5f; //리더와 함께 싸울때 데미지 배율 
    public float commandRadius = 15f; //리더가 죽었을 때 효과가 전파되는 범위
    public bool useBerserk = true; //광폭화 스킬 사용 여부
    public float berserkThreshold = 0.4f; //광폭화 발동 체력 비율(현재 40퍼)
    public float berserkSpeedMultiplier = 2.0f; //광폭화시 속도 배율
    // (필요시 추가)
    // public AttackPatternType attackType = AttackPatternType.Normal; 
}