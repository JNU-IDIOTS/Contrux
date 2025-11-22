using UnityEngine;

// 이 스크립트는 메인 카메라 오브젝트에 부착됩니다.
// Rigidbody 2D나 Collider 2D가 필요 없습니다.
public class CameraFollower : MonoBehaviour
{
    [Header("Target & Speed")]
    [Tooltip("카메라가 따라갈 플레이어 Transform")]
    public Transform target;
    [Tooltip("카메라 추적 속도 (0에 가까울수록 느림)")]
    public float followSpeed = 5f;

    [Header("Boundary Transition")]
    [Tooltip("경계 전환 시 부드럽게 목표 위치로 이동하는 시간")]
    public float smoothTransitionTime = 0.5f;

    // --- 경계 상태 변수 ---
    [Header("Boundary State")]
    [Tooltip("현재 카메라의 움직임을 제한하는 경계 영역")]
    public CameraBoundary currentBoundary;

    [Tooltip("게임 시작 시 기본으로 사용할 카메라 경계 (옵션)")]
    public CameraBoundary defaultBoundary;

    private bool _isTransitioning = false;
    private Vector3 _transitionStartPos;
    private Vector3 _transitionEndPos;
    private float _transitionTimer;

    // 카메라 컴포넌트 참조 (자동 할당)
    private Camera _mainCamera; // 🚨 컴포넌트 참조 변수명 규칙 적용
    private float _cameraHalfHeight;
    private float _cameraHalfWidth;

    private void Awake()
    {
        // 🚨 컴포넌트 자동 할당
        _mainCamera = GetComponent<Camera>();

        if (_mainCamera == null)
        {
            Debug.LogError("CameraFollower requires a Camera component.");
            enabled = false;
            return;
        }

        UpdateCameraSize();

        // 🚨 Target 자동 할당 (인스펙터 할당이 없을 경우)
        if (target == null)
        {
            GameObject playerObject = GameObject.FindWithTag("Player");
            if (playerObject != null)
            {
                target = playerObject.transform;
            }
        }

        // 🚨 기본 경계가 설정되어 있고, 현재 경계가 비어 있을 경우 자동 적용
        if (currentBoundary == null && defaultBoundary != null)
        {
            SetCurrentBoundary(defaultBoundary);
        }
    }

    private void UpdateCameraSize()
    {
        _cameraHalfHeight = _mainCamera.orthographicSize;
        _cameraHalfWidth = _cameraHalfHeight * _mainCamera.aspect;
    }

    private void FixedUpdate()
    {
        if (target == null) return;

        Vector3 targetPosition = target.position;
        targetPosition.z = transform.position.z; // Z축은 유지

        Vector3 newPosition;

        // 🚨 1. 경계 전환 중일 때 (튀는 현상 방지)
        if (_isTransitioning)
        {
            _transitionTimer += Time.fixedDeltaTime;
            float t = Mathf.Clamp01(_transitionTimer / smoothTransitionTime);

            newPosition = Vector3.Lerp(_transitionStartPos, _transitionEndPos, t);

            if (t >= 1f)
            {
                _isTransitioning = false;
                _transitionTimer = 0f;
            }
        }
        // 🚨 2. 평상시 추적 중일 때
        else
        {
            // 부드럽게 목표 위치를 향해 추적합니다.
            newPosition = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.fixedDeltaTime);
        }

        // 3. 경계 제한 (Clamping) 적용
        if (currentBoundary != null)
        {
            newPosition = ClampCameraPosition(newPosition, currentBoundary.Bounds);
        }

        transform.position = newPosition;
    }

    // 🚨 [Public 메서드]: CameraBoundary가 플레이어 진입 시 호출
    public void SetCurrentBoundary(CameraBoundary newBoundary)
    {
        // 새 경계를 설정하기 전에 부드러운 전환 시작
        if (newBoundary != currentBoundary)
        {
            // 전환 목표 위치 계산
            Vector3 targetClamped = target.position; // 플레이어 위치가 기준
            targetClamped.z = transform.position.z;

            // 🚨 새 경계의 제한을 미리 적용하여 EndPos를 계산합니다.
            // 이렇게 하면 전환이 시작될 때 카메라가 바로 경계 안쪽으로 들어오게 됩니다.
            if (newBoundary != null)
            {
                targetClamped = ClampCameraPosition(targetClamped, newBoundary.Bounds);
            }

            // 부드러운 전환 시작 설정
            _isTransitioning = true;
            _transitionTimer = 0f;
            _transitionStartPos = transform.position;
            _transitionEndPos = targetClamped; // 🚨 플레이어 위치를 기반으로 클램프된 지점이 최종 목표가 됩니다.

            currentBoundary = newBoundary;

            Debug.Log($"카메라 경계 설정: {newBoundary.gameObject.name} (전환 시작)");
        }
    }

    // 🚨 [Public 메서드]: CameraBoundary가 플레이어 이탈 시 호출
    public void ClearCurrentBoundary(CameraBoundary boundaryToClear)
    {
        // 이탈한 경계가 현재 경계와 일치할 때만 해제합니다.
        if (currentBoundary == boundaryToClear)
        {
            // (이탈 시에는 전환 애니메이션 없이 즉시 null로 해제합니다.)
            currentBoundary = null;
            Debug.Log($"카메라 경계 이탈: {boundaryToClear.gameObject.name}");
        }
    }

    // 카메라 위치에 경계 제한을 적용하는 메서드 (메서드명 규칙 적용)
    private Vector3 ClampCameraPosition(Vector3 position, Bounds bounds)
    {
        // 경계 박스 내에서 카메라가 움직일 수 있는 최소/최대 좌표를 계산합니다.
        float minX = bounds.min.x + _cameraHalfWidth;
        float maxX = bounds.max.x - _cameraHalfWidth;
        float minY = bounds.min.y + _cameraHalfHeight;
        float maxY = bounds.max.y - _cameraHalfHeight;

        // 🔹 방 크기가 카메라보다 작은 경우: 중앙에 고정되도록 보정
        if (minX > maxX)
        {
            float midX = (bounds.min.x + bounds.max.x) * 0.5f;
            minX = maxX = midX;
        }
        if (minY > maxY)
        {
            float midY = (bounds.min.y + bounds.max.y) * 0.5f;
            minY = maxY = midY;
        }

        // 경계 제한을 적용합니다.
        position.x = Mathf.Clamp(position.x, minX, maxX);
        position.y = Mathf.Clamp(position.y, minY, maxY);

        return position;
    }
}
