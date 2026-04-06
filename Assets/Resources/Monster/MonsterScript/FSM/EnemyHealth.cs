using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    private EnemyAI _enemyAI;
    private MonsterAnimatorController _ani;
    private Rigidbody2D _rb;

    [Header("상태 정보")]
    public int currentHP;
    public float currentCourage;
    public bool isDeath = false;
    public bool isBerserkMode = false;

    private void Awake()
    {
        _enemyAI = GetComponent<EnemyAI>();
        _ani = GetComponent<MonsterAnimatorController>();
        _rb = GetComponent<Rigidbody2D>();
    }

    // 뇌가 데이터를 로드하면 초기화
    public void InitHealth()
    {
        if (_enemyAI.speciesData == null) return;
        currentHP = _enemyAI.speciesData.maxHP;
        currentCourage = _enemyAI.speciesData.maxCourage;
    }

    // 평시 용기 회복 (IdleState 등에서 호출)
    public void RecoverCourage(float amount)
    {
        if (currentCourage < _enemyAI.speciesData.maxCourage)
        {
            currentCourage += amount * Time.deltaTime;
        }
    }

    // 피격 처리
    public void TakeDamage(float damage)
    {
        if (isDeath) return;

        float finalDamage = Mathf.Max(1, damage - _enemyAI.speciesData.defense);
        currentHP -= Mathf.RoundToInt(finalDamage);

        // 리더가 없거나 죽었으면 용기 감소
        if (_enemyAI._enemySquad.currentLeader == null || _enemyAI._enemySquad.currentLeader._enemyHealth.isDeath)
        {
            currentCourage -= _enemyAI.speciesData.courageDamagePerHit;
        }

        float effectiveCourage = currentCourage + _enemyAI._enemySquad.allyCourageBonus;

        // 광폭화 체크
        if (_enemyAI.speciesData.isLeader && _enemyAI.speciesData.useBerserk && !isBerserkMode)
        {
            if (currentHP <= _enemyAI.speciesData.maxHP * _enemyAI.speciesData.berserkThreshold)
            {
                _enemyAI._enemySquad.ActivateBerserk();
            }
        }

        // 도주 상태 전환
        if (effectiveCourage <= 0 && _enemyAI.currentState != _enemyAI.fleeState)
        {
            _enemyAI.ChangeState(_enemyAI.fleeState);
        }

        // 사망 처리
        if (currentHP <= 0)
        {
            isDeath = true;
            Die();
        }
    }

    public void Die()
    {
        // 리더였다면 부하들에게 알림
        if (_enemyAI.speciesData.isLeader)
        {
            _enemyAI._enemySquad.NotifyLeaderDeath();
        }

        _ani.Die();
        _rb.linearVelocity = Vector2.zero;
        _rb.bodyType = RigidbodyType2D.Static;
        Destroy(gameObject);
    }
}