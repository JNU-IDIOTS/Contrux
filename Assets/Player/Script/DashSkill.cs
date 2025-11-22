using System.Collections;
using UnityEngine;

public class DashSkill : MonoBehaviour
{
    [Tooltip("기본 이동 속도에 적용되는 대시 배율")]
    public float dashMultiplier = 5f;

    public float dashDuration = 0.25f;
    public float dashCooldown = 5.0f;

    [Header("스택 설정")]
    public int maxStacks = 2;
    private int _currentStacks;

    // 상태 플래그
    private bool _isDashing = false;
    public bool isInvincible = false;

    [Header("잔상 효과")]
    public GameObject afterimagePrefab;
    public float afterimageInterval = 0.05f;

    // 🚨 잔상 투명도 제어 변수
    private int _afterimageCount; // 잔상 순서를 추적하는 멤버 변수
    private const float MaxAlpha = 1.0f;
    private const float MinAlpha = 0.4f;
    private const int MaxAlphaSteps = 5; // 투명도 변화를 나눌 단계 수

    // 🚨 [변경] 개별 참조 변수 제거 -> PlayerRef 사용
    private PlayerRef _ref;
    private Coroutine _dashCoroutine;

    public bool IsDashing => _isDashing;
    public bool IsInvincible => isInvincible;
    public int GetCurrentStacks => _currentStacks;

    private void Awake()
    {
        _currentStacks = maxStacks;

        // 🚨 [변경] PlayerRef 할당
        _ref = GetComponent<PlayerRef>();
    }


    public void TryDash(Vector2 dir, Rigidbody2D rb/*, PlayerAnimatorController animController*/)
    {
        if (dir == Vector2.zero || _isDashing || _currentStacks <= 0)
            return;

        if (_dashCoroutine != null)
            StopCoroutine(_dashCoroutine);

        _afterimageCount = 0; // 🚨 대시 시작 시 카운트 0으로 초기화

        _dashCoroutine = StartCoroutine(DashCoroutine(dir.normalized, rb));
        _currentStacks--;
        StartCoroutine(RechargeStack());
    }

    private IEnumerator DashCoroutine(Vector2 dir, Rigidbody2D rb)
    {
        _isDashing = true;
        isInvincible = true;

        StartCoroutine(SpawnAfterimages());

        float originalGravity = rb.gravityScale;
        // 🚨 [변경] _sync -> _ref._AnimSync
        if (_ref._AnimSync != null) _ref._AnimSync.Dash();

        rb.gravityScale = 0f;

        // 🚨 [변경] _playerMove -> _ref._Move
        float baseSpeed = (_ref._Move != null) ? _ref._Move.speed : 0f;
        float dashSpeed = baseSpeed * dashMultiplier;

        rb.linearVelocity = dir * dashSpeed;

        yield return new WaitForSeconds(dashDuration);

        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = originalGravity;
        // 🚨 [변경] _sync -> _ref._AnimSync
        if (_ref._AnimSync != null) _ref._AnimSync.AirSpeedY(rb.linearVelocity.y);

        _isDashing = false;
        isInvincible = false;
    }

    private IEnumerator RechargeStack()
    {
        yield return new WaitForSeconds(dashCooldown);
        _currentStacks = Mathf.Min(_currentStacks + 1, maxStacks);
    }

    private IEnumerator SpawnAfterimages()
    {
        while (_isDashing)
        {
            _afterimageCount++; // 🚨 잔상 생성 순서 증가

            // 1) 잔상 생성
            GameObject ghost = Instantiate(afterimagePrefab, transform.position, transform.rotation);

            // 2) 플레이어의 scale (바라보는 방향)과 동일하게 설정
            ghost.transform.localScale = transform.localScale;

            // 🟢 [핵심 로직]: 생성 순서에 따른 투명도 계산 및 설정

            // 2-1) 투명도 변화 단계 계산 (0 ~ 9 순환)
            float t = (float)((_afterimageCount - 1) % MaxAlphaSteps) / (MaxAlphaSteps - 1f);

            // 2-2) 투명도 Lerp: 본체와 가까울수록 연하게 (minAlpha), 멀수록 진하게 (maxAlpha)
            float currentAlpha = Mathf.Lerp(MinAlpha, MaxAlpha, t);

            // 2-3) FadeOut 컴포넌트에 초기 투명도를 주입
            FadeOut fadeOut = ghost.GetComponent<FadeOut>();
            if (fadeOut != null)
            {
                fadeOut.SetInitialAlpha(currentAlpha);
            }

            // 3) SpriteRenderer의 flipX 상태 복사
            var ghostSRs = ghost.GetComponentsInChildren<SpriteRenderer>();
            var playerSRs = GetComponentsInChildren<SpriteRenderer>();

            for (int i = 0; i < ghostSRs.Length; i++)
            {
                if (i < playerSRs.Length)
                {
                    ghostSRs[i].flipX = playerSRs[i].flipX;
                }
            }

            yield return new WaitForSeconds(afterimageInterval);
        }
    }
}