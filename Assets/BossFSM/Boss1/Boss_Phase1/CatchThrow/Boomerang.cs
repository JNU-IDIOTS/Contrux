using UnityEngine;

public class Boomerang : MonoBehaviour
{
    public Boss1_FSM FSM;
    public Boss1_Animation ani;

    [Header("Movement Settings")]
    public float speed = 100f;

    public Vector2 origin;
    private Vector2 direction;
    private bool returning = false;
    [SerializeField] private CircleCollider2D pillarAttack;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (!rb)
        {
            Debug.LogError("Boomerang: Rigidbody2D가 없습니다.");
        }
    }

    // FSM 정보 초기화
    public void Init(Boss1_FSM boss)
    {
        FSM = boss;
        ani = boss.GetComponent<Boss1_Animation>();
        if (!ani)
        {
            Debug.LogError("Boomerang: Boss1_Animation을 찾을 수 없습니다.");
        }
    }

    // 위치 및 방향 초기화
    public void Init(Vector2 Boomerang_startPos, Vector2 throwDir)
    {
        origin = Boomerang_startPos;
        direction = throwDir.normalized;
        transform.position = origin;

       // Debug.Log($"Boomerang 초기화 완료 | 방향: {direction}, 거리: {Vector2.Distance(origin, origin + direction)}");
    }

    private void Update()
    {
        if (!rb) return;

        Vector2 moveDir = returning ? (origin - rb.position).normalized : direction;
        rb.MovePosition(rb.position + moveDir * speed * Time.fixedDeltaTime);

        float dist = Vector2.Distance(rb.position, origin);

        if (returning && dist < 0.1f)
        {
            if (ani != null)
                ani.CatchBoomerang();

            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!returning)
        {
            // 플레이어면 데미지
            if (other.CompareTag("Player") && other.TryGetComponent(out PlayerHealth player))
            {
                player.TakeDamage(10, transform, 0f, 0f);
            }
            FSM.DamagePillarsInRange(pillarAttack, 1);
        }
            // 특정 태그에 부딪혔을 때 복귀 시작
            if (!returning && other.CompareTag("Tag"))
        {
            returning = true;
           // Debug.Log("Boomerang: 충돌 감지됨 → 복귀 시작");
        }
    }
}
