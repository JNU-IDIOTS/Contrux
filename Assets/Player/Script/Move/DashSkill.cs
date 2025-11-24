using System.Collections;
using UnityEngine;

public class DashSkill : MonoBehaviour
{
    [Header("대쉬 설정")]
    public float dashDistance = 4f;
    public float dashDuration = 0.2f;
    
    [SerializeField] private float baseDashCooldown = 5.0f; 
    private float currentDashCooldown; 

    public int maxStacks = 2;
    public int currentStacks;
    private float cooldownTimer = 0f;

    [Header("잔상 설정")]
    public GameObject ghostPrefab;
    public float ghostInterval = 0.05f;

    private PlayerRef _ref;
    
    // 🚀 [수정 1] 외부에서 읽을 수 있게 프로퍼티 추가 (에러 해결)
    public bool IsDashing => isDashing; 
    private bool isDashing = false;

    private void Awake()
    {
        _ref = GetComponent<PlayerRef>();
    }

    private void Start()
    {
        // 상점 스탯 적용(우현)
        if (_ref._Status != null)
        {
            maxStacks += _ref._Status.BonusDashCount; 
            currentDashCooldown = baseDashCooldown * _ref._Status.SkillCooldownMult; 
        }
        else
        {
            currentDashCooldown = baseDashCooldown;
        }

        currentStacks = maxStacks;
    }

    private void Update()
    {
        // 쿨타임 회복(우현)
        if (currentStacks < maxStacks)
        {
            cooldownTimer += Time.deltaTime;
            if (cooldownTimer >= currentDashCooldown)
            {
                currentStacks++;
                cooldownTimer = 0f;
                // Debug.Log($"대쉬 회복! 현재 스택: {currentStacks}");
            }
        }

        // 🚀 [수정 2] 자체 입력 감지는 PlayerMoveController가 호출해주므로 제거하거나 유지해도 되지만,
        // PlayerMoveController가 TryDash를 호출하는 구조라면 여기서는 입력을 뺍니다.
        // (만약 PlayerMoveController 없이 단독으로 쓸 때는 아래 주석 해제)
        /*
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            TryDash();
        }
        */
    }

    // 🚀 [수정 3] 외부(PlayerMoveController)에서 호출할 수 있는 공개 함수 추가 (에러 해결)
    public bool TryDash()
    {
        if (!isDashing && currentStacks > 0)
        {
            // 입력 벡터 가져오기 시도
            Vector2 inputDir = Vector2.zero;
            if (_ref._Move != null) 
            {
                inputDir = _ref._Move.GetInputVector(); // PlayerMoveController에 이 함수가 있어야 함
            }

            // 이동 중이거나 제자리일 때 처리
            if (inputDir.sqrMagnitude > 0.01f || _ref._Move.GetFacingDir != 0)
            {
                StartCoroutine(PerformDash(inputDir));
                return true; // 대쉬 성공
            }
        }
        return false; // 대쉬 실패
    }

    private IEnumerator PerformDash(Vector2 inputDir)
    {
        isDashing = true;
        currentStacks--;
        
        if (_ref._Move != null) _ref._Move.SetAttackLock(true); 

        // 입력이 없으면 바라보는 방향으로
        Vector2 dashDir = inputDir.normalized;
        if (dashDir == Vector2.zero && _ref._Move != null) 
            dashDir = new Vector2(_ref._Move.GetFacingDir, 0);

        float elapsed = 0f;
        StartCoroutine(SpawnGhosts());

        while (elapsed < dashDuration)
        {
            if (_ref._Rb != null)
                _ref._Rb.linearVelocity = dashDir * (dashDistance / dashDuration);
            
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (_ref._Rb != null) _ref._Rb.linearVelocity = Vector2.zero;
        if (_ref._Move != null) _ref._Move.SetAttackLock(false);
        isDashing = false;
    }

    private IEnumerator SpawnGhosts()
    {
        while (isDashing)
        {
            if (ghostPrefab != null)
            {
                GameObject ghost = Instantiate(ghostPrefab, transform.position, transform.rotation);
                SpriteRenderer ghostSr = ghost.GetComponent<SpriteRenderer>();
                SpriteRenderer playerSr = GetComponentInChildren<SpriteRenderer>();
                
                if (ghostSr != null && playerSr != null)
                {
                    ghostSr.sprite = playerSr.sprite;
                    ghostSr.flipX = playerSr.flipX;
                    // 색상 등을 흐리게 하거나 쉐이더 설정 가능
                }
                Destroy(ghost, 0.5f);
            }
            yield return new WaitForSeconds(ghostInterval);
        }
    }
}