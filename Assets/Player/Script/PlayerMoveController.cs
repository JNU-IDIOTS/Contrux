using UnityEngine;
using System.Collections;
using System.Xml.Serialization;

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
    private Vector2 dashInputDir;
    public bool isjump = false;

    private Coroutine _animationSlowRoutine;
    private Coroutine _stepRoutine;
    private Coroutine _platformIgnoreRoutine;

    // ===== 내부에서 사용할 참조 =====
    // 🚨 [변경] 개별 참조 변수 제거 -> PlayerRef 사용
    private PlayerRef _ref;

    private void Awake()
    {
        // 🚨 [변경] PlayerRef 할당
        _ref = GetComponent<PlayerRef>();
        // 🚨 [변경] 초기값 할당 (_rb -> _ref._Rb)
        if (_ref._Rb != null)
            defaultDrag = _ref._Rb.linearDamping;
    }

    public void Update()
    {
        HandleInput();
    }

    private void FixedUpdate()
    {
        // 🚨 [변경] _rb -> _ref._Rb
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
            if (Input.GetKey(KeyCode.Space))
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

        // 🚨 [변경] _dashSkill -> _ref._Dash
        if (_ref._Dash != null && _ref._Dash.IsDashing)
        {
            return;
        }

        HandleMovement();
    }

    public void HandleInput()
    {
        moveInput = 0f;
        if (Input.GetKey(KeyCode.RightArrow)) moveInput = 1f;
        else if (Input.GetKey(KeyCode.LeftArrow)) moveInput = -1f;

        bool isDownJumpInput = Input.GetKey(KeyCode.DownArrow) && Input.GetKeyDown(KeyCode.Space);

        // 🚨 [변경] _dashSkill -> _ref._Dash
        if (_ref._Dash != null && (Input.GetKeyDown(KeyCode.D)))
        {
            if (!_ref._Dash.IsDashing)
            {
                Vector2 dashDir;
                if (moveInput != 0f) dashDir = new Vector2(moveInput, 0f);
                else dashDir = new Vector2(GetFacingDir, 0f);

                if (_platformIgnoreRoutine != null)
                {
                    StopCoroutine(_platformIgnoreRoutine);
                    _platformIgnoreRoutine = null;

                    // 🚨 [변경] _ground -> _ref._Ground
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

                // 🚨 [변경] _rb -> _ref._Rb
                _ref._Dash.TryDash(dashDir, _ref._Rb);
                return;
            }
        }

        // 🚨 [변경] _ground -> _ref._Ground
        if (isDownJumpInput && _ref._Ground != null && _ref._Ground.isGrounded && _ref._Ground.isOnFloatGround)
        {
            int floatGroundLayer = LayerMask.NameToLayer(_ref._Ground.FloatGroundLayerName);

            if (floatGroundLayer >= 0)
            {
                StartPlatformIgnore(floatGroundLayer, platformDropTime);
                return;
            }
        }

        if (Input.GetKeyDown(KeyCode.Space) && jumpCount < maxJumps)
        {
            {
                // 🚨 [변경] _sync -> _ref._AnimSync, _rb -> _ref._Rb
                if (_ref._AnimSync != null) _ref._AnimSync.Jump();
                if (_ref._Rb != null) _ref._Rb.linearVelocity = new Vector2(_ref._Rb.linearVelocity.x, jumpForce);

                // 🚨 [변경] _ground -> _ref._Ground
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
        // 🚨 [변경] _dashSkill -> _ref._Dash
        if (_ref._Dash != null && _ref._Dash.IsDashing) return;

        // 🚨 [변경] _rb -> _ref._Rb
        if (_ref._Rb != null) _ref._Rb.linearVelocity = new Vector2(moveInput * speed, _ref._Rb.linearVelocity.y);

        if (moveInput != 0f)
        {
            facingDir = (int)Mathf.Sign(moveInput);
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * facingDir;
            transform.localScale = scale;
        }

        // 🚨 [변경] _sync -> _ref._AnimSync
        if (_ref._AnimSync != null)
        {
            _ref._AnimSync.AirSpeedY(_ref._Rb.linearVelocity.y);
            _ref._AnimSync.IsWalking(moveInput != 0f);
        }
    }

    private void StartPlatformIgnore(int layerToIgnore, float duration)
    {
        Physics2D.IgnoreLayerCollision(gameObject.layer, layerToIgnore, true);

        // 🚨 [변경] _ground -> _ref._Ground
        if (_ref._Ground != null)
        {
            _ref._Ground.IgnoreFloatGroundLayer();
        }

        if (_platformIgnoreRoutine != null)
        {
            StopCoroutine(_platformIgnoreRoutine);
        }
        _platformIgnoreRoutine = StartCoroutine(PlatformDropResetRoutine(layerToIgnore, duration));

        // 🚨 [변경] _playerCollider -> _ref._Col
        float colliderHeight = _ref._Col != null ? _ref._Col.bounds.extents.y : 0.5f;
        float pushUpAmount = colliderHeight + 0.05f;

        // 🚨 [변경] _rb -> _ref._Rb
        if (_ref._Rb != null && _ref._Rb.linearVelocity.y > 0)
        {
            transform.position += new Vector3(0f, pushUpAmount, 0f);
        }
    }

    // 🚀 [수정됨] ignoreCooldown 파라미터 추가
    // 갈고리에 걸릴 때는 쿨타임과 상관없이 점프 카운트를 초기화해야 하므로 true를 사용합니다.
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
        // 🚨 [변경] _rb -> _ref._Rb
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
        // 🚨 [변경] _rb -> _ref._Rb
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

    public IEnumerator RecoverAfterAnimationEnd(int stateHash)
    {
        yield return null;
        _animationSlowRoutine = null;
    }

    public IEnumerator JumpRoutine()
    {
        yield return null;
        isjump = false;
    }

    private IEnumerator PlatformDropResetRoutine(int layerToIgnore, float duration)
    {
        yield return new WaitForSeconds(duration);

        Physics2D.IgnoreLayerCollision(gameObject.layer, layerToIgnore, false);

        // 🚨 [변경] _ground -> _ref._Ground
        if (_ref._Ground != null)
        {
            _ref._Ground.ResumeFloatGroundLayer();
        }

        _platformIgnoreRoutine = null;
    }
}