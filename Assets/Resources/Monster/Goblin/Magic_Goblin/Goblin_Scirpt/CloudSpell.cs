using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CloudSpell : MonoBehaviour
{
    [Tooltip("구름 애니메이션 이벤트에서 사용할 데미지 양")]
    public int damageAmount = 2;

    [Header("넉백 범위 설정")]
    [SerializeField] private float minKnockbackForce = 1f;
    [SerializeField] private float maxKnockbackForce = 3f;
    [SerializeField] private float minKnockbackUpwardForce = 0.5f;
    [SerializeField] private float maxKnockbackUpwardForce = 2f;

    // 트리거에 들어온 데미지 가능 대상들
    private readonly HashSet<IDamageable> _targetsInRange = new HashSet<IDamageable>();

    // Collider 참조
    private Collider2D _col;

    private void Awake()
    {
        _col = GetComponent<Collider2D>();
        if (_col == null)
        {
            Debug.LogError("[CloudSpell] Collider2D가 필요합니다.", this);
            return;
        }
        _col.isTrigger = true;
        _col.enabled = false;           // 평소엔 꺼둠
        StartCoroutine(AutoDestroy());
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!_col.enabled) return;

        // IDamageable을 구현한 컴포넌트가 있으면 수집
        if (other.TryGetComponent<IDamageable>(out var dmg))
            _targetsInRange.Add(dmg);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!_col.enabled) return;
        if (other.TryGetComponent<IDamageable>(out var dmg))
            _targetsInRange.Remove(dmg);
    }

    /// <summary>
    /// 애니메이션 이벤트로 호출: 콜라이더 활성화
    /// </summary>
    public void EnableCollider()
    {
        _targetsInRange.Clear();
        _col.enabled = true;
    }

    /// <summary>
    /// 애니메이션 이벤트로 호출: 콜라이더 비활성화
    /// </summary>
    public void DisableCollider()
    {
        _col.enabled = false;
    }

    /// <summary>
    /// 애니메이션 이벤트로 호출: 범위 내 모든 대상에게 데미지를 준다
    /// </summary>
    public void DealDamage()
    {
        if (!_col.enabled) return;

        float kbForce = Random.Range(minKnockbackForce, maxKnockbackForce);
        float kbUp = Random.Range(minKnockbackUpwardForce, maxKnockbackUpwardForce);

        foreach (var target in _targetsInRange)
        {
            target.TakeDamage(damageAmount, transform, kbForce, kbUp);
        }
    }

    private IEnumerator AutoDestroy()
    {
        yield return new WaitForSeconds(3f);
        Destroy(gameObject);
    }
}
