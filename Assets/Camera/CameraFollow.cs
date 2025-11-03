using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform steampunktarget;                   // 따라다닐 대상 (보통 플레이어)
    public Transform magictarget;                   // 따라다닐 대상 (보통 플레이어)
    private Vector3 target;

    public float smoothSpeed = 0.125f;         // 카메라 이동 부드러움 정도
    public float airYOffset = 1f;              // 공중에 있을 때 위로 이동할 오프셋
    public float groundYOffset = -1f;          // 지상에 있을 때 아래로 이동할 오프셋
    private float camHalfWidth;                // 카메라 반 너비 (화면 가로 절반)
    private float camHalfHeight;               // 카메라 반 높이 (화면 세로 절반)
    private Vector2 minBounds;                 // 카메라 이동 최소 좌표
    private Vector2 maxBounds;                 // 카메라 이동 최대 좌표
    private float currentYOffset;              // 현재 Y축 오프셋

    private bool Steampunk = false;
    private bool Magic = false;
    private bool Select = false;
        
    [SerializeField] private float CamaraYmaius = 0f;
    [HideInInspector] public bool isGrounded = true; // 플레이어의 땅에 닿았는지 여부 (외부에서 설정)
    [SerializeField] private ChooseOne chooseone;

    void Start()
    {
        // 카메라 크기 계산 (Orthographic 기준)
        Camera cam = Camera.main;
        camHalfHeight = cam.orthographicSize;
        camHalfWidth = camHalfHeight * Screen.width / (float)Screen.height;
    }

    void LateUpdate()
    {
        //if (target == null) return;

        if(!Select) return;

        // 땅에 닿았는지 여부에 따라 Y 오프셋을 부드럽게 변경
        float desiredYOffset = isGrounded ? groundYOffset : airYOffset;
        currentYOffset = Mathf.Lerp(currentYOffset, desiredYOffset, Time.deltaTime * 3f);

        // 타겟 위치 + 오프셋 계산
        if(Steampunk) target = steampunktarget.position;
        else if (Magic) target = magictarget.position;

        Vector3 targetPos = target + new Vector3(0, currentYOffset - CamaraYmaius, 0);

        // 제한 범위 내에서 위치 고정 (Clamp)
        float clampedX = Mathf.Clamp(targetPos.x, minBounds.x, maxBounds.x);
        float clampedY = Mathf.Clamp(targetPos.y, minBounds.y, maxBounds.y);

        // 최종 위치로 부드럽게 이동
        Vector3 finalPos = new Vector3(clampedX, clampedY, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, finalPos, smoothSpeed);
    }

    // 룸 범위(BoxCollider2D 기준)를 받아 카메라 이동 범위 설정
    public void SetBounds(BoxCollider2D roomBounds)
    {
        Bounds bounds = roomBounds.bounds;

        // 카메라가 룸 바깥으로 나가지 않도록 보정된 최소/최대 좌표 계산
        minBounds = new Vector2(bounds.min.x + camHalfWidth, bounds.min.y + camHalfHeight);
        maxBounds = new Vector2(bounds.max.x - camHalfWidth, bounds.max.y - camHalfHeight);
    }

    public void SteamPunktype()
    {
        Steampunk = true;
        target = steampunktarget.position;
        Select = true;
    }

    public void Magictype()
    {
        Magic = true;
        target = magictarget.position;
        Select = true;
    }
}
