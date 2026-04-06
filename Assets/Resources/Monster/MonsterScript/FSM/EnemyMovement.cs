using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    private Rigidbody2D _rb;
    private EnemyAI _enemyAI; // 목표 위치나 속도 데이터를 가져오기 위한 뇌 참조
    private MonsterAnimatorController _ani;

    [Header("이동 설정")]
    [SerializeField] private float backStepForce = 1f;

    private Vector2 savedVelocity;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _enemyAI = GetComponent<EnemyAI>();
        _ani = GetComponent<MonsterAnimatorController>();
    }

    // 방향 전환
    public void FaceDirection(int dir)
    {
        if (dir == 0) return;
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * -dir;
        transform.localScale = scale;
    }

    // 플레이어 방향 보기
    public void FacetoPlayer()
    {
        if (_enemyAI.player == null) return;
        Vector2 direction = (_enemyAI.player - transform.position).normalized;
        if (Mathf.Abs(direction.x) > 0.01f)
            FaceDirection(direction.x > 0 ? 1 : -1);
    }

    // 플레이어 향해 걷기
    public void MoveTowardsPlayer()
    {
        if (_enemyAI.player == null) return;

        Vector2 direction = (_enemyAI.player - transform.position).normalized;
        if (Mathf.Abs(direction.x) > 0.01f)
            FaceDirection(direction.x > 0 ? 1 : -1);

        _rb.linearVelocity = new Vector2(direction.x * _enemyAI.currentMoveSpeed, _rb.linearVelocity.y);
    }

    // 특정 지점으로 이동 (전술용)
    public void MoveToTarget(Vector3 targetPos)
    {
        Vector2 direction = (targetPos - transform.position).normalized;
        if (Mathf.Abs(direction.x) > 0.01f)
            FaceDirection(direction.x > 0 ? 1 : -1);

        _rb.linearVelocity = new Vector2(direction.x * _enemyAI.currentMoveSpeed, _rb.linearVelocity.y);
    }

    // 제자리 정지
    public void StopMoving()
    {
        _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);
        _ani.SetMoving(false);
    }

    // 백스텝
    public void Backstep()
    {
        Vector2 dir = -(_enemyAI.player - transform.position).normalized;
        _rb.AddForce(dir * backStepForce, ForceMode2D.Impulse);
    }

    // 점프
    public void Jump()
    {
        _rb.AddForce(Vector2.up * 5f, ForceMode2D.Impulse);
    }

    // 공격 전 감속
    public void PrepareForAttack()
    {
        savedVelocity = _rb.linearVelocity;
        float slowFactor = Random.Range(0.2f, 0.6f);
        _rb.linearVelocity = new Vector2(savedVelocity.x * slowFactor, savedVelocity.y);
    }
}