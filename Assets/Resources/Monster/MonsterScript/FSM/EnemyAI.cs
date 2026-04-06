using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(EnemyMovement))]
[RequireComponent(typeof(EnemyCombat))]
[RequireComponent(typeof(EnemyHealth))] // 심장 자동 부착
[RequireComponent(typeof(EnemySquad))]  // 무전기 자동 부착
public class EnemyAI : MonoBehaviour
{
    // --- 핵심 부서(컴포넌트) 참조 ---
    [HideInInspector] public EnemyMovement _enemyMovement;
    [HideInInspector] public EnemyCombat _enemyCombat;
    [HideInInspector] public EnemyHealth _enemyHealth;
    [HideInInspector] public EnemySquad _enemySquad;

    // --- 상태(State) 머신 ---
    public IEnemyState currentState;
    public IEnemyState idleState, chaseState, attackState, fleeState, searchState;

    // --- 데이터 및 핵심 스탯 ---
    public SpeciesData speciesData { get; private set; }
    public float currentMoveSpeed;
    public float currentCooldown;

    // --- 뇌의 판단 메모리 ---
    public Vector3 player;
    public Vector3 lastKnownPos;
    public float waitScore = 0f;
    public bool canAttack = false;
    private float attacktimer = 0f;
    private static int globalGhostCount = 0;
    private bool isMyGhostActive = false;

    // --- 플레이어 캐싱 ---
    private Transform _playerTransform;
    private Transform PlayerTransform
    {
        get
        {
            if (_playerTransform == null)
            {
                var playerGO = GameObject.FindWithTag("Player");
                if (playerGO != null) _playerTransform = playerGO.transform;
            }
            return _playerTransform;
        }
    }

    [HideInInspector] public bool showDebugGizmos = false;

    public Rigidbody2D GetRigidbody() => GetComponent<Rigidbody2D>();
    public Animator GetAnimator() => GetComponent<Animator>();

    private void Awake()
    {
        _enemyMovement = GetComponent<EnemyMovement>();
        _enemyCombat = GetComponent<EnemyCombat>();
        _enemyHealth = GetComponent<EnemyHealth>();
        _enemySquad = GetComponent<EnemySquad>();

        LoadDataByTag(gameObject.tag);
        if (speciesData == null) return;

        currentMoveSpeed = speciesData.chaseSpeed;
        currentCooldown = speciesData.attackCooldown;

        MonsterAnimatorController ani = GetComponent<MonsterAnimatorController>();
        idleState = new IdleState(this, ani);
        chaseState = new ChaseState(this, ani);
        attackState = new AttackState(this, ani);
        fleeState = new FleeState(this, ani);
        searchState = new SearchState(this, ani);
    }

    private void Start()
    {
        _enemyHealth.InitHealth(); // 심장 초기화
        ChangeState(idleState);
    }

    private void Update()
    {
        if (speciesData == null || PlayerTransform == null || _enemyHealth.isDeath) return;

        if (!_enemySquad.justAlerted)
        {
            player = PlayerTransform.position;
        }

        // 평화로울 때 용기 회복
        if (currentState == idleState || currentState == fleeState)
        {
            _enemyHealth.RecoverCourage(1f);
        }

        currentState?.Update();
    }

    private void FixedUpdate()
    {
        attacktimer += Time.deltaTime;
        if (attacktimer >= currentCooldown)
        {
            attacktimer = 0f;
            canAttack = true;
        }
    }

    private void OnDisable()
    {
        if (isMyGhostActive) SetPassThroughPlayer(false);
    }

    public void ChangeState(IEnemyState newState)
    {
        if (currentState == newState) return;

        currentState?.Exit();
        currentState = newState;
        currentState?.Enter();

        if (currentState == idleState) _enemyMovement.StopMoving();
    }

    private void LoadDataByTag(string tag)
    {
        SpeciesData[] allData = Resources.LoadAll<SpeciesData>("SpeciesData");
        for (int i = 0; i < allData.Length; i++)
        {
            if (allData[i].speciesTag == tag) { speciesData = allData[i]; return; }
        }
    }

    public bool IsPlayerAttackable()
    {
        if (player == null || _enemyHealth.isDeath) return false;
        return Vector2.Distance(transform.position, player) <= speciesData.attackRange;
    }

    // --- 협동 전술(충돌 무시) 및 쿨타임 제어 ---
    public void SetPassThroughPlayer(bool enablePassThrough)
    {
        Collider2D[] myColliders = GetComponentsInChildren<Collider2D>();
        Collider2D[] playerColliders = PlayerTransform?.GetComponentsInChildren<Collider2D>();

        if (myColliders != null && playerColliders != null)
        {
            foreach (var myCol in myColliders)
                foreach (var pCol in playerColliders) Physics2D.IgnoreCollision(myCol, pCol, enablePassThrough);
        }
    }

    public void SetGhostMode(bool enable)
    {
        if (isMyGhostActive == enable) return;
        isMyGhostActive = enable;
        int enemyLayer = LayerMask.NameToLayer("Enemy");

        if (enable)
        {
            globalGhostCount++;
            if (globalGhostCount > 0 && enemyLayer != -1) Physics2D.IgnoreLayerCollision(enemyLayer, enemyLayer, true);
        }
        else
        {
            globalGhostCount--;
            if (globalGhostCount < 0) globalGhostCount = 0;
            if (globalGhostCount == 0 && enemyLayer != -1) Physics2D.IgnoreLayerCollision(enemyLayer, enemyLayer, false);
        }
    }

    private Coroutine cooldownCoroutine;
    public void StartCooldown(float seconds = 3f)
    {
        if (cooldownCoroutine != null) StopCoroutine(cooldownCoroutine);
        cooldownCoroutine = StartCoroutine(CooldownCoroutine(seconds));
    }
    private IEnumerator CooldownCoroutine(float seconds) { yield return new WaitForSeconds(seconds); }

    // (기즈모 코드는 기존 유지 - 분량상 생략되었으나 원래 있던 OnDrawGizmosSelected 등 그대로 둬도 됨)
}