using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Search;

public class EnemyAI : MonoBehaviour, IHealth
{
    public IEnemyState currentState;
    public IEnemyState idleState, chaseState, attackState, fleeState, searchState; //모드 변수 선언.
    // 1. 데이터베이스 원본 (로드됨)
    public SpeciesData speciesData { get; private set; }

    // 2. 현재 몬스터 스탯(변동됨, 일단은 용기 스탯)
    public float currentCourage;
    public bool hasDealtDamage = false;

    public bool justAlerted = false;
    private Coroutine alertShieldCoroutine = null;
    private float allyCourageBonus = 0f; // 아군 용기 보너스 누적 변수
    private float courageCheckTimer = 0f;
    public float currentMoveSpeed { get; private set; }
    public float currentCooldown { get; private set; }
    private LayerMask enemyLayerMask; // (성능을 위해 레이어 마스크를 캐시)
    public int mySquadRank { get; private set; } = 0;
    public Vector3 lastKnownPos; // 마지막으로 플레이어가 목격된 위치\
    private static int globalGhostCount = 0; // 유령 모드 사용 중인 몬스터 수
    private bool isMyGhostActive = false; // 이 몬스터가 유령 모드인지 여부(안전장치)
    private bool isBerserkMode = false; //광폭화 상태인지 체크
    // 트리거 기반 감지 처리
    public bool playerInRange = false;
    public bool playerAttackable = false;
    public float attackRange = 2f;
    public EnemyAI currentLeader = null;
    public AttackIndicator attackVisualizer;
    //플레이어
    public Vector3 player;
    private Transform _playerTransform;
    private Transform _playerBackPos;
    private Transform PlayerTransform //정적캐싱
    {
        get
        {
            if (_playerTransform == null)
            {
                var playerGO = GameObject.FindWithTag("Player");
                if (playerGO != null)
                    _playerTransform = playerGO.transform;
            }
            return _playerTransform;
        }
    }

    public float moveSpeed /*= 1f* 초기값이니 일단 삭제*/;
    private Vector2 savedVelocity;

    [Header("공격")]
    [SerializeField] public LayerMask playerLayer; // 공격 대상이 될 레이어
    public float attackRangeradius = 2f;
    public Transform attackPoint;
    public float AttackTimer = 0f;
    public float cooldown = 1.5f;
    public bool canAttack = false;

    [Header("백스텝")]
    [SerializeField] private float BackStepForce = 1f;

    [Header("넉백 범위 설정")]
    [SerializeField] private float minKnockbackForce = 0.2f;
    [SerializeField] private float maxKnockbackForce = 0.4f;
    [SerializeField] private float minKnockbackUpwardForce = 0.2f;
    [SerializeField] private float maxKnockbackUpwardForce = 0.4f;

    public bool IsHit { get; protected set; } = false;

    public float BusterChasingTime = 0f;
    public float BusterAttackSpeed = 0f;

    public float waitScore = 0f;
    public int currentHP;
    public bool IsDeath = false;
    public bool IsAttacking = false;
    public bool IsMoving = false;
    public float attacktimer = 0f;
    public bool steampunk = false;
    public bool magic = false;
    public bool select = false;

    //몬스터 콜라이더(자신)와 플레이어 콜라이더 저장용
    private Collider2D myCollider;
    private Collider2D playerCollider;

    [Header("시각화 온오프")]
    [HideInInspector] public bool showDebugGizmos = false; // 에디터에서 제어할 변수

    public Rigidbody2D rb;
    private Coroutine cooldownCoroutine;  // 쿨타임을 위한 코루틴
    private MonsterAnimatorController ani;
    public GameObject attackAreaObject;
    private DetectionArea detection;
    private DwarfBuster_SAttack_Collider hitboxScript;
    private ObjectActive objectactive;
    public DwarfBusterBullet dwarfbuster;

    public Rigidbody2D GetRigidbody()
    {
        return rb;
    }
    public Animator GetAnimator()
    {
        return GetComponent<Animator>();
    }
    private void Awake()
    {
        Debug.Log($"[{gameObject.name}] Awake() 시작");
        //종족별 데이터에 맞게 초기값 설정하기
        LoadDataByTag(gameObject.tag);
        if (speciesData == null)
        {
            Debug.Log($"[{gameObject.name}] SpeciesData 로드 실패! 태그: {gameObject.tag}", this);
            return;
        }
        Debug.Log($"[{gameObject.name}] SpeciesData 로드 성공! maxCourage={speciesData.maxCourage}");
        currentHP = speciesData.maxHP;
        currentCourage = speciesData.maxCourage;
        cooldown = speciesData.attackCooldown;
        currentMoveSpeed = speciesData.chaseSpeed;
        currentCooldown = speciesData.attackCooldown;
        enemyLayerMask = LayerMask.GetMask("Enemy"); //Enemy 레이어 마스크 캐시

        ani = GetComponent<MonsterAnimatorController>();
        rb = GetComponent<Rigidbody2D>();
        idleState = new IdleState(this, ani);
        chaseState = new ChaseState(this, ani);
        attackState = new AttackState(this, ani);
        fleeState = new FleeState(this, ani); //도주 상태
        searchState = new SearchState(this, ani); //수색 상태
        detection = GetComponent<DetectionArea>();
        objectactive = GetComponent<ObjectActive>();
        dwarfbuster = GetComponent<DwarfBusterBullet>();
        myCollider = GetComponent<Collider2D>(); //자신의 콜라이더 가져오기

        try
        {
            DebugLogLoadedData();
            Debug.LogWarning($"[{gameObject.name}] DebugLogLoadedData() 완료됨");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[{gameObject.name}] ❌ DebugLogLoadedData() 에러: {e.Message}\n{e.StackTrace}");
        }

        try
        {
            if (attackVisualizer != null)
            {
                Debug.LogWarning($"[{gameObject.name}] attackVisualizer.Setup() 호출 중...");
                // (스프라이트는 메쉬 방식이라 안 쓰지만, 호환성을 위해 null 전달)
                attackVisualizer.Setup(null, speciesData.indicatorColor, speciesData.attackAngle);
                Debug.LogWarning($"[{gameObject.name}] attackVisualizer.Setup() 완료됨");
            }
            else
            {
                Debug.LogWarning($"[{gameObject.name}] !! attackVisualizer is NULL!!");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[{gameObject.name}] ❌ attackVisualizer 에러: {e.Message}\n{e.StackTrace}");
        }

        //Debug.LogWarning($"[{gameObject.name}] Awake() 완료! activeSelf={gameObject.activeSelf}, enabled={enabled}");
    }


    void Start()
    {
        ChangeState(idleState);
        attacktimer = 0f;
        //플레이어 콜라이더가 아직 없다면 찾기(플레이어가 씬에 로드 된 후)
        if (playerCollider == null && PlayerTransform != null)
        {
            playerCollider = PlayerTransform.GetComponent<Collider2D>();
        }
    }

    private void Update()
    {
        /*IdleState.raycastDistance = raycastDistance;
        IdleState.horizontalOffset = horizontalOffset;
        IdleState.verticalOffset = verticalOffset; 삭제 */
        //Debug.Log(raycastDistance);
        if (speciesData == null)
        {
            Debug.LogWarning($"[{gameObject.name}] Update(): speciesData is NULL!");
            return; // 종족 데이터가 할당되지 않았으면 정지
        }
        if (PlayerTransform == null)
        {
            Debug.LogWarning($"[{gameObject.name}] Update(): PlayerTransform is NULL!");
            return; // 아직 할당 안 됐으면 건너뛰기
        }
        if (_playerBackPos == null)
        {
            // 플레이어의 직계 자식 중에서 이름이 "BackPos"인 것을 찾음
            _playerBackPos = PlayerTransform.Find("BackPos");
        }
        // 위치 계산
        if (!justAlerted)
        {
            player = PlayerTransform.position;
            _playerBackPos = PlayerTransform.Find("BackPos");
        }
        if (IsDeath)
        {
            Debug.LogWarning($"[{gameObject.name}] Update(): IsDeath is TRUE!");
            return;
        }

        courageCheckTimer += Time.deltaTime;
        if (courageCheckTimer >= 0.5f) //0.5초마다 용기 보너스 갱신
        {
            UpdateCourageBonus();
            courageCheckTimer = 0f;
        }

        if (currentState == idleState || currentState == fleeState)
        {
            if (currentCourage < speciesData.maxCourage)
            {
                currentCourage += 1f * Time.deltaTime;
                Debug.Log($"용기 : {currentCourage}");
            }
        }

        // 디버그: 현재 상태 확인
        Debug.Log($"[{gameObject.name}] 상태: {currentState?.GetType().Name ?? "NULL"}, player: {player}, distance: {Vector2.Distance(transform.position, player)}");

        currentState?.Update();

    }

    void FixedUpdate()
    {
        attacktimer += Time.deltaTime;
        //Debug.Log(currentState);
        if (attacktimer >= currentCooldown)
        {
            attacktimer = 0f;
            canAttack = true;
        }
    }
    private void OnDisable() //유령모드 안전장치
    {
        // 몬스터가 꺼질 때, 유령 모드였다면 끄고 나감
        if (isMyGhostActive)
        {
            SetPassThroughPlayer(false);
        }
    }
    public void FaceDirection(int dir) // 그림 전환 메소드
    {
        if (dir == 0) return;

        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * -dir;
        transform.localScale = scale;
    }

    public void SetDetectionRadius(float radius)
    {
        Transform detectionArea = transform.Find("DetectionArea");
        if (detectionArea == null)
        {
            //Debug.Log("DetectionArea 오브젝트를 찾을 수 없습니다.");
            return;
        }
        CircleCollider2D collider = detectionArea.GetComponent<CircleCollider2D>();
        if (collider == null)
        {
            //Debug.Log("DetectionArea에 CircleCollider2D가 없습니다.");
            return;
        }
        collider.radius = radius;
    }

    public void ChangeState(IEnemyState newState)
    {
        if (currentState == newState) return;  // 이미 같은 상태로 전환된 경우는 처리하지 않음

        currentState?.Exit();
        currentState = newState;  // 새로운 상태로 전환
        currentState?.Enter();    // 새로운 상태 시작

        // 상태에 따라 이동 방식 제어
        if (currentState == chaseState)
        {
            //Debug.Log("Entering chase mode!");
        }
        else if (currentState == idleState)
        {
            //Debug.Log("Entering idle mode!");
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);  // 대기 모드에서는 이동하지 않음
        }
    }

    public void MoveTowardsPlayer()
    {
        if (player == null) return;

        Vector2 direction = (player - transform.position).normalized; // 플레이어 방향 계산

        if (Mathf.Abs(direction.x) > 0.01f)
            FaceDirection(direction.x > 0 ? 1 : -1);
        // 플레이어 방향으로 이동

        rb.linearVelocity = new Vector2(direction.x * moveSpeed, rb.linearVelocity.y);  // X축으로 이동하고 Y축 속도는 유지
    }

    public void FacetoPlayer()
    {
        if (player == null) return;

        Vector2 direction = (player - transform.position).normalized; // 플레이어 방향 계산

        if (Mathf.Abs(direction.x) > 0.01f)
            FaceDirection(direction.x > 0 ? 1 : -1);
    }
    public void DashToPlayer() // 플레이어한테 대시 추후 제작 예정
    {
        if (player == null) return;

        Vector2 direction = (player - transform.position).normalized;
        rb.AddForce(direction * 7f, ForceMode2D.Impulse);
    }

    public void NAttackDealDamage() //애니메이션 이벤트
    {
        DealAreaDamage(attackPoint.position, attackRangeradius, speciesData.NAttackDamage);
    }

    public void SAttackDealDamage() //애니메이션 이벤트
    {
        DealAreaDamage(attackPoint.position, attackRangeradius, speciesData.SAttackDamage);
    }

    public void DealAreaDamage(Vector2 point, float radius, int damage)
    {
        float finalDamage = damage;
        if (currentLeader != null && !currentLeader.IsDeath)
        {
            finalDamage *= speciesData.leaderBuffDamageMultiplier;
        }

        Collider2D attackCol = attackPoint.GetComponent<Collider2D>();
        Collider2D[] hits = new Collider2D[0];

        // 1. 일단 범위(도형) 안에 있는 모든 적을 가져옴 (기존 로직)
        if (attackCol != null)
        {
            if (attackCol is BoxCollider2D box)
            {
                hits = Physics2D.OverlapBoxAll(box.bounds.center, box.bounds.size, box.transform.eulerAngles.z, playerLayer);
            }
            else if (attackCol is CapsuleCollider2D capsule)
            {
                hits = Physics2D.OverlapCapsuleAll(capsule.bounds.center, capsule.size, capsule.direction, capsule.transform.eulerAngles.z, playerLayer);
            }
            else if (attackCol is CircleCollider2D circle)
            {
                float realRadius = circle.radius * Mathf.Max(circle.transform.lossyScale.x, circle.transform.lossyScale.y);
                hits = Physics2D.OverlapCircleAll(circle.bounds.center, realRadius, playerLayer);
            }
        }
        else
        {
            hits = Physics2D.OverlapCircleAll(point, radius, playerLayer);
        }

        // 2. [추가] 부채꼴 각도 계산 (원형 콜라이더일 때만 적용)
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<PlayerHealth>(out var playerHealth))
            {
                // ★ 부채꼴 판정 로직 ★
                // (공격 범위가 원형이고, 각도가 360도보다 작을 때만 계산)
                if (attackCol is CircleCollider2D && speciesData.attackAngle < 360f)
                {
                    // 몬스터가 보는 방향 (오른쪽: 1, 왼쪽: -1)
                    // (EnemyAI는 Scale.x로 방향을 돌리므로 이를 기준으로 잡습니다)
                    Vector2 facingDir = transform.localScale.x < 0 ? Vector2.left : Vector2.right;

                    // 몬스터 -> 플레이어 방향 벡터
                    Vector2 dirToTarget = (hit.transform.position - transform.position).normalized;

                    // 두 벡터 사이의 각도 계산 (0 ~ 180도)
                    float angleToTarget = Vector2.Angle(facingDir, dirToTarget);

                    // 각도가 설정된 범위(절반)보다 크면 -> 빗나감!
                    if (angleToTarget > speciesData.attackAngle * 0.5f)
                    {
                        continue; // 데미지 안 주고 넘어감
                    }
                }

                // 3. 데미지 적용 (피격)
                float kbForce = Random.Range(minKnockbackForce, maxKnockbackForce);
                float kbUpForce = Random.Range(minKnockbackUpwardForce, maxKnockbackUpwardForce);
                playerHealth.TakeDamage((int)finalDamage);

                // TODO: 넉백 기능은 나중에 구현
                // playerKnockback.ApplyKnockback(transform, kbForce, kbUpForce);
            }
        }
    }
    // 죽음
    public void Die()
    {
        if (speciesData.isLeader)
        {
            Collider2D[] followers = Physics2D.OverlapCircleAll(transform.position, speciesData.commandRadius, enemyLayerMask);
            foreach (var col in followers)
            {
                if (col.gameObject != gameObject && col.TryGetComponent<EnemyAI>(out EnemyAI follower))
                {
                    follower.OnLeaderDied(); // "리더가 죽었다! 도망쳐!"
                }
            }
        }

        ani.Die();
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Static;
        // TODO: 경험치 시스템 추가 시 활성화
        // PlayerExpManager.AddExp(90);

        Destroy(gameObject);
    }
    public void Backstep()
    {
        // 플레이어의 반대 방향으로 이동
        Vector2 dir = -(player - transform.position).normalized;
        rb.AddForce(dir * BackStepForce, ForceMode2D.Impulse);  // 반대 방향으로 힘을 가하여 후퇴
    }

    public void StopMoving()
    {
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        ani.SetMoving(false);
    }

    // IsPlayerInRange() 오버라이드
    public bool IsPlayerInRange()
    {
        //Debug.Log($"isplayerinrange는 {playerInRange}");
        return playerInRange;

    }

    // IsPlayerAttackable
    public bool IsPlayerAttackable()
    {
        if (attackPoint == null || IsDeath)
            return false;

        // attackPoint(GameObject 자식)에 붙은 Collider2D 가져오기
        Collider2D col = attackPoint.GetComponent<Collider2D>();
        if (col == null)
            return false;
        bool isplayerattackable = col.IsTouchingLayers(playerLayer);
        // col 모양 그대로 playerLayer와 겹치는지 체크
        return isplayerattackable;
    }

    // Ground Check 추가 (점프 시 사용)
    public bool HasGroundAbove()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.up, 1f, LayerMask.GetMask("Ground"));
        return hit.collider != null;  // 위쪽에 장애물(땅)이 있으면 true 반환
    }

    public void Jump()
    {
        rb.AddForce(Vector2.up * 5f, ForceMode2D.Impulse);  // 위로 5의 힘을 가하여 점프
    }

    // 공격 준비 (PrepareForAttack)
    public void PrepareForAttack()
    {
        // 현재 속도를 저장하고 서서히 감속
        savedVelocity = rb.linearVelocity;

        // 랜덤 감속률 적용 (예: 0.2~0.6 사이)
        float slowFactor = Random.Range(0.2f, 0.6f);
        rb.linearVelocity = new Vector2(savedVelocity.x * slowFactor, savedVelocity.y);
    }


    // 쿨다운 (StartCooldown)
    public void StartCooldown(float seconds = 3f)
    {
        if (cooldownCoroutine != null)
        {
            StopCoroutine(cooldownCoroutine);  // 이전 쿨다운이 진행 중이면 중지
        }
        cooldownCoroutine = StartCoroutine(CooldownCoroutine(seconds));
    }


    public IEnumerator BusterToPlayer()
    {
        objectactive.StarActivateTarget();
        yield return new WaitForSeconds(BusterChasingTime);
        Vector2 direction = (player - transform.position).normalized;
        if (Mathf.Abs(direction.x) > 0.01f)
            FaceDirection(direction.x > 0 ? 1 : -1);
        ani.SAttack();
        rb.AddForce(new Vector2(direction.x * BusterAttackSpeed, rb.linearVelocityY), ForceMode2D.Impulse);
        cooldown = 5f;
    }

    public void StartBusterCharge()
    {
        StartCoroutine(BusterToPlayer());
    }
    public void Jumpoo()
    {
        rb.AddForce(Vector2.up * 3f, ForceMode2D.Impulse);
    }

    public IEnumerator CooldownCoroutine(float seconds)
    {
        yield return new WaitForSeconds(seconds);  // 쿨다운 대기
        //Debug.Log("Cooldown finished, ready to attack!");
        // 쿨다운 후 공격 가능 상태로 전환하는 로직을 추가 예정
    }

    private void OnDrawGizmosSelected()
    {
        // 1. Awake() 대신 진짜 필요한 데이터 로드 함수만 호출
        if (speciesData == null)
        {
            LoadDataByTag(gameObject.tag);
        }

        // 데이터 로드 실패했거나, 기즈모 보기 설정이 꺼져있으면 즉시 종료
        if (speciesData == null || !showDebugGizmos) return;

        // 플레이어 방향선
        if (_playerTransform != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, _playerTransform.position);
            Gizmos.DrawSphere(_playerTransform.position, 0.05f);
        }

        // 공격 범위
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRangeradius);
        }

        // 2. 완벽하게 추가한 탐지 범위 시각화
        if (speciesData != null)
        {
            // 기본 탐지 범위 (보라색)
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, speciesData.detectionRadius);

            // 추적 상실 범위 (파란색)
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, speciesData.loseDetectionRadius);
        }

        // 낙하 방지 레이
        if (speciesData != null)
        {
            int facing = (transform.localScale.x < 0f) ? 1 : -1;

            Vector3 rayOrigin = new Vector3(
                transform.position.x + (speciesData.horizontalOffset * facing),
                transform.position.y - speciesData.verticalOffset,
                transform.position.z
            );
            Vector2 rayDir = Vector2.down;

            int groundMask = LayerMask.GetMask("Ground");
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, rayDir, speciesData.raycastDistance, groundMask);

            Gizmos.color = hit.collider ? Color.green : Color.red;
            Gizmos.DrawLine(rayOrigin, rayOrigin + (Vector3)(rayDir * speciesData.raycastDistance));
            Gizmos.DrawSphere(rayOrigin, 0.04f);
        }
    }

    public void TakeDamage(float damage) // <-- float으로 변경
    {
        if (IsDeath) return;
        Debug.Log("TakeDamage 호출: 데미지 = " + damage);
        // --- 1. HP 계산 (SpeciesData 연동) ---
        // '방어력' 규칙을 읽어와서 최종 데미지 계산
        float finalDamage = Mathf.Max(1, damage - speciesData.defense); // 최소 1 데미지

        // 데미지를 정수로 변환 (체력은 보통 정수이므로)
        currentHP -= Mathf.RoundToInt(finalDamage);

        // --- 2. 용기 계산 (SpeciesData 연동) 리더 효과와 연동---
        if (currentLeader != null && !currentLeader.IsDeath)
        {

        }
        else
        {
            currentCourage -= speciesData.courageDamagePerHit;
        }
        //healthBarUI.UpdateHealthUI(); 애초에 HealthBarUI는 본인 스스로 업데이트하면서 계속해서 관리하므로 여기서 호출 필요 x
        Debug.Log($"{gameObject.name} | HP: {currentHP}/{speciesData.maxHP} | 용기: {currentCourage} (+{allyCourageBonus}) / {speciesData.maxHP}");
        // --- 3. 상태 변경 (기존 로직) ---
        float effectiveCourage = currentCourage + allyCourageBonus;
        if (speciesData.isLeader && speciesData.useBerserk && !isBerserkMode)
        {
            if (currentHP <= speciesData.maxHP * speciesData.berserkThreshold)
            {
                ActivateBerserk();
            }
        }
        if (effectiveCourage <= 0 && currentState != fleeState)
        {
            ChangeState(fleeState);
        }

        if (currentHP <= 0)
        {
            IsDeath = true;
            Die();
        }
    }

    void LoadDataByTag(string tag)
    {
        // 📌 여기서 경로를 직접 지정 (Assets/Resources/ 이후 경로)
        string speciesDataPath = "SpeciesData";  // ← 이 부분에서 경로 수정
        //Debug.LogError($"[{gameObject.name}] LoadDataByTag() 호출 - 태그: {tag}");

        SpeciesData[] allData = Resources.LoadAll<SpeciesData>(speciesDataPath);
        //Debug.LogWarning($"[{gameObject.name}] 로드된 SpeciesData 총 {allData.Length}개");

        for (int i = 0; i < allData.Length; i++)
        {
            //Debug.LogWarning($"[{gameObject.name}] SpeciesData[{i}]: speciesTag = '{allData[i].speciesTag}'");
            if (allData[i].speciesTag == tag)
            {
                speciesData = allData[i];
                //Debug.Log($"[{gameObject.name}] ✓ '{speciesData.speciesTag}' 데이터 로드 성공!");
                return;
            }
        }

        //Debug.LogError($"[{gameObject.name}] ✗ 실패! 태그 '{tag}'와 매칭되는 SpeciesData가 없습니다!");
    }

    private void UpdateCourageBonus()
    {
        int allyCount = 0;
        this.currentLeader = null;

        // 'allyCheckRadius' 범위 안의 "Enemy" 레이어를 가진 모든 콜라이더를 찾음
        Collider2D[] allies = Physics2D.OverlapCircleAll(transform.position, speciesData.allyCheckRadius, enemyLayerMask);

        foreach (var col in allies)
        {
            // 나 자신은 제외
            if (col.gameObject != this.gameObject)
            {
                allyCount++;

                if (col.TryGetComponent<EnemyAI>(out EnemyAI allyAI))
                {
                    // 리더 찾기
                    if (allyAI.speciesData.isLeader && !allyAI.IsDeath)
                    {
                        this.currentLeader = allyAI;
                    }
                }
            }
        }
        allyCount = Mathf.Min(allyCount, (int)speciesData.MaxPackBonusCount);

        // 보너스 계산
        this.allyCourageBonus = allyCount * speciesData.courageBonusPerAlly;


        float bonusSpeed = allyCount * speciesData.speedBonusPerAlly;
        this.currentMoveSpeed = speciesData.chaseSpeed + bonusSpeed;
        if (currentLeader != null && currentLeader.isBerserkMode) //리더 광폭화 버프 적용
        {
            this.currentMoveSpeed *= currentLeader.speciesData.berserkSpeedMultiplier;
            this.currentCooldown *= 0.5f; //리더가 광폭화 상태면 쿨타임 절반
            Debug.Log($"[리더 광폭화 버프] 속도: {currentMoveSpeed}, 쿨타임: {currentCooldown}");
        }
        float reduction = allyCount * speciesData.cooldownReductionPerAlly;
        this.currentCooldown = Mathf.Max(0.5f, speciesData.attackCooldown - reduction);

        if (allyCount > 0) Debug.Log($"[무리 버프] 속도: {currentMoveSpeed}, 쿨타임: {currentCooldown}");
    }
    // isChainReaction: 남의 소리를 듣고 릴레이로 전파하는 중인지 여부
    public void AlertNearbyAllies(bool isChainReaction = false)
    {
        // 리더가 없는데 연쇄 반응으로 들어온 경보면 전파 차단
        if (isChainReaction && currentLeader == null && !speciesData.isLeader)
        {
            return;
        }
        // 'alertRadius' 범위 안의 "Enemy" 레이어를 가진 모든 콜라이더를 찾음
        Collider2D[] allies = Physics2D.OverlapCircleAll(transform.position, speciesData.alertRadius, enemyLayerMask);

        // 내가 현재 쫓고 있는 플레이어의 위치
        Vector3 playerPos = player;

        foreach (var col in allies)
        {
            // 나 자신은 제외
            if (col.gameObject == gameObject)
                continue;

            // 아군의 'EnemyAI' 컴포넌트를 가져옴
            if (col.TryGetComponent<EnemyAI>(out EnemyAI allyAI))
            {
                // 'ChangeState' 대신, 'Alert' 함수를 호출하여 "타겟 위치"를 전달
                allyAI.Alert(playerPos);
            }
        }
    }
    public void Alert(Vector3 playerPosition)
    {
        // 'IdleState' 타입이 아니면(즉, 이미 싸우거나 도망 중이면) 무시
        if (!(currentState is IdleState))
        {
            return;
        }

        Debug.Log($"[{gameObject.name}]가 경보를 받고 추적 시작!");

        // 2. 나의 'player' 위치를 즉시 갱신
        // (FindWithTag를 기다릴 필요 없이, 전달받은 위치로 타겟 강제 설정)
        this.player = playerPosition;

        // 1. 이미 켜져있는 방패(이전 경보)가 있다면 멈추고 새로 시작
        if (alertShieldCoroutine != null)
        {
            StopCoroutine(alertShieldCoroutine);
        }
        // 2. 3초간 방패를 켜는 코루틴 시작
        alertShieldCoroutine = StartCoroutine(AlertShieldCoroutine(3.0f));

        // 3. 'Chase' 상태로 변경
        ChangeState(chaseState);
    }
    private IEnumerator AlertShieldCoroutine(float duration)
    {
        this.justAlerted = true;  // 방패 켜기 (ChaseState가 참조)
        yield return new WaitForSeconds(duration); // 3초 대기
        this.justAlerted = false; // 방패 내리기
        Debug.Log($"[{gameObject.name}] 경보 방패 해제.");
    }
    public void RecalculateSquadRank()
    {
        if (player == null) return;

        Collider2D[] colliders = Physics2D.OverlapCircleAll(player, speciesData.alertRadius, enemyLayerMask);
        List<EnemyAI> squad = new List<EnemyAI>();

        foreach (var col in colliders)
        {
            if (col.TryGetComponent<EnemyAI>(out EnemyAI ai))
            {
                if (ai.IsDeath || !ai.gameObject.activeInHierarchy) continue;
                squad.Add(ai);
            }
        }

        // 정렬 로직 (떨림 방지 핵심) 
        squad.Sort((a, b) =>
        {
            float distA = Vector2.Distance(a.transform.position, player);
            float distB = Vector2.Distance(b.transform.position, player);

            // 거리가 '비슷하면' (0.5m 이내 차이) -> 거리 무시하고 ID로 고정!
            // (이렇게 해야 같은 줄에 있는 애들끼리 자리를 안 바꿈)
            if (Mathf.Abs(distA - distB) < 2.0f)
            {
                return a.GetInstanceID().CompareTo(b.GetInstanceID());
            }

            // 거리가 확실히 차이 나면 -> 거리순 정렬
            return distA.CompareTo(distB);
        });

        // 내 등수 저장
        mySquadRank = squad.IndexOf(this);
    }
    public Vector3 GetFlankingTargetPos()
    {
        if (player == null) return transform.position;

        // --- [수정] 플레이어의 회전(Facing)을 무시하고, 절대적인 왼쪽/오른쪽 배정 ---

        // 짝수 등수(0, 2, 4...) -> 무조건 플레이어 왼쪽 (-1)
        // 홀수 등수(1, 3, 5...) -> 무조건 플레이어 오른쪽 (+1)
        // (플레이어가 어디를 보든 상관없이 고정된 자리입니다)
        float fixedSide = (mySquadRank % 2 == 0) ? -1f : 1f;

        // 거리 계산 (등수 / 2)
        // 0,1등: 1열 (flankSpacing 거리)
        // 2,3등: 2열 (flankSpacing + 1.5m) ...
        int rowNumber = mySquadRank / 2;
        float finalSpacing = speciesData.flankSpacing + (rowNumber * 0.5f); // 1.5f는 줄 간격

        // 목표 위치 계산
        // 플레이어 위치 + (방향 * 거리)
        Vector3 targetPos = new Vector3(player.x + (fixedSide * finalSpacing), player.y, player.z);

        return targetPos;
    }
    //협동 전술용 플레이어 통과 설정
    // EnemyAI.cs 수정

    public void SetPassThroughPlayer(bool enablePassThrough)
    {
        // 1. 내 몸에 붙은 모든 콜라이더 가져오기
        Collider2D[] myColliders = GetComponentsInChildren<Collider2D>();

        // 2. 플레이어 몸에 붙은 모든 콜라이더 가져오기
        Collider2D[] playerColliders = null;
        if (PlayerTransform != null)
        {
            playerColliders = PlayerTransform.GetComponentsInChildren<Collider2D>();
        }

        if (myColliders != null && playerColliders != null)
        {
            foreach (var myCol in myColliders)
            {
                foreach (var pCol in playerColliders)
                {
                    // 핵심: 여기서는 플레이어와의 충돌만 건드립니다.
                    Physics2D.IgnoreCollision(myCol, pCol, enablePassThrough);
                }
            }
        }
    }
    public void SetGhostMode(bool enable)
    {
        // 1. 중복 호출 방지
        if (isMyGhostActive == enable) return;
        isMyGhostActive = enable;

        int enemyLayer = LayerMask.NameToLayer("Enemy");

        if (enable)
        {
            globalGhostCount++;
            // 1명이라도 유령이면 -> 적끼리 충돌 끔
            if (globalGhostCount > 0 && enemyLayer != -1)
                Physics2D.IgnoreLayerCollision(enemyLayer, enemyLayer, true);
        }
        else
        {
            globalGhostCount--;
            if (globalGhostCount < 0) globalGhostCount = 0;

            // 아무도 유령이 아니면 -> 적끼리 충돌 다시 켬
            if (globalGhostCount == 0 && enemyLayer != -1)
                Physics2D.IgnoreLayerCollision(enemyLayer, enemyLayer, false);
        }

        // (플레이어 충돌 제어 코드는 삭제함 -> SetPassThroughPlayer가 전담)
    }
    public void SetSearchGhostMode(bool isGhost)
    {
        // 기능이 똑같으므로 통합된 함수를 호출
        SetPassThroughPlayer(isGhost);
    }
    // --- 협동 전술 용 특정 지점으로 이동하는 함수 (범용 이동) ---
    public void MoveToTarget(Vector3 targetPos)
    {
        // 목표 지점 방향 계산
        Vector2 direction = (targetPos - transform.position).normalized;

        // 방향 전환 (좌우)
        if (Mathf.Abs(direction.x) > 0.01f)
            FaceDirection(direction.x > 0 ? 1 : -1);

        // 이동 (현재 계산된 속도 사용)
        rb.linearVelocity = new Vector2(direction.x * currentMoveSpeed, rb.linearVelocity.y);
    }

    public void OnLeaderDied() //리더 사망시 호출
    {
        if (IsDeath) return;

        currentLeader = null; // 리더 참조 제거
        currentCourage = 0;   // 용기 바닥남

        Debug.Log($"[{gameObject.name}] 리더 사망! 공포에 질려 도망칩니다!");
        ChangeState(fleeState); // 즉시 도주 상태로 전환
    }
    private void ActivateBerserk() //광폭화 발동
    {
        isBerserkMode = true;
        Debug.Log($"<color=red>[Leader] {gameObject.name} 광폭화 발동!! 모두 돌격!!</color>");

        // 1. 나 자신의 스펙 강화 (예: 체력 조금 회복 or 무적 등)
        // currentCourage = speciesData.maxCourage * 2; // 용기 풀충전

        // 2. 주변 모든 부하들에게 광폭화 명령
        Collider2D[] followers = Physics2D.OverlapCircleAll(transform.position, speciesData.commandRadius, enemyLayerMask);
        foreach (var col in followers)
        {
            if (col.TryGetComponent<EnemyAI>(out EnemyAI minion))
            {
                // 부하들의 속도를 강제로 올림 (UpdateCourageBonus에서 덮어씌워질 수 있으므로, 
                // 아예 BerserkMode 변수를 부하도 갖게 하거나, 버프 수치를 조작해야 함)

                // 여기서는 간단하게 '용기 보너스' 함수에 영향을 주는 방식으로 구현 추천
                // 혹은 즉발적인 효과 부여:
                minion.GetAnimator().speed = 2.0f; // 애니메이션 속도 2배
                // (제대로 하려면 EnemyAI에 'berserkBuffMultiplier' 변수를 두고 그걸 적용해야 함)
            }
        }
    }
    void DebugLogLoadedData()
    {
        if (speciesData == null)
        {
            Debug.LogError(gameObject.name + ": DebugLogLoadedData() 호출 실패. speciesData가 null입니다.");
            return;
        }

        // C# 6.0 이상에서 지원하는 문자열 보간($)을 사용해 깔끔하게 출력
        string logMessage = $"--- [{gameObject.name}]가 로드한 데이터 ({speciesData.name}) --- \n" +
                            $"1. 식별: Tag = {speciesData.speciesTag}\n" +
                            $"2. 열망: MaxCourage = {speciesData.maxCourage}, Aggression = {speciesData.aggression}\n" +
                            $"3. 기본: MaxHP = {speciesData.maxHP}, AttackRange = {speciesData.attackRange}, NAttack = {speciesData.NAttackDamage}, SAttack = {speciesData.SAttackDamage}, Cooldown = {speciesData.attackCooldown}\n" +
                            $"4. 이동: PatrolSpeed = {speciesData.patrolSpeed}, ChaseSpeed = {speciesData.chaseSpeed}\n" +
                            $"5. 탐지: Detection = {speciesData.detectionRadius}, Raycast = {speciesData.raycastDistance}, H_Offset = {speciesData.horizontalOffset}, V_Offset = {speciesData.verticalOffset}\n" +
                            $"6. 피격: Defense = {speciesData.defense}, CourageDmg = {speciesData.courageDamagePerHit}\n" +
                            $"7. 공격: Pattern = {speciesData.attackType}\n" +
                            //$"8. 특수: BackStep = {speciesData.backStepForce}, BusterTime = {speciesData.busterChasingTime}, BusterSpeed = {speciesData.busterAttackSpeed}\n" +
                            $"9. UI: Offset = {speciesData.healthBarOffset}\n" +
                            $"------------------------------------------------------";

        Debug.Log(logMessage);
    }


    // <summary>
    /// 순찰용 절벽 감지 레이캐스트를 씬(Scene) 뷰에 시각화합니다.
    /// </summary>
    private void OnDrawGizmos()
    {
        // 1. speciesData가 없으면 그리지 않음
        if (speciesData == null || !showDebugGizmos) return;

        // --- 2. 경보 범위 (Alert Radius) 시각화 ---
        // '무리 본능' 헤더가 있다고 가정
        if (speciesData.alertRadius > 0)
        {
            Gizmos.color = new Color(1f, 0.92f, 0.016f, 0.15f); // (반투명 노란색)
            Gizmos.DrawSphere(transform.position, speciesData.alertRadius);
        }
        // 3. 현재 방향 (오른쪽: 1, 왼쪽: -1)
        // (IdleState의 moveDir을 직접 알 수 없으므로, 현재 바라보는 방향을 사용)
        int moveDir = (transform.localScale.x < 0f) ? -1 : 1;

        // 4. 레이캐스트 시작점 계산
        Vector2 rayOrigin = new Vector2(
            transform.position.x + (speciesData.horizontalOffset * moveDir),
            transform.position.y - speciesData.verticalOffset
        );

        // 5. 레이캐스트 방향 및 길이
        Vector2 rayDirection = Vector2.down;
        float distance = speciesData.raycastDistance;

        // 6. 레이캐스트가 실제로 땅을 감지하는지 테스트
        int groundMask = LayerMask.GetMask("Ground", "FloatGround");
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, rayDirection, distance, groundMask);

        // 7. 씬 뷰에 그리기 (감지되면 초록색, 안되면 빨간색)
        if (hit.collider != null)
        {
            Gizmos.color = Color.green; // 땅 감지 성공!
        }
        else
        {
            Gizmos.color = Color.red; // 땅 감지 실패!
        }

        Gizmos.DrawLine(rayOrigin, rayOrigin + (rayDirection * distance));
    }
    #region IHealth 구현
    // UI가 이 값을 읽어갑니다
    float IHealth.currentHP => this.currentHP;
    float IHealth.maxHP => (speciesData != null) ? this.speciesData.maxHP : 100; // (널 참조 방지)
    #endregion
}

