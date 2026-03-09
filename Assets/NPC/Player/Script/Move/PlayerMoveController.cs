using UnityEngine;
using System.Collections;

public class PlayerMoveController : MonoBehaviour
{
    [Header("이동 & 액션")]
    [SerializeField] public float speed = 3f;
    [SerializeField] private float jumpForce = 7.5f;

    [Header("물리 설정")]
    [Tooltip("하강 시에만 적용되는 중력 가속도 배율")]
    [SerializeField] private float _fallMultiplier = 2.5f;
    [Tooltip("점프 버튼을 누르고 있을 때의 상승 중력 배율 (느린 상승 개선)")]
    [SerializeField] private float _ascentMultiplier = 2.5f;

    [Header("하향 점프 설정 및 점프 충돌 무시")]
    [Tooltip("플랫폼과의 충돌을 무시할 시간 (하향 점프)")]
    [SerializeField] private float platformDropTime = 0.5f;
    [Tooltip("플랫폼과의 충돌을 무시할 시간 (점프 스루)")]
    [SerializeField] private float jumpThroughTime = 0.3f;

    [Header("공격 이동 잠금")]
    [SerializeField] private float attackLockExtraDrag = 1000f;

    [Header("점프 버그 방지")]
    [Tooltip("점프 직후, 점프 카운트 초기화를 막을 시간")]
    [SerializeField] private float _jumpResetCooldown = 0.2f;
    private float lastJumpTime = -99f;

    private bool attackLocked = false;
    private float defaultDrag;

    public int GetFacingDir => facingDir;

    // ===== 상태 플래그 =====
    [SerializeField] public int jumpCount = 0;
    private int facingDir = 1;
    private const int maxJumps = 2;
    private float moveInput;

    public float dashDuration = 0.25f;
    public float dashSpeed = 3f;
    public float dashCooldown = 5.0f;
    public bool isjump = false;

    // [수정] 사용하지 않는 변수 제거 (CS0414 경고 해결)
    // private Coroutine _animationSlowRoutine;
    
    private Coroutine _stepRoutine;
    private Coroutine _platformIgnoreRoutine;

    // ===== 내부에서 사용할 참조 =====
    private PlayerRef _ref;

    private void Awake()
    {
        _ref = GetComponent<PlayerRef>();
        if (_ref._Rb != null)
            defaultDrag = _ref._Rb.linearDamping;
    }

    public void Update()
    {
        // 🚀 [수정(우현)] 안전장치 추가
        if (Time.timeScale == 0 || (_ref._Health != null && _ref._Health.isDead)) return;

        HandleInput();
    }

    private void FixedUpdate()
    {
        if (_ref._Rb == null) return;

        float gravityScale = _ref._Rb.gravityScale;
        float defaultGravity = Physics2D.gravity.y * gravityScale;
        float extraGravityForce = 0f;

        if (_ref._Rb.linearVelocity.y < 0f)
        {
            extraGravityForce = defaultGravity * (_fallMultiplier - 1f);
        }
        else if (_ref._Rb.linearVelocity.y > 0f)
        {
            // 🚀 [수정(우현)] KeyManager 사용
            if (Input.GetKey(KeyManager.Instance.KeyJump)) // KeyCode.Space -> KeyJump
            {
                extraGravityForce = defaultGravity * (_ascentMultiplier - 1f);
            }
        }

        if (extraGravityForce != 0f)
        {
            _ref._Rb.linearVelocity += Vector2.up * extraGravityForce * Time.fixedDeltaTime;
        }

        if (attackLocked)
        {
            _ref._Rb.linearVelocity = new Vector2(0f, _ref._Rb.linearVelocity.y);
            return;
        }

        // [수정] DashSkill에 IsDashing 프로퍼티가 추가되었으므로 접근 가능
        if (_ref._Dash != null && _ref._Dash.IsDashing)
        {
            return;
        }

        HandleMovement();
    }

    public void HandleInput()
    {
        moveInput = 0f;
        // 🚀 [수정(우현)] KeyManager 사용 (좌우 이동)
        if (Input.GetKey(KeyManager.Instance.KeyRight)) moveInput = 1f;
        else if (Input.GetKey(KeyManager.Instance.KeyLeft)) moveInput = -1f;

        // 🚀 [수정(우현)] 하향 점프 키 조합 변경
        bool isDownJumpInput = Input.GetKey(KeyManager.Instance.KeyDown) && Input.GetKeyDown(KeyManager.Instance.KeyJump);

        // 대쉬 입력 (KeyCode.D -> KeyDash)
        // 🚀 [수정(우현)] KeyManager 사용
        if (_ref._Dash != null && (Input.GetKeyDown(KeyManager.Instance.KeyDash)))
        {
            if (!_ref._Dash.IsDashing)
            {
                // [수정] 여기서 직접 방향을 계산하지 않고 DashSkill이 알아서 가져가도록 변경
                // (기존 코드의 방향 계산 로직은 DashSkill 내부로 이동됨)

                if (_platformIgnoreRoutine != null)
                {
                    StopCoroutine(_platformIgnoreRoutine);
                    _platformIgnoreRoutine = null;

                    if (_ref._Ground != null)
                    {
                        int floatGroundLayer = LayerMask.NameToLayer(_ref._Ground.FloatGroundLayerName);
                        if (floatGroundLayer >= 0)
                        {
                            Physics2D.IgnoreLayerCollision(gameObject.layer, floatGroundLayer, false);
                            _ref._Ground.ResumeFloatGroundLayer();
                        }
                    }
                }

                // 🚀 [수정] 인자 없이 호출 (에러 해결)
                _ref._Dash.TryDash(); 
                return;
            }
        }

        if (isDownJumpInput && _ref._Ground != null && _ref._Ground.isGrounded && _ref._Ground.isOnFloatGround)
        {
            int floatGroundLayer = LayerMask.NameToLayer(_ref._Ground.FloatGroundLayerName);

            if (floatGroundLayer >= 0)
            {
                StartPlatformIgnore(floatGroundLayer, platformDropTime);
                return;
            }
        }

        // 🚀 [수정(우현)] KeyManager 사용 (점프)
        if (Input.GetKeyDown(KeyManager.Instance.KeyJump) && jumpCount < maxJumps)
        {
            {
                if (_ref._AnimSync != null) _ref._AnimSync.Jump();
                if (_ref._Rb != null) _ref._Rb.linearVelocity = new Vector2(_ref._Rb.linearVelocity.x, jumpForce);

                if (_ref._Ground != null) _ref._Ground.isGrounded = false;
                isjump = true;
                jumpCount++;

                lastJumpTime = Time.time;

                if (_ref._Ground != null && _ref._Ground.FloatGroundLayerName != null)
                {
                    int floatGroundLayer = LayerMask.NameToLayer(_ref._Ground.FloatGroundLayerName);
                    if (floatGroundLayer >= 0)
                    {
                        StartPlatformIgnore(floatGroundLayer, jumpThroughTime);
                    }
                }

                StartCoroutine(JumpRoutine());
            }
        }
    }

    public void HandleMovement()
    {
        if (_ref._Dash != null && _ref._Dash.IsDashing) return;

        if (_ref._Rb != null) _ref._Rb.linearVelocity = new Vector2(moveInput * speed, _ref._Rb.linearVelocity.y);

        if (moveInput != 0f)
        {
            facingDir = (int)Mathf.Sign(moveInput);
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * facingDir;
            transform.localScale = scale;
        }

        if (_ref._AnimSync != null)
        {
            _ref._AnimSync.AirSpeedY(_ref._Rb.linearVelocity.y);
            _ref._AnimSync.IsWalking(moveInput != 0f);
        }
    }

    private void StartPlatformIgnore(int layerToIgnore, float duration)
    {
        Physics2D.IgnoreLayerCollision(gameObject.layer, layerToIgnore, true);

        if (_ref._Ground != null)
        {
            _ref._Ground.IgnoreFloatGroundLayer();
        }

        if (_platformIgnoreRoutine != null)
        {
            StopCoroutine(_platformIgnoreRoutine);
        }
        _platformIgnoreRoutine = StartCoroutine(PlatformDropResetRoutine(layerToIgnore, duration));

        float colliderHeight = _ref._Col != null ? _ref._Col.bounds.extents.y : 0.5f;
        float pushUpAmount = colliderHeight + 0.05f;

        if (_ref._Rb != null && _ref._Rb.linearVelocity.y > 0)
        {
            transform.position += new Vector3(0f, pushUpAmount, 0f);
        }
    }

    public void ResetJumpCount(bool ignoreCooldown = false)
    {
        if (!ignoreCooldown && Time.time < lastJumpTime + _jumpResetCooldown)
        {
            return;
        }

        jumpCount = 0;
    }

    public void SetAttackLock(bool locked)
    {
        attackLocked = locked;
        if (_ref._Rb == null) return;

        if (locked)
        {
            _ref._Rb.linearVelocity = new Vector2(0f, _ref._Rb.linearVelocity.y);
            _ref._Rb.linearDamping = attackLockExtraDrag;
        }
        else
        {
            _ref._Rb.linearDamping = defaultDrag;
        }
    }

    public void StepForward(float distance, float duration, AnimationCurve curve = null)
    {
        if (_stepRoutine != null) StopCoroutine(_stepRoutine);
        _stepRoutine = StartCoroutine(StepRoutine(distance, duration, curve));
    }

    private IEnumerator StepRoutine(float distance, float duration, AnimationCurve curve)
    {
        float signedDistance = distance * Mathf.Sign(GetFacingDir);
        Vector2 start = _ref._Rb.position;
        Vector2 target = start + new Vector2(signedDistance, 0f);

        float t = 0f;
        while (t < duration)
        {
            t += Time.fixedDeltaTime;
            float lerp = duration > 0f ? Mathf.Clamp01(t / duration) : 1f;
            float eased = (curve != null) ? curve.Evaluate(lerp) : lerp;
            Vector2 next = Vector2.Lerp(start, target, eased);
            if (_ref._Rb != null) _ref._Rb.MovePosition(next);
            yield return new WaitForFixedUpdate();
        }

        if (_ref._Rb != null) _ref._Rb.MovePosition(target);
        _stepRoutine = null;
    }

    public IEnumerator JumpRoutine()
    {
        // 🚀 [수정(우현)] 점프 멈춤 방지용 안전장치
        yield return new WaitForSeconds(0.1f);
        isjump = false;
    }

    private IEnumerator PlatformDropResetRoutine(int layerToIgnore, float duration)
    {
        yield return new WaitForSeconds(duration);

        Physics2D.IgnoreLayerCollision(gameObject.layer, layerToIgnore, false);

        if (_ref._Ground != null)
        {
            _ref._Ground.ResumeFloatGroundLayer();
        }

        _platformIgnoreRoutine = null;
    }

    // 🚀 [수정(우현)] KeyManager 기반 입력 벡터 반환 함수
    public Vector2 GetInputVector()
    {
        float h = 0f;
        if (Input.GetKey(KeyManager.Instance.KeyRight)) h = 1f;
        else if (Input.GetKey(KeyManager.Instance.KeyLeft)) h = -1f;

        float v = 0f;
        if (Input.GetKey(KeyManager.Instance.KeyUp)) v = 1f;
        else if (Input.GetKey(KeyManager.Instance.KeyDown)) v = -1f;

        return new Vector2(h, v);
    }
}