using UnityEngine;
using System.Collections;

public class SteamPunk_Hook : MonoBehaviour
{
    // 🚨 컴포넌트 참조 변수 규칙 적용
    [Header("라인 렌더러 설정")]
    public LineRenderer _chainLR;
    public Transform _hook;
    public Transform _hookStart;

    [Header("속성")]
    public float hookSpeed = 15f;
    public float maxDistance = 5f;
    public float dashSpeed = 20f;
    public float attachRadius = 0.1f;
    [SerializeField] float hookGravityScale = 0.1f; // 사용되지 않음
    private float hookDetachTimer = 0f;
    public int segments = 20;
    public float sagAmount = 0.3f;

    [Header("포물선 이동 설정")]
    public float parabolaDuration = 0.5f;
    public float parabolaPeakHeight = 1.5f;

    [Header("사슬 텍스처 설정")]
    [Tooltip("사슬 텍스처의 하나의 패턴(고리)이 차지하는 월드 길이. 이 값으로 텍스처가 반복됩니다.")]
    public float chainSegmentLength = 1.2f;

    // 🚀 [라인 렌더러 보정 변수 추가]
    [Header("갈고리 스프라이트 오프셋")]
    [Tooltip("갈고리 스프라이트의 중심(Pivot)에서 사슬 연결부까지의 거리 (월드 스케일)")]
    public float hookSpriteOffset = -0.1f;

    [Header("상태 플래그")]
    public bool isHookActive;
    public bool isLineMax;
    public bool isAttachReady;
    public bool isDashingToHook;

    private Transform hookOriginalParent;

    // ── 쿨타임 변수 추가 ─────────────────────────────────────────────────
    [Header("갈고리 쿨타임")]
    [SerializeField] public static float hookCooldown = 3f;
    private float hookCooldownTimer = 0f;

    // 🚨 컴포넌트 참조 변수 규칙 적용 및 통합
    public Transform _playerTransform;
    private PlayerRef _ref;
    private Vector2 launchDir;
    private SpriteRenderer _hookSpriteRenderer;

    private void Awake()
    {
        // 🚀 자동 할당: 플레이어 관련 컴포넌트
        // 🚨 [변경] PlayerRef 할당 (같은 오브젝트)
        _ref = GetComponent<PlayerRef>();
        hookOriginalParent = _hook.parent;

        // 🚀 자동 할당: 갈고리 이미지 컴포넌트
        _hookSpriteRenderer = _hook.GetComponent<SpriteRenderer>();
        if (_hookSpriteRenderer == null)
        {
            _hookSpriteRenderer = _hook.GetComponentInChildren<SpriteRenderer>();
        }

        // 🚨 [변경] _ref 확인으로 변경
        if (_ref == null || _ref._Rb == null || _ref._Move == null)
            Debug.LogWarning("Hook 스크립트: PlayerRef 또는 필수 컴포넌트를 찾을 수 없습니다.");

        if (_hookSpriteRenderer == null)
            Debug.LogWarning("Hook 스크립트: 갈고리(Hook)의 자식에서 SpriteRenderer를 찾을 수 없습니다.");


        // 초기화
        isHookActive = false;
        isLineMax = false;
        isAttachReady = false;
        isDashingToHook = false;

        if (_hookSpriteRenderer != null) _hookSpriteRenderer.enabled = false;

        if (_chainLR != null)
        {
            _chainLR.positionCount = 2;
            _chainLR.enabled = false;
            _chainLR.widthMultiplier = 1f;
        }
    }

    private void Update()
    {
        // ── 쿨타임 처리 ───────────────────────────────────
        if (hookCooldownTimer > 0f)
        {
            hookCooldownTimer -= Time.deltaTime;
        }

        // ── 1) 라인 렌더러 활성/위치 갱신 ───────────────────────────────────
        if (_chainLR != null && (isHookActive && !isLineMax || isAttachReady || isDashingToHook))
        {
            if (!_chainLR.enabled) _chainLR.enabled = true;

            _chainLR.SetPosition(0, _hookStart.position);

            // 🚀 [라인 렌더러 끝점 위치 보정]
            Vector3 hookEndPosition = _hook.position;

            if (isHookActive || isAttachReady)
            {
                Vector2 directionToPlayer = (_hookStart.position - _hook.position).normalized;
                hookEndPosition = _hook.position + (Vector3)directionToPlayer * hookSpriteOffset;
            }

            _chainLR.SetPosition(1, hookEndPosition);

            // 🚀 [사슬 텍스처 타일링 계산]
            float chainLength = Vector2.Distance(_hookStart.position, hookEndPosition);
            float tileCount = chainLength / chainSegmentLength;
            _chainLR.material.mainTextureScale = new Vector2(tileCount, 1);
        }
        else if (_chainLR != null && _chainLR.enabled)
        {
            _chainLR.enabled = false;
        }

        // ── 2) 첫 번째 E: 갈고리 발사 ─────────────────────────────────────────────
        if (Input.GetKeyDown(KeyCode.E)
        && !isHookActive
        && !isAttachReady
        && !isDashingToHook
        && hookCooldownTimer <= 0f)
        {
            LaunchHook();
        }

        // ── 3) 갈고리 상태별 처리 ───────────────────────────────────

        // 3-1) 갈고리 날아가는 중
        if (isHookActive && !isAttachReady && !isLineMax && !isDashingToHook)
        {
            _hook.position += (Vector3)(launchDir * Time.deltaTime * hookSpeed);
            DetectRingAndAttach();

            if (Vector2.Distance(_hookStart.position, _hook.position) >= maxDistance)
            {
                isLineMax = true;
                if (_hookSpriteRenderer != null)
                    _hookSpriteRenderer.enabled = false;
                if (_chainLR != null && _chainLR.enabled)
                    _chainLR.enabled = false;
            }
        }
        // 3-2) 갈고리 복귀 모드
        else if (isHookActive && isLineMax && !isAttachReady && !isDashingToHook)
        {
            _hook.position = Vector2.MoveTowards(_hook.position, _hookStart.position, Time.deltaTime * hookSpeed);

            if (Vector2.Distance(_hookStart.position, _hook.position) < 0.1f)
            {
                isHookActive = false;
                isLineMax = false;
                if (_chainLR != null && _chainLR.enabled)
                    _chainLR.enabled = false;
            }
        }
        // 3-3) 갈고리가 Ring에 걸려서 대기 상태
        else if (isAttachReady && !isDashingToHook)
        {
            HandleAttachReadyInput();
            hookDetachTimer += Time.deltaTime;
        }
    }

    public void LaunchHook()
    {
        if (_hook.parent != null) _hook.SetParent(null, true);
        _hook.position = _hookStart.position;

        if (_hookSpriteRenderer != null)
            _hookSpriteRenderer.enabled = true;

        Vector2 dir = Vector2.zero;
        if (Input.GetKey(KeyCode.UpArrow)) dir.y += 1f;
        if (Input.GetKey(KeyCode.DownArrow)) dir.y -= 1f;
        if (Input.GetKey(KeyCode.LeftArrow)) dir.x -= 1f;
        if (Input.GetKey(KeyCode.RightArrow)) dir.x += 1f;

        // 🚨 [변경] _playerMove -> _ref._Move
        if (dir == Vector2.zero && _ref._Move != null) dir.x = _ref._Move.GetFacingDir;
        else if (dir == Vector2.zero) dir.x = 1f;

        launchDir = dir.normalized;
        isHookActive = true;
        isLineMax = false;

        // 🚀 [갈고리 회전 기능]
        if (_hook != null)
        {
            float targetAngle = Mathf.Atan2(launchDir.y, launchDir.x) * Mathf.Rad2Deg;
            float roundedAngle = Mathf.Round(targetAngle / 45f) * 45f;
            float finalZRotation = roundedAngle - 90f;
            _hook.rotation = Quaternion.AngleAxis(finalZRotation, Vector3.forward);
        }

        hookCooldownTimer = hookCooldown;
    }

    private void DetectRingAndAttach()
    {
        Collider2D hit = Physics2D.OverlapCircle(_hook.position, attachRadius);
        if (hit != null && hit.CompareTag("RING"))
        {
            _hook.position = hit.ClosestPoint(_hook.position);
            isAttachReady = true;

            if (_ref._Move != null)
            {
                // 🚀 [수정됨] 강제 리셋 (true 파라미터 추가 예정)
                // 방금 점프했더라도 갈고리에 걸리면 즉시 점프 가능 상태로 만듭니다.
                // 🚨 [변경] _playerMove -> _ref._Move
                _ref._Move.ResetJumpCount(true);
            }

            isHookActive = false;
            isLineMax = false;

            // 🚨 [변경] _playerRb -> _ref._Rb
            if (_ref._Rb != null)
            {
                _ref._Rb.linearVelocity = Vector2.zero;
            }

            // 🚨 [변경] _playerMove -> _ref._Move
            if (_ref._Move != null)
            {
                _ref._Move.SetAttackLock(true);
            }

            return;
        }
    }

    private void HandleAttachReadyInput()
    {
        // ── 두 번째 E: 대시 시작 ────────────────────────────
        if (Input.GetKeyDown(KeyCode.E))
        {
            isDashingToHook = true;
            // 🚨 [변경] _playerMove -> _ref._Move
            if (_ref._Move != null)
            {
                _ref._Move.SetAttackLock(false);
            }
            StartCoroutine(ParabolaMoveToHook());
            return;
        }

        // 🚀 [추가됨] 점프(Space) 입력 시 갈고리 해제 및 점프 허용
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // true를 전달하여 속도를 0으로 만들지 않고 연결만 끊습니다.
            // 이후 PlayerMoveController가 같은 프레임에 점프 힘을 가하게 됩니다.
            ResetHookState(true);
            return;
        }

        // ── 자동 해제 ────────────────────────────
        if (hookDetachTimer > 0.7f)
        {
            ResetHookState();
        }
    }

    // 💡 메서드명 규칙 적용: 대문자로 시작
    // 🚀 [수정됨] isJumping 파라미터 추가 (기본값 false)
    public void ResetHookState(bool isJumping = false)
    {
        isAttachReady = false;
        isHookActive = false;
        isDashingToHook = false;
        isLineMax = false;

        if (_hook.parent != hookOriginalParent)
            _hook.SetParent(hookOriginalParent, true);

        _hook.position = _hookStart.position;

        if (_hook != null)
        {
            _hook.localRotation = Quaternion.identity;
        }

        if (_hookSpriteRenderer != null)
            _hookSpriteRenderer.enabled = false;

        if (_chainLR != null && _chainLR.enabled)
            _chainLR.enabled = false;

        // 🚀 [수정됨] 점프 중이 아닐 때만 속도를 0으로 만듭니다.
        // 점프 중이라면 PlayerMoveController가 가하는 점프 힘을 보존해야 합니다.
        // 🚨 [변경] _playerRb -> _ref._Rb
        if (!isJumping && _ref._Rb != null)
        {
            _ref._Rb.linearVelocity = Vector2.zero;
        }

        // 🚨 [변경] _playerMove -> _ref._Move
        if (_ref._Move != null) _ref._Move.SetAttackLock(false);
        hookDetachTimer = 0f;
    }

    private IEnumerator ParabolaMoveToHook()
    {
        Vector3 startPos = _playerTransform.position;
        Vector3 endPos = _hook.position;

        Vector3 midPoint = (startPos + endPos) * 0.5f;
        float distance = Vector2.Distance(startPos, endPos);
        float heightOffset = Mathf.Min(parabolaPeakHeight, distance * 0.5f);
        Vector3 controlPoint = midPoint + Vector3.up * heightOffset;

        float elapsed = 0f;
        float duration = parabolaDuration;

        // 🚨 [변경] _playerRb -> _ref._Rb
        if (_ref._Rb != null)
        {
            _ref._Rb.linearVelocity = Vector2.zero;
        }

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            Vector3 a = Vector3.Lerp(startPos, controlPoint, t);
            Vector3 b = Vector3.Lerp(controlPoint, endPos, t);
            Vector3 newPos = Vector3.Lerp(a, b, t);
            _playerTransform.position = newPos;
            yield return null;
        }

        _playerTransform.position = endPos;

        // 🚨 [변경] _playerRb -> _ref._Rb
        if (_ref._Rb != null)
        {
            _ref._Rb.linearVelocity = Vector2.zero;
        }

        isDashingToHook = false;
        isHookActive = false;
        isLineMax = false;

        ResetHookState();

        yield break;
    }

    private void OnDrawGizmosSelected()
    {
        if (_hook != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(_hook.position, attachRadius);
        }
    }
}