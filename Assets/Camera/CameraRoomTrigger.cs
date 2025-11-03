using UnityEngine;

public class CameraRoomTrigger : MonoBehaviour
{
    // 플레이어가 이 트리거에 진입하면 해당 룸의 카메라 제한 범위를 설정
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 충돌한 오브젝트가 "Player" 태그를 가지고 있다면
        if (other.CompareTag("Player"))
        {
            // 메인 카메라에서 CameraFollow 컴포넌트 가져오기
            CameraFollow cam = Camera.main.GetComponent<CameraFollow>();
            // 이 오브젝트에 붙어있는 BoxCollider2D 가져오기 (룸의 범위)
            BoxCollider2D col = GetComponent<BoxCollider2D>();

            // 둘 다 유효하면 카메라에 현재 룸 범위를 전달
            if (cam && col)
            {
                cam.SetBounds(col);
            }
        }
    }
}
