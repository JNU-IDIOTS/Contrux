using System.Collections;
using UnityEngine;

public class MagicGoblinAI : MonoBehaviour, IHealth
{
    [Header("=== Health Settings ===")]
    [Tooltip("최대 체력")] public int maxHP = 5;
    [Tooltip("피격 쿨타임(초)")] public float damageCooldown = 1f;

    [Header("=== HealthBar UI ===")]
    [Tooltip("EnemyHealthBarUI 프리팹")] public GameObject healthBarPrefab;
    [Tooltip("체력바를 띄울 Canvas (World Space)")] public Canvas healthBarCanvas;
    [Tooltip("Goblin마다 조절할 Y 오프셋")] public Vector3 healthBarOffset = new Vector3(0, 1.5f, 0);

    [Header("=== Attack Settings ===")]
    [Tooltip("떨어뜨릴 마법 투사체 Prefab (Rigidbody2D 필요)")] public GameObject magicProjectilePrefab;
    [Tooltip("플레이어 위에 스폰할 높이 오프셋")] public float dropHeight = 5f;
    [Tooltip("낙하 속도")] public float dropSpeed = 10f;
    [Tooltip("공격 쿨다운(초)")] public float attackCooldown = 2f;
    [Tooltip("마법 차징(초)")] public float magicChargingTime = 2f;

    [Header("=== Target Settings ===")]
    [Tooltip("공격할 대상 레이어 (Player 레이어만 체크하세요)")]
    public LayerMask targetLayer;

    // 내부 상태
    public float currentHP;
    private float lastDamageTime = -999f;
    private bool isDead;
    private bool canAttack = true;

    // 플레이어 추적
    private Transform playerTransform;

    // UI
    private EnemyHealthBarUI healthBarUI;

    // 애니메이터 컨트롤러
    private MonsterAnimatorController ani;

    private void Awake()
    {
        // 체력 초기화 및 컴포넌트 캐싱
        currentHP = maxHP;
        ani = GetComponent<MonsterAnimatorController>();

        // healthBarCanvas가 할당되지 않았다면 씬에서 자동 검색
        if (healthBarCanvas == null)
        {
            healthBarCanvas = Object.FindFirstObjectByType<Canvas>();
            if (healthBarCanvas == null)
                Debug.LogError("[MagicGoblinAI] healthBarCanvas가 할당되지 않았습니다.", this);
        }
    }

    private void OnEnable()
    {
        if (healthBarPrefab == null || healthBarCanvas == null)
        {
            Debug.LogError("[MagicGoblinAI] HealthBar 관련 프리팹/캔버스가 없습니다.", this);
            return;
        }

        var hbGO = Instantiate(healthBarPrefab, healthBarCanvas.transform);
        healthBarUI = hbGO.GetComponent<EnemyHealthBarUI>();
        if (healthBarUI == null)
        {
            Debug.LogError("[MagicGoblinAI] healthBarPrefab에 EnemyHealthBarUI가 없습니다.", this);
            return;
        }
        healthBarUI.SetTarget(this, transform, healthBarOffset);
        healthBarUI.UpdateHealthUI();
    }

    private void OnDisable()
    {
        if (healthBarUI != null)
            Destroy(healthBarUI.gameObject);
    }

    float IHealth.currentHP => currentHP;
    float IHealth.maxHP => maxHP;

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        lastDamageTime = Time.time;
        currentHP -= damage;
        healthBarUI?.UpdateHealthUI();

        if (currentHP <= 0) Die();
    }

    private void Die()
    {
        isDead = true;
        StopAllCoroutines();
        this.enabled = false;
        if (healthBarUI != null) Destroy(healthBarUI.gameObject);
        Destroy(gameObject, 1f);
    }

    /// <summary>
    /// 자식 콜라이더에서 호출: 플레이어 감지
    /// </summary>
    public void OnPlayerDetected(Transform player)
    {
        // 레이어 마스크로 필터링: targetLayer에 포함된 레이어가 아니면 무시
        if (((1 << player.gameObject.layer) & targetLayer.value) == 0)
            return;

        playerTransform = player;
    }

    /// <summary>
    /// 자식 콜라이더에서 호출: 플레이어 감지 해제
    /// </summary>
    public void OnPlayerLost(Transform player)
    {
        if (playerTransform == player)
            playerTransform = null;
    }

    private void Update()
    {
        if (playerTransform != null && canAttack && !isDead)
            StartCoroutine(MagicCasting());
    }

    private IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    private IEnumerator MagicCasting()
    {
        canAttack = false;
        ani.MGoblinCharging();
        yield return new WaitForSeconds(magicChargingTime);

        if (magicProjectilePrefab != null && playerTransform != null)
        {
            Vector3 spawnPos = new Vector3(
                playerTransform.position.x,
                playerTransform.position.y + dropHeight,
                0f
            );
            var proj = Instantiate(magicProjectilePrefab, spawnPos, Quaternion.identity);

            // (선택) 필요하다면 프로젝타일 레이어도 설정 가능
            // proj.layer = LayerMask.NameToLayer("EnemyProjectile");

            var rb = proj.GetComponent<Rigidbody2D>();
            if (rb != null)
                rb.linearVelocity = Vector2.down * dropSpeed;
        }

        StartCoroutine(AttackCooldown());
    }
}
