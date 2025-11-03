using UnityEngine;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    public IEnemyState currentState;
    public IEnemyState idleState, chaseState, attackState; //모드 변수 선언.
    public bool hasDealtDamage = false;

    // 트리거 기반 감지 처리
    public bool playerInRange = false;
    public bool playerAttackable = false;
    public float attackRange = 2f;

    //플레이어
    public Vector3 player;
    private Transform playerT;
    private Transform _playerTransform;
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

    public float moveSpeed = 1f;
    private Vector2 savedVelocity;

    [Header("공격")]
    [SerializeField] public LayerMask playerLayer; // 공격 대상이 될 레이어
    public float attackRangeradius = 2f;
    public Transform attackPoint;
    public float AttackTimer = 0f;
    public float cooldown = 1.5f;
    public bool canAttack = false;
    [Tooltip("공격 데미지")]
    public int NattackDamage = 20;
    public int SattackDamage = 40;

    [Header("체력")]
    [SerializeField] public int maxHP = 100;

    [Header("백스텝")]
    [SerializeField] private float BackStepForce = 1f;

    [Header("넉백 범위 설정")]
    [SerializeField] private float minKnockbackForce = 0.2f;
    [SerializeField] private float maxKnockbackForce = 0.4f;
    [SerializeField] private float minKnockbackUpwardForce = 0.2f;
    [SerializeField] private float maxKnockbackUpwardForce = 0.4f;

    public bool IsHit { get; protected set; } = false;

    [Header("드워프버스터전용")]
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

    [Header("낙하 방지")]
    [SerializeField] private float raycastDistance = 5f;
    [SerializeField] private float horizontalOffset = 0.5f;
    [SerializeField] private float verticalOffset = 0.1f;

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
        ani = GetComponent<MonsterAnimatorController>();
        rb = GetComponent<Rigidbody2D>();
        idleState = new IdleState(this, ani);
        chaseState = new ChaseState(this, ani);
        attackState = new AttackState(this, ani);
        detection = GetComponent<DetectionArea>();
        objectactive = GetComponent<ObjectActive>();
        dwarfbuster = GetComponent<DwarfBusterBullet>();
    }


    void Start()
    {
        if (CompareTag("Wolf"))
        {
            raycastDistance = 0.35f;
            horizontalOffset = 0.8f;
            verticalOffset = 0.5f;
        }
        else if (CompareTag("Dwarf"))
        {
            raycastDistance = 0.3f;
            horizontalOffset = 0.75f;
            verticalOffset = 0;
        }
        else if (CompareTag("Dwarf_Hammer"))
        {
            raycastDistance = 0.3f;
            horizontalOffset = 0.75f;
            verticalOffset = 0;
        }
        else if (CompareTag("DwarfBuster"))
        {
            raycastDistance = 0.3f;
            horizontalOffset = 1.1f;
            verticalOffset = 0;
        }
        else if (CompareTag("Goblin"))
        {
            raycastDistance = 0.3f;
            horizontalOffset = 0.45f;
            verticalOffset = 1.5f;
        }
        else
        {
            raycastDistance = 2f;
        }
        currentHP = maxHP;
        ChangeState(idleState);
        attacktimer = 0f;
    }

    private void Update()
    {
        IdleState.raycastDistance = raycastDistance;
        IdleState.horizontalOffset = horizontalOffset;
        IdleState.verticalOffset = verticalOffset;
        //Debug.Log(raycastDistance);
        if (PlayerTransform == null)
            return; // 아직 할당 안 됐으면 건너뛰기
        if (PlayerTransform == null)
            return; // 아직 할당 안 됐으면 건너뛰기

        // 위치 계산
        player = PlayerTransform.position;
        if (IsDeath) return;
        currentState?.Update();
    }

    void FixedUpdate()
    {
        attacktimer += Time.deltaTime;
        //Debug.Log(currentState);
        if (attacktimer >= cooldown)
        {
            attacktimer = 0f;
            canAttack = true;
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
        DealAreaDamage(attackPoint.position, attackRangeradius, NattackDamage);
    }

    public void SAttackDealDamage() //애니메이션 이벤트
    {
        DealAreaDamage(attackPoint.position, attackRangeradius, SattackDamage);
    }

    public void DealAreaDamage(Vector2 attackPoint, float radius, int damage)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint, radius, playerLayer);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<PlayerHealth>(out var playerHealth))
            {
                float kbForce = Random.Range(minKnockbackForce, maxKnockbackForce);
                float kbUpForce = Random.Range(minKnockbackUpwardForce, maxKnockbackUpwardForce);
                playerHealth.TakeDamage(damage, transform, kbForce, kbUpForce);
                //Debug.Log($"피격! 넉백 힘: {kbForce:F2}, 상승 힘: {kbUpForce:F2}");
            }
        }
    }
    // 죽음
    public void Die()
    {
        ani.Die();
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Static;
        PlayerExpManager.AddExp(90);

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

        // DetectionArea 범위 (자식 CircleCollider2D 기준)
        var det = transform.Find("DetectionArea");
        if (det != null)
        {
            var cc = det.GetComponent<CircleCollider2D>();
            if (cc != null)
            {
                Gizmos.color = new Color(1f, 0.5f, 0f, 0.8f);
                Vector3 c = cc.transform.TransformPoint(cc.offset);
                Gizmos.DrawWireSphere(c, cc.radius);
            }
        }

        // 낙하 방지 레이
        int facing = (transform.localScale.x < 0f) ? 1 : -1;

        Vector3 rayOrigin = new Vector3(
            transform.position.x + (horizontalOffset * facing),
            transform.position.y - verticalOffset,
            transform.position.z
        );
        Vector2 rayDir = Vector2.down;

        int groundMask = LayerMask.GetMask("Ground");
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, rayDir, raycastDistance, groundMask);

        Gizmos.color = hit.collider ? Color.green : Color.red;
        Gizmos.DrawLine(rayOrigin, rayOrigin + (Vector3)(rayDir * raycastDistance));
        Gizmos.DrawSphere(rayOrigin, 0.04f);
    }

}
