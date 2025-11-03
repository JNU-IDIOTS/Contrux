using System.Collections;
using UnityEngine;

public class DashSkill : MonoBehaviour
{
    [Tooltip("currentMoveSpeed에 곱해줄 대쉬 배수")]
    public float dashMultiplier = 2f;

    public float dashDuration = 0.25f;
    public float dashCooldown = 5.0f;

    public int maxStacks = 2;
    private int currentStacks;
    private bool isDashing = false;
    public bool isInvincible = false;

    public GameObject afterimagePrefab;
    public float afterimageInterval = 0.05f;

    private PlayerAnimationSync sync;
    private Coroutine dashCoroutine;
    private Player_move playermove;
    private DashCooldownUI cooldownUI; //대쉬 쿨타임 연결 UI

    public bool IsDashing => isDashing;
    public bool IsInvincible => isInvincible;
    public int CurrentStacks => currentStacks;

    private void Awake()
    {
        currentStacks = maxStacks;
        playermove = GetComponent<Player_move>();
        sync = GetComponent<PlayerAnimationSync>();
    }

    public void TryDash(Vector2 dir, Rigidbody2D rb/*, PlayerAnimatorController animController*/)
    {
        if (dir == Vector2.zero || isDashing || currentStacks <= 0)
            return;

        //anim = animController;

        if (dashCoroutine != null)
            StopCoroutine(dashCoroutine);

        dashCoroutine = StartCoroutine(DashCoroutine(dir.normalized, rb));  // 
        currentStacks--;
        StartCoroutine(RechargeStack());  // 
        if (cooldownUI != null)
            cooldownUI.StartCooldown(); //UI 연동 여기서 실행
    }
    public void SetCooldownUI(DashCooldownUI ui)
    {
        cooldownUI = ui;
    }

    private IEnumerator DashCoroutine(Vector2 dir, Rigidbody2D rb)
    {
        isDashing = true;
        isInvincible = true;

        StartCoroutine(SpawnAfterimages());

        float originalGravity = rb.gravityScale;
        sync.Dash();
        //anim.Dash();
        rb.gravityScale = 0f;

        float baseSpeed = playermove.finalSpeed;
        float dashSpeed = baseSpeed * dashMultiplier;
        rb.linearVelocity = dir * dashSpeed;

        yield return new WaitForSeconds(dashDuration);

        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = originalGravity;
        //anim.AirSpeedY(rb.linearVelocity.y);
        sync.AirSpeedY(rb.linearVelocity.y);

        isDashing = false;
        isInvincible = false;
    }

    private IEnumerator RechargeStack()
    {
        yield return new WaitForSeconds(dashCooldown);
        currentStacks = Mathf.Min(currentStacks + 1, maxStacks);
    }
    private IEnumerator SpawnAfterimages()
    {
        while (isDashing)
        {
            // 1) Afterimage 생성
            GameObject ghost = Instantiate(afterimagePrefab, transform.position, transform.rotation);

            // 2) 플레이어가 flipX 방식으로 좌우 반전하는 경우
            //    자식 SpriteRenderer 전부에 flipX 복사
            var ghostSRs = ghost.GetComponentsInChildren<SpriteRenderer>();
            var playerSRs = GetComponentsInChildren<SpriteRenderer>();
            for (int i = 0; i < ghostSRs.Length; i++)
            {
                ghostSRs[i].flipX = playerSRs[i].flipX;
                //ghostSRs[i].flipY = playerSRs[i].flipY; // 필요하면 Y도 복사
            }

            // 또는 3) 플레이어가 transform.localScale.x = ±1 로 반전하는 경우
            ghost.transform.localScale = transform.localScale;

            yield return new WaitForSeconds(afterimageInterval);
        }
    }
}
