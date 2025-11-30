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
    //[SerializeField] float hookGravityScale = 0.1f; // 사용되지 않음(우현)
    private float hookDetachTimer = 0f;
    public int segments = 20;
    public float sagAmount = 0.3f;

    [Header("포물선 이동 설정")]
    public float parabolaDuration = 0.5f;
    public float parabolaPeakHeight = 1.5f;

    [Header("사슬 텍스처 설정")]
    [Tooltip("사슬 텍스처의 하나의 패턴(고리)이 차지하는 월드 길이. 이 값으로 텍스처가 반복됩니다.")]
    public float chainSegmentLength = 1.2f;

    [Header("갈고리 스프라이트 오프셋")]
    public float hookSpriteOffset = -0.1f;

    [Header("상태 플래그")]
    public bool isHookActive;
    public bool isLineMax;
    public bool isAttachReady;
    public bool isDashingToHook;

    private Transform hookOriginalParent;

    [Header("갈고리 쿨타임")]
    public float baseHookCooldown = 3f; 
    private float hookCooldownTimer = 0f;

    public Transform _playerTransform;
    private PlayerRef _ref;
    private Vector2 launchDir;
    private SpriteRenderer _hookSpriteRenderer;

    private void Awake()
    {
        // 🚀 [수정(우현)] 태그로 플레이어 찾기 (안전장치)
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            _ref = playerObj.GetComponent<PlayerRef>();
            _playerTransform = playerObj.transform;
        }

        hookOriginalParent = _hook.parent;
        _hookSpriteRenderer = _hook.GetComponent<SpriteRenderer>();
        if (_hookSpriteRenderer == null) _hookSpriteRenderer = _hook.GetComponentInChildren<SpriteRenderer>();

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
        // 🚀 [수정(우현)] 일시정지 체크
        if (Time.timeScale == 0) return;

        if (hookCooldownTimer > 0f) hookCooldownTimer -= Time.deltaTime;

        if (_chainLR != null && (isHookActive && !isLineMax || isAttachReady || isDashingToHook))
        {
            if (!_chainLR.enabled) _chainLR.enabled = true;
            _chainLR.SetPosition(0, _hookStart.position);

            Vector3 hookEndPosition = _hook.position;
            if (isHookActive || isAttachReady)
            {
                Vector2 directionToPlayer = (_hookStart.position - _hook.position).normalized;
                hookEndPosition = _hook.position + (Vector3)directionToPlayer * hookSpriteOffset;
            }
            _chainLR.SetPosition(1, hookEndPosition);

            float chainLength = Vector2.Distance(_hookStart.position, hookEndPosition);
            float tileCount = chainLength / chainSegmentLength;
            _chainLR.material.mainTextureScale = new Vector2(tileCount, 1);
        }
        else if (_chainLR != null && _chainLR.enabled)
        {
            _chainLR.enabled = false;
        }

        // 🚀 [수정(우현)] KeyManager 사용
        if (Input.GetKeyDown(KeyManager.Instance.KeyHook) && !isHookActive && !isAttachReady && !isDashingToHook && hookCooldownTimer <= 0f)
        {
            LaunchHook();
        }

        // (이동 로직 기존 유지)
        if (isHookActive && !isAttachReady && !isLineMax && !isDashingToHook)
        {
            _hook.position += (Vector3)(launchDir * Time.deltaTime * hookSpeed);
            DetectRingAndAttach();

            if (Vector2.Distance(_hookStart.position, _hook.position) >= maxDistance)
            {
                isLineMax = true;
                if (_hookSpriteRenderer != null) _hookSpriteRenderer.enabled = false;
                if (_chainLR != null && _chainLR.enabled) _chainLR.enabled = false;
            }
        }
        else if (isHookActive && isLineMax && !isAttachReady && !isDashingToHook)
        {
            _hook.position = Vector2.MoveTowards(_hook.position, _hookStart.position, Time.deltaTime * hookSpeed);
            if (Vector2.Distance(_hookStart.position, _hook.position) < 0.1f)
            {
                isHookActive = false;
                isLineMax = false;
                if (_chainLR != null && _chainLR.enabled) _chainLR.enabled = false;
            }
        }
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

        if (_hookSpriteRenderer != null) _hookSpriteRenderer.enabled = true;

        Vector2 dir = Vector2.zero;
        // 🚀 [수정(우현)] KeyManager 사용
        if (Input.GetKey(KeyManager.Instance.KeyUp)) dir.y += 1f;
        if (Input.GetKey(KeyManager.Instance.KeyDown)) dir.y -= 1f;
        if (Input.GetKey(KeyManager.Instance.KeyLeft)) dir.x -= 1f;
        if (Input.GetKey(KeyManager.Instance.KeyRight)) dir.x += 1f;

        if (dir == Vector2.zero && _ref._Move != null) dir.x = _ref._Move.GetFacingDir;
        else if (dir == Vector2.zero) dir.x = 1f;

        launchDir = dir.normalized;
        isHookActive = true;
        isLineMax = false;

        if (_hook != null)
        {
            float targetAngle = Mathf.Atan2(launchDir.y, launchDir.x) * Mathf.Rad2Deg;
            float roundedAngle = Mathf.Round(targetAngle / 45f) * 45f;
            float finalZRotation = roundedAngle - 90f;
            _hook.rotation = Quaternion.AngleAxis(finalZRotation, Vector3.forward);
        }

        // 상점 스탯 적용
        float cooldownMult = (_ref._Status != null) ? _ref._Status.ItemCooldownMult : 1.0f;
        hookCooldownTimer = baseHookCooldown * cooldownMult;
    }

    private void DetectRingAndAttach()
    {
        Collider2D hit = Physics2D.OverlapCircle(_hook.position, attachRadius);
        if (hit != null && hit.CompareTag("RING"))
        {
            _hook.position = hit.ClosestPoint(_hook.position);
            isAttachReady = true;
            if (_ref._Move != null) _ref._Move.ResetJumpCount(true);
            isHookActive = false;
            isLineMax = false;
            if (_ref._Rb != null) _ref._Rb.linearVelocity = Vector2.zero;
            if (_ref._Move != null) _ref._Move.SetAttackLock(true);
            return;
        }
    }

    private void HandleAttachReadyInput()
    {
        // 🚀 [수정(우현)] KeyManager 사용
        if (Input.GetKeyDown(KeyManager.Instance.KeyHook))
        {
            isDashingToHook = true;
            if (_ref._Move != null) _ref._Move.SetAttackLock(false);
            StartCoroutine(ParabolaMoveToHook());
            return;
        }
        
        // 🚀 [수정(우현)] KeyJump로 취소
        if (Input.GetKeyDown(KeyManager.Instance.KeyJump))
        {
            ResetHookState(true);
            return;
        }
        if (hookDetachTimer > 0.7f) ResetHookState();
    }

    public void ResetHookState(bool isJumping = false)
    {
        isAttachReady = false;
        isHookActive = false;
        isDashingToHook = false;
        isLineMax = false;

        if (_hook.parent != hookOriginalParent) _hook.SetParent(hookOriginalParent, true);
        _hook.position = _hookStart.position;
        if (_hook != null) _hook.localRotation = Quaternion.identity;

        if (_hookSpriteRenderer != null) _hookSpriteRenderer.enabled = false;
        if (_chainLR != null && _chainLR.enabled) _chainLR.enabled = false;

        if (!isJumping && _ref._Rb != null) _ref._Rb.linearVelocity = Vector2.zero;
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

        if (_ref._Rb != null) _ref._Rb.linearVelocity = Vector2.zero;

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
        if (_ref._Rb != null) _ref._Rb.linearVelocity = Vector2.zero;

        isDashingToHook = false;
        isHookActive = false;
        isLineMax = false;
        ResetHookState();
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