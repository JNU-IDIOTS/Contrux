using UnityEngine;

// 이 스크립트를 경계 역할을 할 Composite Collider 2D 오브젝트에 붙입니다.
// 이 오브젝트에는 Composite Collider 2D (Is Trigger)가 필요합니다.
public class CameraBoundary : MonoBehaviour
{
    // 🚨 BoxCollider2D 대신 CompositeCollider2D를 참조합니다.
    private CompositeCollider2D _compositeCollider2D; // 🚨 컴포넌트 참조 변수명 규칙 적용
    private CameraFollower _cameraFollower; // 🚨 카메라 참조 변수명 규칙 적용

    public Bounds Bounds { get; private set; } // 이 경계의 실제 영역

    private void Awake()
    {
        // 🚨 GetComponent를 이용한 CompositeCollider2D 자동 할당
        _compositeCollider2D = GetComponent<CompositeCollider2D>();

        if (_compositeCollider2D == null)
        {
            Debug.LogError("CameraBoundary requires a CompositeCollider2D component on its GameObject.");
            // 🚨 이 로그는 이제 불필요하거나, 아래 성공 로그로 대체됩니다.
            // Debug.Log($"[Awake] Boundary Check: Composite Collider Found: {_compositeCollider2D != null}. CameraFollower Found: {_cameraFollower != null}"); 
            enabled = false;
            return;
        }

        // 🚨 CameraFollower 자동 찾기 (씬 전체에서 한 개만 있다고 가정)
        _cameraFollower = UnityEngine.Object.FindFirstObjectByType<CameraFollower>();

        // 🚨 [수정된 로그 위치] 성공적으로 컴포넌트를 찾은 후에 로그를 출력합니다.
        Debug.Log($"[Awake] Boundary Check: Composite Collider Found: True. CameraFollower Found: {_cameraFollower != null}");


        if (_cameraFollower == null)
        {
            Debug.LogError("CameraBoundary cannot find CameraFollower in the scene.");
        }

        // 월드 좌표계 기준의 실제 경계 영역을 계산하여 저장합니다.
        Bounds = _compositeCollider2D.bounds;
    }

    // 🚨 [핵심: 플레이어 진입 감지]
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 🚨 2. 어떤 콜라이더든 진입 시 로그가 뜨는지 확인합니다.
        Debug.Log($"[Trigger Test] Collider Entered by: {other.gameObject.name} (Layer: {other.gameObject.layer})");

        if (other.CompareTag("Player") && _cameraFollower != null)
        {
            // 🚨 3. 플레이어 태그 확인 및 최종 호출 직전 로그를 확인합니다.
            Debug.Log($"[SUCCESS] Player entered boundary: {gameObject.name}");
            _cameraFollower.SetCurrentBoundary(this);
        }
    }

    // 🚨 [핵심: 플레이어 이탈 감지]
    private void OnTriggerExit2D(Collider2D other)
    {
        // 플레이어 오브젝트에 붙은 콜라이더인지 태그로 확인
        if (other.CompareTag("Player") && _cameraFollower != null)
        {
            // 카메라 팔로우에게 이 경계를 해제하라고 요청합니다.
            _cameraFollower.ClearCurrentBoundary(this);
        }
    }
}