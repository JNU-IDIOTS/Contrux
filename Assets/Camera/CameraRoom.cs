using UnityEngine;

/*public class CameraRoom : MonoBehaviour
{
    // 플레이어가 이 룸의 트리거 범위에 진입했을 때 호출됨
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 충돌한 객체가 "Player" 태그가 아니라면 무시
        if (!other.CompareTag("Player")) return;

        // 메인 카메라에서 CameraFollow 스크립트를 찾음
        CameraFollow camFollow = Camera.main.GetComponent<CameraFollow>();

        // CameraFollow 스크립트가 존재하면
        if (camFollow != null)
        {
            // 이 오브젝트에 붙은 BoxCollider2D 컴포넌트를 가져와서
            BoxCollider2D roomBounds = GetComponent<BoxCollider2D>();
            // CameraFollow 스크립트에 현재 룸의 카메라 제한 영역을 설정함
            camFollow.SetBounds(roomBounds);
        }
    }
}*/
public class CameraRoom : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        CameraController cam = Camera.main.GetComponent<CameraController>();
        BoxCollider2D roomBounds = GetComponent<BoxCollider2D>();

        if (cam != null && roomBounds != null)
        {
            cam.SetRoomBounds(roomBounds);
            cam.SnapToTarget(); // 여기서 바로 스냅
        }

    }
}