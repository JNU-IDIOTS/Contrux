using System.Collections.Generic;
using UnityEngine;

public class Boss1_Jump : MonoBehaviour 
{
    public Rigidbody2D rb;
    public Boss1_FSM FSM;
    private bool isGrounded;

    private float previousY;
    public float increasedGravityScale = 10f;
    public float gravity = 1f;
    private LayerMask groundLayer;
    private bool isFallingToTarget = false;
    public Vector2 landingPosition;

    [SerializeField] private Transform detect;

    [SerializeField] private float rayOriginOffsetY = 0.1f;
    [SerializeField] private float rayDistance = 1.3f;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float defaultGravityScale = 3f;
    [SerializeField] private CircleCollider2D[] jumpAttackAreas;
    [SerializeField] private LayerMask playerLayer;

    public Boss1_Animation ani;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        ani = GetComponent<Boss1_Animation>();
        previousY = transform.position.y;

        if (detect == null)
        {
            var go = GameObject.FindWithTag("Player");
            if (go != null) detect = go.transform;
        }
    }
    private void FixedUpdate()
    {
        Vector2 rayOrigin = new Vector2(transform.position.x, transform.position.y - rayOriginOffsetY);
        
        // 바닥을 향한 Ray 발사
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, rayDistance, groundMask);

        // Scene 뷰에서 디버그 Ray 표시 (초록: 바닥 닿음, 빨강: 공중)
        Debug.DrawRay(rayOrigin, Vector2.down * rayDistance, hit.collider != null ? Color.green : Color.red);

        // 닿았는지 여부 판단
        bool onGround = (hit.collider != null);

        // 중력 조정
        if (onGround)
        {
            rb.gravityScale = defaultGravityScale;
            //if(rb.gravityScale ==  defaultGravityScale)
            //Debug.Log("기본 중력 (Ground와 닿아 있음)");
            
        }
        else
        {
            rb.gravityScale = 100f;
            float distanceX = Mathf.Abs(landingPosition.x - transform.position.x);
            Vector3 pos = transform.position;
            float targetX = detect.position.x;
            float targetY = detect.position.y;
            float xDir = Mathf.Sign(landingPosition.x - transform.position.x);
            float airMoveSpeed = 35f;
            if (distanceX <= 1f)
            {
                airMoveSpeed = 5f;
            }
            else if(distanceX <= 2.5f)
            {
                airMoveSpeed = 20f;
            }
            else if (distanceX <= 5f)
            {
                airMoveSpeed = 25f;
            }
            else if (distanceX < 8f)
            {
                airMoveSpeed = 35f;
            }
            else
            {
                airMoveSpeed = 70f;
            }
            rb.linearVelocity = new Vector2(xDir * airMoveSpeed, rb.linearVelocity.y);
            //Debug.Log("중력 증가 (Ground와 닿지 않음)");
            ani.JumpAttack();

        }
        /*if (isFallingToTarget && rb.linearVelocity.y < 0)  // 낙하 중일 때만
        {

            Vector3 pos = transform.position;
            float targetX = detect.position.x;

            float newX = Mathf.MoveTowards(pos.x, targetX, 300f * Time.deltaTime); // 5f는 x이동 속도
            transform.position = new Vector3(newX, pos.y, pos.z);
        }*/

    }

    public void Jump(Transform target, float jumpHeight)
    {
        float gravity = Physics2D.gravity.y * 3f;
        float vy = Mathf.Sqrt(6 * Mathf.Abs(gravity) * jumpHeight);
        int dir = 0;
        landingPosition = target.position;
        float timeToApex = vy / Mathf.Abs(gravity);
        float totalTime = timeToApex * 2;

        FSM.FindTarget();
        Debug.Log("Jump() 호출됨");
        ani.Jump();
        Rigidbody2D rb = FSM.GetComponent<Rigidbody2D>();
        rb.linearVelocity = new Vector2(0, vy);

        if (FSM.target == null || rb == null)
        {
            //Debug.LogWarning("target 이 null이라 점프 안 함");
            return;
        }

        Vector2 startPos = FSM.transform.position;
        Vector2 targetPos = FSM.target.position;
        float vx = (landingPosition.x - startPos.x) / totalTime;
        //Debug.Log($"vx : {vx}");
        float Ivx = (targetPos.x - startPos.x) / totalTime;
        if (Ivx <= 0)
        {
            dir = -1;
        }
        else
        {
            dir = 1;
        }

        FSM.FaceDirection(dir);

        rb.linearVelocity = new Vector2(vx, vy);
        //Debug.Log("점프 벡터: " + rb.linearVelocity);
        DealJumpAttackDamage();
    }

    private void DealJumpAttackDamage()
    {
        HashSet<PlayerHealth> damaged = new HashSet<PlayerHealth>();

        foreach (var area in jumpAttackAreas)
        {
            Vector2 center = area.bounds.center;
            float radius = area.radius * area.transform.lossyScale.x;
            Collider2D[] hits = Physics2D.OverlapCircleAll(center, radius, playerLayer);

            foreach (var hit in hits)
            {
                if (hit.CompareTag("Player") && hit.TryGetComponent(out PlayerHealth player) && !damaged.Contains(player))
                {
                    if (!Boss1_FSM.cphase2)
                    {
                        player.TakeDamage(10, transform, 0f, 0f);
                        Debug.Log("1페 딜");
                    }
                    else
                    {
                        player.TakeDamage(10, transform, 0f, 0f);
                        Debug.Log("2페 딜");
                    }
                    damaged.Add(player);
                }
            }
        }
    }
    //-----------------------------------------------------------

    //------------------------------------------------------------
    // 착지 판정
    private void OnTriggerEnter2D(Collider2D other) 
    {
        if (other.gameObject.CompareTag("Ground") && !isGrounded)
        {
            Debug.Log("착지 트리거 감지: 1회만 실행됨");
            isGrounded = true;

            // 여기에 실행할 동작 (예: velocity 멈춤 등)
            rb.linearVelocity = Vector2.zero;
        }

        if (other.CompareTag("Ground"))
        {
            isFallingToTarget = false;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            // 땅에서 떨어지면 다시 착지 감지 가능하도록 초기화
            isGrounded = false;

        }
    }

    private void OnDrawGizmosSelected() // 시각화
    {
        // 씬뷰에 레이캐스트 궤적 표시
        Gizmos.color = Color.green;
        Vector2 gizmoOrigin = new Vector2(transform.position.x, transform.position.y - rayOriginOffsetY);
        Gizmos.DrawRay(gizmoOrigin, Vector2.down * rayDistance);
    }
}
