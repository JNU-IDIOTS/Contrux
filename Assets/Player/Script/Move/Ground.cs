using UnityEngine;

// 클래스 이름을 GroundChecker로 사용하는 것이 일반적이지만, 파일 이름과 일치시키기 위해 Ground를 유지합니다.
public class Ground : MonoBehaviour
{
    [Header("Ground Check Settings")]
    [Tooltip("일반 지형 레이어 마스크")]
    [SerializeField] private LayerMask groundMask;

    [Tooltip("밟았을 때 특별 처리가 필요한 (예: 플랫폼) 지형의 Layer Name")]
    [SerializeField] private string floatGroundLayerName = "FloatGround";
    public string FloatGroundLayerName => floatGroundLayerName;

    [Tooltip("레이캐스트 길이")]
    [SerializeField] public float rayDistance = 1.03f;

    [Tooltip("레이캐스트 시작점의 Y축 오프셋 (Collider의 바닥 기준)")]
    [SerializeField] public float rayOriginOffsetY = 0f;

    [Tooltip("레이캐스트 시작점의 X축 오프셋")]
    [SerializeField] public float rayOriginOffsetX = 0.05f;

    [Header("Raycast Configuration")]
    [Tooltip("발사할 레이캐스트 간의 간격")]
    public float spacing = 0.35f;
    private const int RayCount = 3;

    // 내부 상태 변수
    private int _floatGroundLayerIndex;
    private int _combinedMask;
    private int facingDirection;

    // 외부에 공개되는 상태
    [HideInInspector] public bool isGrounded = false;
    [HideInInspector] public bool isOnFloatGround = false;

    // 내부 컴포넌트 참조
    // 🚨 [변경] 개별 참조 변수 제거 -> PlayerRef 사용
    private PlayerRef _ref;

    private void Awake()
    {
        // 🚨 [변경] PlayerRef 할당
        _ref = GetComponentInParent<PlayerRef>();

        _floatGroundLayerIndex = LayerMask.NameToLayer(floatGroundLayerName);
        if (_floatGroundLayerIndex < 0)
        {
            Debug.LogError($"Layer '{floatGroundLayerName}' not found. Check Project Settings > Tags and Layers.");
            _combinedMask = groundMask.value;
            return;
        }

        _combinedMask = groundMask.value | (1 << _floatGroundLayerIndex);
    }

    // 🚨 [새 메서드 1]: FloatGround 레이어를 마스크에서 제거 (충돌 무시 시작 시)
    public void IgnoreFloatGroundLayer()
    {
        // _combinedMask에서 FloatGround 레이어를 비활성화 (해당 비트를 0으로 만듦)
        _combinedMask &= ~(1 << _floatGroundLayerIndex);
    }

    // 🚨 [새 메서드 2]: FloatGround 레이어를 마스크에 다시 추가 (충돌 무시 종료 시)
    public void ResumeFloatGroundLayer()
    {
        // _combinedMask에 FloatGround 레이어를 다시 추가 (해당 비트를 1로 만듦)
        _combinedMask |= (1 << _floatGroundLayerIndex);
    }

    private void FixedUpdate()
    {
        // 점프 중일 때는 땅 체크를 스킵 (선택적 로직)
        // 🚨 [변경] _playerMove -> _ref._Move
        if (_ref != null && _ref._Move != null && _ref._Move.isjump) return;

        facingDirection = transform.localScale.x > 0 ? 1 : -1;

        bool isGroundedThisFrame = false;
        isOnFloatGround = false;

        float halfSpacing = spacing * (RayCount - 1) / 2f;

        for (int i = 0; i < RayCount; i++)
        {
            float offsetX = ((i * spacing) - halfSpacing);

            Vector2 rayOrigin = (Vector2)transform.position
                              + new Vector2(rayOriginOffsetX * facingDirection, -rayOriginOffsetY)
                              + new Vector2(offsetX, 0f);

            // 레이캐스트 실행: 이제 _combinedMask가 PlatformDropRoutine에 의해 변경됩니다.
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, rayDistance, _combinedMask);

            if (hit.collider == null)
            {
                Debug.DrawRay(rayOrigin, Vector2.down * rayDistance, Color.green);
                continue;
            }

            isGroundedThisFrame = true;

            if (hit.collider.gameObject.layer == _floatGroundLayerIndex)
            {
                isOnFloatGround = true;
                Debug.DrawRay(rayOrigin, Vector2.down * rayDistance, Color.cyan);
            }
            else
            {
                Debug.DrawRay(rayOrigin, Vector2.down * rayDistance, Color.red);
            }
        }

        isGrounded = isGroundedThisFrame;

        // 🚨 [변경] _sync -> _ref._AnimSync
        if (_ref != null && _ref._AnimSync != null)
            _ref._AnimSync.IsGround(isGrounded);

        // 🚨 [변경] _playerMove -> _ref._Move
        if (isGrounded && _ref != null && _ref._Move != null)
            _ref._Move.ResetJumpCount();
    }
}