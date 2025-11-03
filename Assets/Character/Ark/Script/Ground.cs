//using UnityEngine;

//public class Ground : MonoBehaviour
//{
//    [Header("지면 검사 (레이)")]
//    [SerializeField] private LayerMask groundMask;           // 지면 레이어 설정
//    [SerializeField] public float rayDistance = 1.3f;        // 레이 길이 (짧게 설정)
//    [SerializeField] public float rayOriginOffsetY = 0f;   // transform.position.y 에서 아래로 내릴 오프셋
//    [SerializeField] public float rayOriginOffsetX = 0f;   // transform.position.y 에서 아래로 내릴 오프셋

//    private int facingDir;
//    public float spacing = 0.2f;
//    public bool isGrounded;
//    public bool onGround;
//    private bool prevGrounded = false;
//    public bool isGroundedNow = false;

//    private PlayerAnimatorController animController;
//    private PlayerAnimationSync sync;
//    private Player_move Player_move;

//    private void Awake()
//    {
//        animController = GetComponentInParent<PlayerAnimatorController>();
//        Player_move = GetComponentInParent<Player_move>();
//        sync = GetComponentInParent<PlayerAnimationSync>();
//    }

//    private void Update()
//    {
//        //Debug.Log($"IsGrounded: {isGrounded}");
//    }

//    private void FixedUpdate()
//    {
//        if (Player_move.isjump) return;

//        facingDir = transform.localScale.x > 0 ? 1 : -1;

//        int rayCount = 3;
//        float halfSpacing = spacing * (rayCount - 1) / 2f;

//        isGroundedNow = false;

//        for (int i = 0; i < rayCount; i++)
//        {
//            float offsetX = ((i * spacing) - halfSpacing) * facingDir;
//            Vector2 rayOrigin = (Vector2)transform.position
//                                + new Vector2(rayOriginOffsetX * facingDir, -rayOriginOffsetY)
//                                + new Vector2(offsetX, 0f);

//            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, rayDistance, groundMask);

//            if (hit.collider != null)
//            {
//                isGroundedNow = true;
//            }

//            Debug.DrawRay(rayOrigin, Vector2.down * rayDistance, isGroundedNow ? Color.red : Color.green);
//        }

//        isGrounded = isGroundedNow;
//        sync.IsGround(isGroundedNow);
//        /*if (isGroundedNow && !prevGrounded || !Player_move.isjump)
//        {
//            sync.DoubleJump();
//            //Player_move.ResetJumpCount();
//        }*/
//        if (isGroundedNow)
//        {
//            Player_move.ResetJumpCount();
//        }
//        prevGrounded = isGroundedNow;
//    }

//    private void OnDrawGizmosSelected()
//    {
//        Gizmos.color = Color.green;
//        Vector2 gizmoOrigin = (Vector2)transform.position + new Vector2(rayOriginOffsetX * facingDir, -rayOriginOffsetY);
//        Gizmos.DrawRay(gizmoOrigin, Vector2.down * rayDistance);
//    }
//}

using UnityEngine;

public class Ground : MonoBehaviour
{
    [Header("지면 검사 (레이)")]
    [SerializeField] private LayerMask groundMask;           // 일반 지면 마스크
    [SerializeField] private string floatGroundLayerName = "FloatGround"; // ← 이름으로 고정
    [SerializeField] public float rayDistance = 1.3f;
    [SerializeField] public float rayOriginOffsetY = 0f;
    [SerializeField] public float rayOriginOffsetX = 0f;

    private int floatGroundLayerIndex;   // ← 인덱스 캐싱
    private int combinedMask;            // ← groundMask + FloatGround

    private int facingDir;
    public float spacing = 0.2f;
    public bool isGrounded;
    public bool onGround;
    private bool prevGrounded = false;
    public bool isGroundedNow = false;
    public bool floatGround = false;

    private PlayerAnimationSync sync;
    private Player_move Player_move;

    private void Awake()
    {
        Player_move = GetComponentInParent<Player_move>();
        sync = GetComponentInParent<PlayerAnimationSync>();

        // FloatGround 레이어 인덱스 확보
        floatGroundLayerIndex = LayerMask.NameToLayer(floatGroundLayerName);
        if (floatGroundLayerIndex < 0)
            Debug.LogError($"레이어 '{floatGroundLayerName}'를 찾을 수 없습니다. Project Settings > Tags and Layers에서 확인하세요.");

        // 레이용 마스크: 일반 groundMask + FloatGround를 합침
        combinedMask = groundMask.value | (1 << floatGroundLayerIndex);
    }

    private void FixedUpdate()
    {
        if (Player_move.isjump) return;

        facingDir = transform.localScale.x > 0 ? 1 : -1;

        const int rayCount = 3;
        float halfSpacing = spacing * (rayCount - 1) / 2f;

        // 프레임 시작에 리셋
        isGroundedNow = false;
        floatGround = false;

        for (int i = 0; i < rayCount; i++)
        {
            float offsetX = ((i * spacing) - halfSpacing) * facingDir;
            Vector2 rayOrigin = (Vector2)transform.position
                              + new Vector2(rayOriginOffsetX * facingDir, -rayOriginOffsetY)
                              + new Vector2(offsetX, 0f);

            // ★ FloatGround도 맞추기 위해 combinedMask 사용
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, rayDistance, combinedMask);

            if (!hit.collider)
            {
                Debug.DrawRay(rayOrigin, Vector2.down * rayDistance, Color.green);
                continue;
            }

            isGroundedNow = true;

            // ★ 오직 FloatGround일 때만 true
            if (hit.collider.gameObject.layer == floatGroundLayerIndex)
                floatGround = true;

            Debug.DrawRay(rayOrigin, Vector2.down * rayDistance, floatGround ? Color.cyan : Color.red);
        }

        isGrounded = isGroundedNow;
        sync.IsGround(isGroundedNow);

        if (isGroundedNow)
            Player_move.ResetJumpCount();

        prevGrounded = isGroundedNow;
    }
}