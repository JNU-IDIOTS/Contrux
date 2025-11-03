using UnityEngine;

public class Elevator : MonoBehaviour
{
    [Header("엘리베이터 이동 설정")]
    public Transform topPoint;
    public Transform bottomPoint;
    public float speed = 3f;

    private Vector3 topPos;
    private Vector3 bottomPos;

    [SerializeField]private bool playerOnElevator = false;
    private Transform player;

    void Start()
    {
        if (topPoint == null || bottomPoint == null)
        {
            Debug.LogError("[Elevator] TopPoint 또는 BottomPoint가 연결되지 않았습니다.");
            return;
        }

        topPos = topPoint.position;
        bottomPos = bottomPoint.position;

        Debug.Log($"[Elevator Ready] Top: {topPos}, Bottom: {bottomPos}");
    }

    void Update()
    {
        if (!playerOnElevator || player == null) return;

        float step = speed * Time.deltaTime;
        float currentY = transform.position.y;

        // 위로 이동
        if (Input.GetKey(KeyCode.UpArrow) && currentY < topPos.y)
        {
            transform.position = new Vector3(transform.position.x, Mathf.MoveTowards(currentY, topPos.y, step), transform.position.z);
        }
        // 아래로 이동
        else if (Input.GetKey(KeyCode.DownArrow) && currentY > bottomPos.y)
        {
            transform.position = new Vector3(transform.position.x, Mathf.MoveTowards(currentY, bottomPos.y, step), transform.position.z);
        }
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        //Debug.Log($"[Elevator TriggerEnter] other={other.name}, tag={other.tag}, layer={LayerMask.LayerToName(other.gameObject.layer)}");
        if (other.CompareTag("Player"))
        {
            playerOnElevator = true;
            player = other.transform;
            //player.SetParent(transform);
            //Debug.Log("[Elevator] 플레이어 탑승");
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerOnElevator = true;
            player = other.transform;
            //player.SetParent(transform);
            //Debug.Log("[Elevator] 플레이어 탑승중");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            /*if (player != null)
            {
                player.SetParent(null);
                player = null;
            }*/
            playerOnElevator = false;
            //Debug.Log("[Elevator] 플레이어 하차");
        }
    }
}
