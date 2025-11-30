using UnityEngine;
using System;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("상태")]
    public float currentHealth;
    public bool isDead = false;
    private bool _hasRevived = false; 

    private PlayerRef _ref;

    private void Awake()
    {
        _ref = GetComponent<PlayerRef>();
    }

    private void Start()
    {
        // 🚀 [중요] Start 시점에 스탯이 아직 계산 안 됐을 수 있으므로 강제 갱신 요청
        if (_ref != null && _ref._Status != null)
        {
            _ref._Status.CalculateStats();
        }

        // 1. 저장된 데이터가 있으면 불러오기 (이어하기)
        if (StatDataManager.Instance != null && StatDataManager.Instance.playerData.currentHP > 0)
        {
            currentHealth = StatDataManager.Instance.playerData.currentHP;
        }
        // 2. 없으면 최대 체력으로 시작 (기본 100이 아니라 계산된 FinalMaxHP 사용)
        else if (_ref != null && _ref._Status != null)
        {
            currentHealth = _ref._Status.FinalMaxHP;
        }
        else
        {
            currentHealth = 100f; // 비상용 기본값
        }
        
        Debug.Log($"❤️ 시작 체력: {currentHealth}");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            TakeDamage(20f);
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        float def = (_ref != null && _ref._Status != null) ? _ref._Status.FinalDefense : 0f;
        float reducedDmg = Mathf.Max(1f, damage - def);

        float mult = (_ref != null && _ref._Status != null) ? _ref._Status.DamageReduceMult : 1.0f;
        float finalDamage = reducedDmg * mult;

        currentHealth -= finalDamage;
        Debug.Log($"피격! 데미지: {finalDamage} (남은 체력: {currentHealth})");

        if (currentHealth <= 0)
        {
            TryDie();
        }
    }

    private void TryDie()
    {
        // 🚀 [수정됨] 주석 해제하여 부활 로직 복구
        if (!_hasRevived && _ref._Status != null && _ref._Status.CanRevive)
        {
            Revive();
        }
        else
        {
            Die();
        }
    }

    private void Revive()
    {
        _hasRevived = true;
        float revivePercent = _ref._Status != null ? _ref._Status.ReviveHpPercent : 0.5f;
        
        // 최대 체력 비례 회복
        currentHealth = (_ref._Status != null ? _ref._Status.FinalMaxHP : 100f) * revivePercent;
        
        Debug.Log($"✨ 부활 발동! 체력 {currentHealth}로 복구됨.");
    }

    private void Die()
    {
        isDead = true;
        currentHealth = 0;
        Debug.Log("💀 플레이어 사망...");
        
        if (StatDataManager.Instance != null)
        {
            StatDataManager.Instance.OnPlayerDead();
        }
    }
}