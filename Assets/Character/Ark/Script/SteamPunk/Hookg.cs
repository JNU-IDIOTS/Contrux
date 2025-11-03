using UnityEngine;

public class Hookg : MonoBehaviour
{
    private Hook Grappling;
    // public DistanceJoint2D joint2D;  // 이제 사용 안 하므로 제거 가능

    private void Start()
    {
        Grappling = GameObject.Find("Ark/SteamPunk").GetComponent<Hook>();
        // joint2D = GetComponent<DistanceJoint2D>();
        // joint2D.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("RING"))
        {
            // 갈고리가 Ring 위치에 걸렸음을 Hook에게 알림
            Grappling.isAttachReady = true;
            Grappling.isHookActive = false;
            Grappling.isLineMax = false;

            // joint2D 활성화는 더 이상 사용하지 않습니다.
        }
    }
}
