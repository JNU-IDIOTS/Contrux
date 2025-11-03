// 완전 깔끔한 원래 버전으로 복원 (cameraOffset 제거됨)
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class CameraController : MonoBehaviour
{
    //[SerializeField] private float pixelsPerUnit = 512f;

    public Transform steampunktarget;    // ← 수정됨
    public Transform magictarget;        // ← 수정됨
    private Vector3 target;     // ← 수정됨
    [SerializeField] private float CamaraYmaius = 0f;

    public float smoothSpeed = 0.08f;

    private Vector2 minBounds;
    private Vector2 maxBounds;
    private Vector3 velocity = Vector3.zero;
    private float camHalfWidth;
    private float camHalfHeight;

    private bool Steampunk = false;
    private bool Magic = false;
    private bool Select = false;

    [SerializeField] private ChooseOne chooseone;

    void Start()
    {

        Camera cam = Camera.main;
        camHalfHeight = cam.orthographicSize;
        camHalfWidth = camHalfHeight * cam.aspect;
    }
    void FixedUpdate()
    {
        if (!Select) return;

        // 타겟 위치 + 오프셋 계산
        if (Steampunk) target = steampunktarget.position;
        else if (Magic) target = magictarget.position;

        Vector3 offset = new Vector3(0f, CamaraYmaius, 0f);
        Vector3 targetWithOffset = target + offset;
        float clampedX = Mathf.Clamp(targetWithOffset.x, minBounds.x, maxBounds.x);
        float clampedY = Mathf.Clamp(targetWithOffset.y, minBounds.y, maxBounds.y);
        //Vector3 targetPos = target;  // ← 수정됨
        //float clampedX = Mathf.Clamp(targetPos.x, minBounds.x, maxBounds.x);
        //float clampedY = Mathf.Clamp(targetPos.y, minBounds.y, maxBounds.y);
        Vector3 desiredPos = new Vector3(clampedX, clampedY, transform.position.z);

        transform.position = Vector3.SmoothDamp(transform.position, desiredPos, ref velocity, smoothSpeed);
    }

    public void SetRoomBounds(BoxCollider2D roomBounds)
    {
        Bounds bounds = roomBounds.bounds;
        minBounds = new Vector2(bounds.min.x + camHalfWidth, bounds.min.y + camHalfHeight);
        maxBounds = new Vector2(bounds.max.x - camHalfWidth, bounds.max.y - camHalfHeight);

        // 위치 스냅 (카메라 점프 + 패럴럭스 sync)
        Vector3 targetPos = target;
        float clampedX = Mathf.Clamp(targetPos.x, minBounds.x, maxBounds.x);
        float clampedY = Mathf.Clamp(targetPos.y, minBounds.y, maxBounds.y);
        Vector3 snappedPos = new Vector3(clampedX, clampedY, transform.position.z);
        transform.position = snappedPos;
        velocity = Vector3.zero;

        // 패럴럭스 레이어들 초기화
        /*foreach (var p in FindObjectsOfType<ParallaxLayer>())
        {
            p.ResetParallax(snappedPos);
        }*/
    }

    public void SnapToTarget()
    {
        Vector3 targetPos = target;
        float clampedX = Mathf.Clamp(targetPos.x, minBounds.x, maxBounds.x);
        float clampedY = Mathf.Clamp(targetPos.y, minBounds.y, maxBounds.y);
        Vector3 snappedPos = new Vector3(clampedX, clampedY, transform.position.z);

        transform.position = snappedPos;
        velocity = Vector3.zero;

        // 패럴럭스 초기화
        /*foreach (var parallax in FindObjectsOfType<ParallaxLayer>())
        {
            parallax.ForceSyncWithCamera(snappedPos);
        }*/
    }

    public void SetSteamPunktype()
    {
        Steampunk = true;
        target = steampunktarget.position;
        Select = true;
    }

    public void SetMagictype()
    {
        Magic = true;
        target = magictarget.position;
        Select = true;
    }
}
