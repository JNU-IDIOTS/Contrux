using UnityEngine;
using System.Collections;

public class EnemyCombat : MonoBehaviour
{
    private EnemyAI _enemyAI;
    private MonsterAnimatorController _ani;
    private ObjectActive _objectactive;

    private Transform _attackPoint;
    private LayerMask _playerLayer;

    [Header("버스터 공격(특수) 설정")]
    public float busterChasingTime = 0.5f;
    public float busterAttackSpeed = 10f;

    private void Awake()
    {
        _enemyAI = GetComponent<EnemyAI>();
        _ani = GetComponent<MonsterAnimatorController>();
        _objectactive = GetComponent<ObjectActive>();

        _playerLayer = LayerMask.GetMask("Player");
        _attackPoint = transform.Find("AttackArea");

        if (_attackPoint == null)
        {
            Debug.LogWarning($"[{gameObject.name}] 자식 오브젝트 중에 'AttackArea'가 없습니다! 프리팹 구조를 확인해주세요.");
        }
    }

    public void NAttackDealDamage()
    {
        if (_attackPoint == null) return;
        DealAreaDamage(_attackPoint.position, _enemyAI.speciesData.attackRange, _enemyAI.speciesData.NAttackDamage);
    }

    public void SAttackDealDamage()
    {
        if (_attackPoint == null) return;
        DealAreaDamage(_attackPoint.position, _enemyAI.speciesData.attackRange, _enemyAI.speciesData.SAttackDamage);
    }

    public void DealAreaDamage(Vector2 point, float radius, int damage)
    {
        float finalDamage = damage;

        // 무전기(Squad)에서 리더 정보 가져옴
        if (_enemyAI._enemySquad.currentLeader != null && !_enemyAI._enemySquad.currentLeader._enemyHealth.isDeath)
        {
            finalDamage *= _enemyAI.speciesData.leaderBuffDamageMultiplier;
        }

        Collider2D attackCol = _attackPoint.GetComponent<Collider2D>();
        Collider2D[] hits = new Collider2D[0];

        if (attackCol != null)
        {
            if (attackCol is BoxCollider2D box)
                hits = Physics2D.OverlapBoxAll(box.bounds.center, box.bounds.size, box.transform.eulerAngles.z, _playerLayer);
            else if (attackCol is CapsuleCollider2D capsule)
                hits = Physics2D.OverlapCapsuleAll(capsule.bounds.center, capsule.size, capsule.direction, capsule.transform.eulerAngles.z, _playerLayer);
            else if (attackCol is CircleCollider2D circle)
            {
                float realRadius = circle.radius * Mathf.Max(circle.transform.lossyScale.x, circle.transform.lossyScale.y);
                hits = Physics2D.OverlapCircleAll(circle.bounds.center, realRadius, _playerLayer);
            }
        }
        else
        {
            hits = Physics2D.OverlapCircleAll(point, radius, _playerLayer);
        }

        foreach (var hit in hits)
        {
            if (hit.TryGetComponent(out PlayerHealth playerHealth))
            {
                if (attackCol is CircleCollider2D && _enemyAI.speciesData.attackAngle < 360f)
                {
                    Vector2 facingDir = transform.localScale.x < 0 ? Vector2.left : Vector2.right;
                    Vector2 dirToTarget = (hit.transform.position - transform.position).normalized;
                    float angleToTarget = Vector2.Angle(facingDir, dirToTarget);

                    if (angleToTarget > _enemyAI.speciesData.attackAngle * 0.5f) continue;
                }

                playerHealth.TakeDamage((int)finalDamage);
            }
        }
    }

    public void StartBusterCharge()
    {
        StartCoroutine(BusterToPlayer());
    }

    private IEnumerator BusterToPlayer()
    {
        if (_objectactive != null) _objectactive.StarActivateTarget();

        yield return new WaitForSeconds(busterChasingTime);

        if (_enemyAI.player != null)
        {
            Vector2 direction = (_enemyAI.player - transform.position).normalized;
            if (Mathf.Abs(direction.x) > 0.01f)
                _enemyAI._enemyMovement.FaceDirection(direction.x > 0 ? 1 : -1);

            if (_ani != null) _ani.SAttack();
            _enemyAI.GetRigidbody().AddForce(new Vector2(direction.x * busterAttackSpeed, _enemyAI.GetRigidbody().linearVelocity.y), ForceMode2D.Impulse);
        }

        _enemyAI.StartCooldown(5f);
    }
}