using System.Xml.Serialization;
using UnityEngine;
using System.Collections;


public class Player_move : MonoBehaviour
{

    [Header("이동 & 액션")]
    [SerializeField] public float speed = 3f;               // 걷기 속도
    [SerializeField] private float jumpForce = 7f;           // 점프 힘

    [Header("공격 이동 잠금")]
    [SerializeField] private float attackLockExtraDrag = 8f;

    private bool attackLocked = false;
    private float defaultDrag;

    // 외부에서 바라보는 방향 필요하니 공개 Getter 추가
    public int FacingDir => facingDir;    

    // ===== 상태 플래그 =====
    [SerializeField] public int jumpCount = 0;
    private int facingDir = 1;
    public static float speedUP = 1;
    private const int maxJumps = 2;
    private float moveInput;
    public float dashDuration = 0.25f;
    public float dashSpeed = 3f;
    public float dashCooldown = 5.0f; // 쿨타임 설정
    private Vector2 dashInputDir;
    public bool isjump = false;
    
    public DashSkill Dash => dash;

    // “스탯”이 주는 기본 이동속도 (예: 레벨, 스텟 포인트)
    [SerializeField] private float statMoveSpeed = 3f;

    // 장비/아이템이 주는 보너스 (예: 신발, 반지 등)
    private float equipmentSpeedBonus = 0f;

    // 버프(가속) 멀티플라이어
    private float buffSpeedMultiplier = 1f;

    // 디버프(감속) 멀티플라이어
    private float debuffSpeedMultiplier = 1f;

    public float finalSpeed = 0f;

    public float CurrentMoveSpeed => CalculateCurrentSpeed();

    // 디버프 복구 코루틴 레퍼런스
    private Coroutine _speedRecoverRoutine;
    private Coroutine _animationSlowRoutine;
    private Coroutine _stepRoutine;

    // ===== 내부에서 사용할 참조 =====
    private PlayerAnimationSync sync;
    private Rigidbody2D rb;
    public DashSkill dash;
    private Ground ground;
    private PlayerHealth health;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sync = GetComponentInParent<PlayerAnimationSync>();
        dash = GetComponent<DashSkill>();
        ground = GetComponentInParent<Ground>();
        health = GetComponentInParent<PlayerHealth>();
        defaultDrag = rb.linearDamping;
    }
    private void Update()
    {
        //Debug.Log(rb.linearVelocity);
        HandleMovementAndJump();
    }

    private void FixedUpdate()
    {
        // 공격 잠금 상태면 x속도 강제 0(미끄러짐 방지)
        if (attackLocked)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        if (!dash.IsDashing && !health.isknockback)
        {
            finalSpeed = CalculateCurrentSpeed();
            rb.linearVelocity = new Vector2(moveInput * finalSpeed, rb.linearVelocity.y);
        }
    }

    private void HandleMovementAndJump()
    {
        // 이동 방향 입력 (좌우 이동용)
        moveInput = 0f;
        if (Input.GetKey(KeyCode.RightArrow)) moveInput = 1f;
        else if (Input.GetKey(KeyCode.LeftArrow)) moveInput = -1f;

        // 대쉬 방향 입력 (8방향)
        dashInputDir = Vector2.zero;

        if (Input.GetKey(KeyCode.UpArrow)) dashInputDir.y += 1f;
        if (Input.GetKey(KeyCode.DownArrow)) dashInputDir.y -= 1f;
        if (Input.GetKey(KeyCode.LeftArrow)) dashInputDir.x -= 1f;
        if (Input.GetKey(KeyCode.RightArrow)) dashInputDir.x += 1f;

        dashInputDir = dashInputDir.normalized;

        if (Input.GetKeyDown(KeyCode.D))
        {
            //dash.TryDash(dashInputDir, rb, animController);
            dash.TryDash(dashInputDir, rb);
            //StartCoroutine(IgnoreCollider(0.2f));
        }

        // 캐릭터 바라보는 방향 업데이트
        if (moveInput != 0f)
        {
            facingDir = (int)Mathf.Sign(moveInput);
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * facingDir;
            transform.localScale = scale;
        }

        // 애니메이션 처리
        sync.AirSpeedY(rb.linearVelocity.y);
        sync.IsWalking(moveInput != 0f);

        // 점프
        //if (Input.GetKeyDown(KeyCode.Space) && jumpCount < maxJumps)
        //{
        //    sync.Jump();
        //    rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        //    ground.isGroundedNow = false;
        //    isjump = true;
        //    jumpCount++;
        //    StartCoroutine(Jump());
        //    StartCoroutine(JumpMooshi(0.5f));
        //}
        if (Input.GetKeyDown(KeyCode.Space) && jumpCount < maxJumps)
        {
            if (Input.GetKey(KeyCode.DownArrow) && Input.GetKeyDown(KeyCode.Space) && ground.floatGround)
            {
                //StartCoroutine(IgnoreCollider(0.5f));
            }
            else
            {
                Debug.Log("kkkkkk");
                sync.Jump();
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                ground.isGroundedNow = false;
                isjump = true;
                jumpCount++;
                StartCoroutine(Jump());
                //StartCoroutine(IgnoreCollider(0.4f));
            }

        }
    }
    public bool IsInvincible()
    {
        return dash.IsInvincible;
    }

    public void ResetJumpCount()
    {
        jumpCount = 0;
        //Debug.Log("Player_move: 점프 카운트 초기화");
    }
    private float CalculateCurrentSpeed()
    {
        float basePlusEquip = speed + statMoveSpeed + equipmentSpeedBonus;
        return basePlusEquip * buffSpeedMultiplier * debuffSpeedMultiplier * speedUP;
    }

    /// <summary>
    /// 현재 재생 중인 애니메이션이 끝날 때까지 속도를 느리게 유지합니다.
    /// </summary>
    public void AttackSpeedDownDuringAnimation(float slowRatio = 0.1f)
    {
        // 이미 실행 중인 코루틴이 있으면 중단
        if (_animationSlowRoutine != null)
            StopCoroutine(_animationSlowRoutine);

        // 속도 느려짐 적용
        debuffSpeedMultiplier = slowRatio;

        // 지금 재생 중인 애니메이션 State의 해시 저장
        AnimatorStateInfo stateInfo = sync.CurrentStateInfo;
        int currentStateHash = stateInfo.shortNameHash;

        // 해당 State가 끝날 때까지 복구 코루틴 실행
        _animationSlowRoutine = StartCoroutine(RecoverAfterAnimationEnd(currentStateHash));
    }

    public void SetAttackLock(bool locked)
    {
        attackLocked = locked;
        if (locked)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            rb.linearDamping = attackLockExtraDrag;   // 바닥에서 미끄러짐 억제
        }
        else
        {
            rb.linearDamping = defaultDrag;
        }
    }

    public void StepForward(float distance, float duration, AnimationCurve curve = null)
    {
        if (_stepRoutine != null) StopCoroutine(_stepRoutine);
        _stepRoutine = StartCoroutine(StepRoutine(distance, duration, curve));
    }

    private IEnumerator StepRoutine(float distance, float duration, AnimationCurve curve)
    {
        // 바라보는 방향 반영
        float signedDistance = distance * Mathf.Sign(FacingDir);
        Vector2 start = rb.position;
        Vector2 target = start + new Vector2(signedDistance, 0f);

        float t = 0f;
        while (t < duration)
        {
            t += Time.fixedDeltaTime;
            float lerp = duration > 0f ? Mathf.Clamp01(t / duration) : 1f;
            float eased = (curve != null) ? curve.Evaluate(lerp) : lerp;
            Vector2 next = Vector2.Lerp(start, target, eased);
            rb.MovePosition(next);
            yield return new WaitForFixedUpdate();
        }

        rb.MovePosition(target);
        _stepRoutine = null;
    }

    private IEnumerator RecoverAfterAnimationEnd(int stateHash)
    {
        // 첫 프레임 건너뛰기
        yield return null;

        // 같은 State가 끝나지 않았으면 계속 대기
        while (sync.CurrentStateInfo.shortNameHash == stateHash
               && sync.CurrentStateInfo.normalizedTime < 1f)
        {
            yield return null;
        }
        yield return new WaitForSeconds(0.5f);
        // 애니메이션 종료 시점에 속도 복구
        debuffSpeedMultiplier = 1f;
        _animationSlowRoutine = null;
    }

    private IEnumerator Jump()
    {
        yield return null;
        isjump = false;
    }

    private IEnumerator IgnoreCollider(float ignoretime)
    {
        CircleCollider2D parentCollider = GetComponentInParent<CircleCollider2D>();
        parentCollider.enabled = false;
        yield return new WaitForSeconds(ignoretime);
        parentCollider.enabled = true;
    }
}



