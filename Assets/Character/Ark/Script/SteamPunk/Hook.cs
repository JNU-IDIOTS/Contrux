using UnityEngine;
using System.Collections;

public class Hook : MonoBehaviour
{
    [Header("라인 렌더러 설정")]
    public LineRenderer chainLR;    // ChainRenderer의 LineRenderer 컴포넌트
    public Transform hook;          // 갈고리 끝점(절대 위치)
    public Transform hookstart;     // 갈고리 시작점(플레이어 손 등)

    [Header("속성")]
    public float hookSpeed = 15f;   // 갈고리 발사/복귀 속도
    public float maxDistance = 5f;  // 갈고리 사거리
    public float dashSpeed = 20f;   // 플레이어가 Ring 방향으로 돌진할 때 속도
    public float attachRadius = 0.1f; // RING 태그 감지 반경
    [SerializeField] float isHookGravity = 0.1f;
    private float hookDetachTimer = 0f; // 타이머 변수 추가
    public int segments = 20;
    public float sagAmount = 0.3f;

    [Header("포물선 이동 설정")]
    public float parabolaDuration = 0.5f;      // 두 번째 E 눌렀을 때 플레이어가 달려갈 시간(초)
    public float parabolaPeakHeight = 1.5f;    // 포물선 곡선의 높이 (거리 대비 적절히 조정)

    [Header("상태 플래그")]
    public bool isHookActive;       // 갈고리 발사 중
    public bool isLineMax;          // 갈고리 사거리 초과되어 복귀 중
    public bool isAttachReady;      // 갈고리가 Ring에 걸려 "두 번째 E"를 기다리는 상태
    public bool isDashingToHook;    // 플레이어가 Ring 방향으로 대시 중

    [Header("플레이어 관련 (외부 할당)")]
    [Tooltip("플레이어 움직임을 담당하는 스크립트를 여기에 할당하세요. Hook이 걸린 동안 이동을 잠그기 위해 사용됩니다.")]
    public MonoBehaviour playerMovementScript;   // 예: Player_move 스크립트

    // ── 쿨타임 변수 추가 ─────────────────────────────────────────────────
    [Header("갈고리 쿨타임")]
    [SerializeField] public static float hookCooldown = 3f;   // 갈고리 쿨타임 (초)
    private float hookCooldownTimer = 0f;                 // 쿨타임 타이머

    private Vector2 launchDir;
    private Rigidbody2D playerRb;   // 플레이어 Rigidbody2D (hookstart의 부모 또는 해당 객체)
    public Transform playerTransform;
    public HookUI hookUI; //HookUI와 연결하기 위한 인스턴스

    private void Awake()
    {
        // 초기화
        isHookActive = false;
        isLineMax = false;
        isAttachReady = false;
        isDashingToHook = false;

        hook.gameObject.SetActive(false);

        // LineRenderer 세팅
        chainLR.positionCount = 2;
        chainLR.enabled = false;
        // 방법 A: 곡선 대신 고정 폭(Constant) 커브 만들기

        // 방법 B: 커브에 곱해줄 배율만 바꾸기
        chainLR.widthMultiplier = 1f;

        // 플레이어 Rigidbody2D 찾기
        playerRb = hookstart.GetComponentInParent<Rigidbody2D>();
        if (playerRb == null)
            Debug.LogWarning("Hook 스크립트: hookstart의 부모 오브젝트에 Rigidbody2D를 찾을 수 없습니다.");
    }

    private void Update()
    {
        // ── 쿨타임 처리: 쿨타임이 끝나면 hookCooldownTimer <= 0이 되어 발사 가능 ───────────────────────────────────
        if (hookCooldownTimer > 0f)
        {
            hookCooldownTimer -= Time.deltaTime;
        }

        // ────────────────────────────────────────────────────────────────────────
        // 1) 라인 렌더러 활성/비활성 및 위치 갱신
        if (isHookActive || isAttachReady || isDashingToHook)
        {
            if (!chainLR.enabled) chainLR.enabled = true;
            chainLR.SetPosition(0, hookstart.position);
            chainLR.SetPosition(1, hook.position);
            float chainLength = Vector2.Distance(hookstart.position, hook.position);
            chainLR.material.mainTextureScale = new Vector2(chainLength / chainLR.startWidth, 1);
        }
        else
        {
            if (chainLR.enabled) chainLR.enabled = false;
        }
        
        // ────────────────────────────────────────────────────────────────────────

        // ── 첫 번째 E: 갈고리 발사 ─────────────────────────────────────────────
        if (Input.GetKeyDown(KeyCode.E)
        && !isHookActive
        && !isAttachReady
        && !isDashingToHook
        && hookCooldownTimer <= 0f)    // 쿨타임이 0 이하여야 발사 가능
        {
            // 갈고리 위치 초기화
            hook.position = hookstart.position;
            hook.gameObject.SetActive(true);

            // 방향키로 발사 방향 계산 (없으면 오른쪽 기본)
            Vector2 dir = Vector2.right;
            if (Input.GetKey(KeyCode.UpArrow)) dir += Vector2.up;
            if (Input.GetKey(KeyCode.DownArrow)) dir += Vector2.down;

            if (dir == Vector2.zero)
                dir = Vector2.right;

            launchDir = dir.normalized;
            isHookActive = true;
            isLineMax = false;

            // ── 여기서 쿨타임 타이머 초기화 ─────────────────────────────────
            hookCooldownTimer = hookCooldown;
            hookUI.StartHookCooldown(); //HookUI에서의 쿨타임 시작
            // ────────────────────────────────────────────────────────────────
        }
        // ── 갈고리 날아가는 중 (사거리 이내) ───────────────────────────────────
        if (isHookActive && !isAttachReady && !isLineMax && !isDashingToHook)
        {
            // (1) hook 이동
            hook.Translate(launchDir * Time.deltaTime * hookSpeed);

            // (2) RING 태그 감지: 걸리면 isAttachReady = true로 전환
            Collider2D hit = Physics2D.OverlapCircle(hook.position, attachRadius);
            if (hit != null && hit.CompareTag("RING"))
            {
                // 걸린 지점으로 고정
                hook.position = hit.ClosestPoint(hook.position);

                // 상태 전환
                isAttachReady = true;
                Player_move playermove = GetComponent<Player_move>();
                if (playermove != null)
                {
                    playermove.ResetJumpCount();
                }
                isHookActive = false;
                isLineMax = false;

                // 중력 감소 및 이동 잠금
                if (playerRb != null)
                {
                    playerRb.linearVelocity = Vector2.zero;
                    playerRb.gravityScale = isHookGravity;
                }
                if (playerMovementScript != null)
                {
                    playerMovementScript.enabled = false;
                }
                // 0.5초 뒤에 gravity를 다시 1로 복원
                StartCoroutine(RestoreGravityAfterDelay());
                return; // 더 이상 사거리 체크하지 않고 이 라인에서 빠져나감
            }

            // (3) 사거리 초과 시 복귀 모드로 전환
            if (Vector2.Distance(hookstart.position, hook.position) >= maxDistance)
            {
                isLineMax = true;
            }
        }
        // ── 갈고리 복귀 모드 (사거리 초과 후) ───────────────────────────────────
        else if (isHookActive && isLineMax && !isAttachReady && !isDashingToHook)
        {
            hook.position = Vector2.MoveTowards(hook.position, hookstart.position, Time.deltaTime * hookSpeed);
            if (Vector2.Distance(hookstart.position, hook.position) < 0.1f)
            {
                isHookActive = false;
                isLineMax = false;
                hook.gameObject.SetActive(false);
            }
        }
        // ── 갈고리가 Ring에 걸려서 두 번째 E 대기 상태 ────────────────────────────
        else if (isAttachReady && !isDashingToHook)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                isDashingToHook = true;
                // 갈고리 자체는 즉시 해제
                hook.gameObject.SetActive(false);
                StartCoroutine(ParabolaMoveToHook()); // 플레이어 포물선 이동 코루틴
            }
        }
        // ── 플레이어가 Ring 방향으로 dash(포물선) 중 ───────────────────────────
        else if (isDashingToHook)
        {
            // 이동은 ParabolaMoveToHook에서 처리
        }

        // ── 디버그용 로그 (필요 시 주석 처리) ────────────────────────────────────
        // Debug.Log($"isAttachReady : {isAttachReady}");

        // ── 붙어있는 동안 자동 해제 타이머 ───────────────────────────────────────
        if (isAttachReady)
        {
            hookDetachTimer += Time.deltaTime;
            if (hookDetachTimer > 0.7f)  // 시간이 지나면 자동으로 해제
            {
                isAttachReady = false;
                ResetHookState();  // 갈고리 연결 해제
            }
        }
        if (Input.GetKeyDown(KeyCode.E) && isAttachReady)
        {
            isAttachReady = false;
            ResetHookState();  // 갈고리 연결 해제
        }
    }

    private void ResetHookState()
    {
        // 갈고리 상태 초기화
        if (playerRb != null)
        {
            playerRb.gravityScale = 1.7f;  // 원래 중력 복원
        }
        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = true; // 이동 가능
        }
        hookDetachTimer = 0f; // 타이머 초기화
    }

    /// <summary>
    /// 갈고리가 걸린 순간에 중력을 0.3으로 낮추고, 0.5초 후에 다시 1.7로 복원합니다.
    /// </summary>
    private IEnumerator RestoreGravityAfterDelay()
    {
        yield return new WaitForSeconds(0.5f);

        if (playerRb != null)
            playerRb.gravityScale = 1.7f;
    }

    /// <summary>
    /// 두 번째 E를 눌렀을 때, 플레이어를 포물선을 그려 hook 위치로 이동시키는 코루틴
    /// </summary>
    private IEnumerator ParabolaMoveToHook()
    {
        // (1) 시작 위치: 반드시 “playerTransform.position”을 가져와야 합니다.
        Vector3 startPos = playerTransform.position;
        Vector3 endPos = hook.position;

        // 제어점 계산: 중간 지점 위로 parabolaPeakHeight만큼 띄움
        Vector3 midPoint = (startPos + endPos) * 0.5f;
        float distance = Vector2.Distance(startPos, endPos);
        float heightOffset = Mathf.Min(parabolaPeakHeight, distance * 0.5f);
        Vector3 controlPoint = midPoint + Vector3.up * heightOffset;

        float elapsed = 0f;
        float duration = parabolaDuration;

        // 이동 중 물리 영향 차단
        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector2.zero;
            playerRb.gravityScale = 0f;
        }

        // Quadratic Bezier 공식으로 위치 업데이트
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            Vector3 a = Vector3.Lerp(startPos, controlPoint, t);
            Vector3 b = Vector3.Lerp(controlPoint, endPos, t);
            Vector3 newPos = Vector3.Lerp(a, b, t);
            // “playerTransform”을 직접 이동시킵니다.
            playerTransform.position = newPos;
            yield return null;
        }

        // 최종 위치 보정
        playerTransform.position = endPos;

        // 이동 완료 후 원래 중력·이동 스크립트 복원
        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector2.zero;
            playerRb.gravityScale = 1.7f;
        }
        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = true;
        }

        // 상태 초기화
        isAttachReady = false;
        isDashingToHook = false;
        isHookActive = false;
        isLineMax = false;

        if (chainLR.enabled)
            chainLR.enabled = false;

        yield break;
    }

    // (선택) Scene 뷰에 attachRadius 시각화
    private void OnDrawGizmosSelected()
    {
        if (hook != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(hook.position, attachRadius);
        }
    }
}