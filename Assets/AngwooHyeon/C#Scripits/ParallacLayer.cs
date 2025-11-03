using UnityEngine;
using UnityEngine.SceneManagement;

[ExecuteAlways]
public class ParallaxLayer : MonoBehaviour
{
    public Transform cameraTransform;
    public float parallaxFactor = 0.05f;

    private Vector3 startPosition;
    private Vector3 cameraStartPosition;
    private float initialZ; // 👈 Z값 고정용

    void Start()
    {
        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;

        startPosition = transform.position;
        cameraStartPosition = cameraTransform.position;
        initialZ = startPosition.z; // ✅ z값 저장
    }
    void Awake() // ✅ Start보다 더 확실한 타이밍
    {
        // 에디터에 드래그할 필요 없이, 언제나 이 카메라를 참조
        cameraTransform = CameraDontDestroy.Instance.CameraTransform;
        startPosition = transform.position;
        cameraStartPosition = cameraTransform.position;
        initialZ = startPosition.z;
    }


    public void ForceSyncWithCamera(Vector3 camPos)
    {
        cameraStartPosition = camPos;
        startPosition = transform.position;
        initialZ = startPosition.z;
    }

    void FixedUpdate()
    {
        if (cameraTransform == null) return;

        Vector3 cameraDelta = cameraTransform.position - cameraStartPosition;

        Vector3 offset = new Vector3(
            cameraDelta.x * parallaxFactor,
            cameraDelta.y * parallaxFactor,
            0f // ✅ Z는 여기서 안 건드림
        );

        Vector3 newPos = startPosition + offset;
        newPos.z = initialZ; // ✅ 항상 원래 z 유지
        transform.position = newPos;
    }

    public void ResetParallax(Vector3 camPos)
    {
        cameraStartPosition = camPos;
        startPosition = transform.position;
        initialZ = startPosition.z;
    }
    void OnEnable()
    {
        SceneManager.sceneLoaded += (s, m) =>
        {
            // 씬 전환 시에도 카메라 참조를 다시 잡아 줌
            cameraTransform = CameraDontDestroy.Instance.CameraTransform;
            ForceSyncWithCamera(cameraTransform.position);
        };
    }
}
