using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// ������ ü�� ���� �� IHealth �������̽� ����
/// </summary>
public class MonsterHP : MonoBehaviour, IHealth
{
    [Header("HP ����")]
    [SerializeField] public int maxHP = 5;
    [SerializeField] public float currentHP;
    [SerializeField] private bool IsDead = false;

    [Header("�ǰ� ��Ÿ��")]
    [SerializeField] private float damageCooldown = 1.0f;
    private float lastDamageTime = -999f;

    [Header("UI")]
    [Tooltip("EnemyHealthBarUI ������")]
    [SerializeField] private GameObject healthBarPrefab;

    [Tooltip("ü�¹ٸ� ��� Canvas (���� �����̽�)")]
    [SerializeField] private Canvas worldSpaceCanvas;

    [Tooltip("�� ���� ���� ü�¹� ������")]
    [SerializeField] private Vector3 healthBarOffset = new Vector3(0, 1.5f, 0);

    private EnemyHealthBarUI healthBarUI;

    private bool isStunned = false;

    private EnemyAI enemy;
    private MonsterAnimatorController ani;
    private MonsterDropGold dropper;

    private void Awake()
    {
        // �ʱ갪 ����: �������̽� ������
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
            Debug.LogError("EnemyAI ������Ʈ�� ã�� �� �����ϴ�.", this);

    // prefab�� Canvas ������ ���̸鼭 ����
    var go = Instantiate(healthBarPrefab,worldSpaceCanvas.transform.position, // Vector3
        worldSpaceCanvas.transform.rotation , /* Quaternion */  worldSpaceCanvas.transform /*�θ� Transform*/ );
        var ui = go.GetComponent<EnemyHealthBarUI>();
        ui.SetTarget(this.GetComponent<IHealth>(), this.transform, healthBarOffset);
    }

    private void OnEnable()
    {
        if (worldSpaceCanvas == null)
        {
            Debug.LogError("[MonsterHP] healthBarCanvas�� �Ҵ��ϼ���!", this);
            return;
        }

        // ü�¹� ������ �ν��Ͻ�ȭ �� �ʱ�ȭ
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
    /// ���Ͱ� �������� ���� �� ȣ��
    /// </summary>
    /*public void Getdamage(float damage)
    {
        Debug.Log($"������ ���� : ");
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
    }*/



    #region IHealth ����
    float IHealth.currentHP => currentHP;
    float IHealth.maxHP => maxHP;
    #endregion
}
