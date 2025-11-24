using UnityEngine;
using System;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("상태")]
    public float currentHealth;
    public bool isDead = false;
    private bool _hasRevived = false; // 게임당 1회 부활 체크용

    private PlayerRef _ref;

    private void Awake()
    {
        _ref = GetComponent<PlayerRef>();
    }

    private void Start()
    {
        // 게임 시작 시 최대 체력으로 초기화
        if (_ref != null && _ref._Status != null)
        {
            currentHealth = _ref._Status.FinalMaxHP;
        }
        else
        {
            currentHealth = 100f; // 기본값
        }
    }

    private void Update()
    {
        // 디버그용: K 키 누르면 20 데미지 입기
        if (Input.GetKeyDown(KeyCode.K))
        {
            TakeDamage(20f);
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        // [방어 공식 적용]
        float def = (_ref != null && _ref._Status != null) ? _ref._Status.FinalDefense : 0f;
        float reducedDmg = Mathf.Max(1f, damage - def);

        // 데미지 감소 배율 적용
        float mult = (_ref != null && _ref._Status != null) ? _ref._Status.DamageReduceMult : 1.0f;
        float finalDamage = reducedDmg * mult;

        currentHealth -= finalDamage;
        Debug.Log($"피격! 데미지: {finalDamage} (남은 체력: {currentHealth})");

        // 사망 체크
        if (currentHealth <= 0)
        {
            TryDie();
        }
    }

    private void TryDie()
    {
        // 부활 체크 (필요 시 주석 해제하여 사용)
        /*
        if (!_hasRevived && _ref._Status != null && _ref._Status.CanRevive)
        {
            Revive();
        }
        else
        {
            Die();
        }
        */
        Die(); // 지금은 무조건 사망 처리
    }

    private void Revive()
    {
        _hasRevived = true;
        float revivePercent = _ref._Status != null ? _ref._Status.ReviveHpPercent : 0.5f;
        currentHealth = (_ref._Status != null ? _ref._Status.FinalMaxHP : 100f) * revivePercent;
        
        Debug.Log($"부활 발동! 체력 {currentHealth}로 복구됨.");
    }

    private void Die()
    {
        isDead = true;
        currentHealth = 0;
        Debug.Log("플레이어 사망...");
        
        // 🚀 [수정됨] _ref._Statu 호출 제거 -> 싱글톤 Instance 사용
        // 사망 시 매니저에게 알림 (아이템 초기화 등)
        if (StatDataManager.Instance != null)
        {
            StatDataManager.Instance.OnPlayerDead();
            Debug.Log("ㅗ");
        }
        else
        {
            Debug.LogWarning("StatDataManager 인스턴스를 찾을 수 없습니다.");
        }
    }
}