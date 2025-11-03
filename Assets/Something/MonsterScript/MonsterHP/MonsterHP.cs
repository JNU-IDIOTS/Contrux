using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 몬스터의 체력 관리 및 IHealth 인터페이스 구현
/// </summary>
public class MonsterHP : MonoBehaviour, IHealth
{
    [Header("HP 설정")]
    [SerializeField] public int maxHP = 5;
    [SerializeField] public float currentHP;
    [SerializeField] private bool IsDead = false;

    [Header("피격 쿨타임")]
    [SerializeField] private float damageCooldown = 1.0f;
    private float lastDamageTime = -999f;

    [Header("UI")]
    [Tooltip("EnemyHealthBarUI 프리팹")]
    [SerializeField] private GameObject healthBarPrefab;

    [Tooltip("체력바를 띄울 Canvas (월드 스페이스)")]
    [SerializeField] private Canvas worldSpaceCanvas;

    [Tooltip("이 몬스터 전용 체력바 오프셋")]
    [SerializeField] private Vector3 healthBarOffset = new Vector3(0, 1.5f, 0);

    private EnemyHealthBarUI healthBarUI;

    private bool isStunned = false;

    private EnemyAI enemy;
    private MonsterAnimatorController ani;
    private MonsterDropGold dropper;

    private void Awake()
    {
        // 초깃값 세팅: 인터페이스 구현용
        if (CompareTag("Wolf")) { maxHP = 30; healthBarOffset = new Vector3(0f, -0.9f, 0f); }
        else if (CompareTag("Dwarf")) { maxHP = 100; healthBarOffset = new Vector3(0f, -0.3f, 0f); }
        else if (CompareTag("Dwarf_Hammer")) { maxHP = 100; healthBarOffset = new Vector3(-0.35f, -0.3f, 0f); }
        else if (CompareTag("DwarfBuster")) { maxHP = 250; healthBarOffset = new Vector3(0f, -0.3f, 0f); }
        else if (CompareTag("Goblin")) { maxHP = 70; healthBarOffset = new Vector3(0f, -1.75f, 0f); }
        else { maxHP = 10; healthBarOffset = new Vector3(0f, 0f, 0f); }
        currentHP = maxHP;
    }

    private void Start()
    {
        enemy = GetComponent<EnemyAI>();
        ani = GetComponent<MonsterAnimatorController>();

        if (enemy == null)
            Debug.LogError("EnemyAI 컴포넌트를 찾을 수 없습니다.", this);

    // prefab을 Canvas 하위로 붙이면서 생성
    var go = Instantiate(healthBarPrefab,worldSpaceCanvas.transform.position, // Vector3
        worldSpaceCanvas.transform.rotation , /* Quaternion */  worldSpaceCanvas.transform /*부모 Transform*/ );
        var ui = go.GetComponent<EnemyHealthBarUI>();
        ui.SetTarget(this.GetComponent<IHealth>(), this.transform, healthBarOffset);
    }

    private void OnEnable()
    {
        if (worldSpaceCanvas == null)
        {
            Debug.LogError("[MonsterHP] healthBarCanvas를 할당하세요!", this);
            return;
        }

        // 체력바 프리팹 인스턴스화 및 초기화
        //GameObject hb = Instantiate(healthBarPrefab, healthBarCanvas.transform);
        //healthBarUI = hb.GetComponent<EnemyHealthBarUI>();
        //healthBarUI.SetTarget(this, transform, healthBarOffset);
        //healthBarUI.UpdateHealthUI();
    }

    private void OnDisable()
    {
        if (healthBarUI != null)
            Destroy(healthBarUI.gameObject);
    }

    /// <summary>
    /// 몬스터가 데미지를 받을 때 호출
    /// </summary>
    public void Getdamage(float damage)
    {
        Debug.Log($"데미지 입음 : ");
        if (IsDead) return;
        //if (Time.time - lastDamageTime < damageCooldown) return;
        ani.Hurt();
        lastDamageTime = Time.time;
        currentHP -= damage;
        Debug.Log($"{currentHP}");
        //animator.SetTrigger("Hurt");

        if (currentHP <= 0)
        {
            IsDead = true;
            enemy.IsDeath = true;
            enemy.Die();
            DropManager.Instance?.SpawnDrop(transform.position);
            dropper?.Drop();
        }
    }



    #region IHealth 구현
    float IHealth.currentHP => currentHP;
    float IHealth.maxHP => maxHP;
    #endregion
}
