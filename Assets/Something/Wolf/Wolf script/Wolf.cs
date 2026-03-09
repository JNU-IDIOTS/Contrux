/*using UnityEngine;
using System.Collections;
using TMPro;
using NUnit.Framework.Interfaces;
using UnityEngine.InputSystem.XR.Haptics;

public class Wolf : MonoBehaviour
{


    [Header("HP 설정")]
    [SerializeField] private float currentHP;
    [SerializeField] private bool IsDeath = false;

    [Header("피격 쿨타임")]
    private float damageCooldown = 0.3f;
    private float lastDamageTime = -999f;

    [SerializeField] private EnemyAI enemy; // 인스펙터 연결용 (또는 GetComponent로 할 수도 있음)
    /*[Header("UI")]
    [SerializeField] private TextMeshProUGUI gameOverText;

    private Animator animator;
    private Rigidbody2D rb;

    private bool isStunned = false;

    public bool isDie = false;

    void Start()
    {
        if (enemy == null)
        {
            Debug.LogWarning(" enemy가 인스펙터에 연결되지 않았습니다. GetComponent 시도");
            enemy = GetComponent<EnemyAI>();
        }

        if (enemy == null)
        {
            Debug.LogError(" EnemyAI 연결 실패 - 행동 불능 상태입니다.");
            return;
        }

        animator = enemy.GetComponent<Animator>();
        if (animator == null) Debug.LogError(" Animator 연결 실패");

        rb = enemy.GetComponent<Rigidbody2D>();
        if (rb == null) Debug.LogError(" Rigidbody2D 연결 실패");

        currentHP = enemy.maxHP;
        animator = enemy.GetComponent<Animator>();
        rb = enemy.GetComponent<Rigidbody2D>();

        if (enemy == null)
        {
            enemy = GetComponent<EnemyAI>();
            if (enemy == null)
                Debug.LogError("EnemyAI 컴포넌트를 찾을 수 없습니다.");
        }


    }

    private void FixDeathAnimation()
    {
        animator.SetTrigger("IsDeath"); // 상태 전이 차단
        Destroy(gameObject, 3.0f);
        if (TryGetComponent<Rigidbody2D>(out var rb))
        {
            rb.simulated = false;
        }

        // 충돌 제거
        foreach (var col in GetComponents<Collider2D>())
        {
            col.enabled = false;
        }

        // 모든 제어 컴포넌트 비활성화 (예: AI, 공격, 감지 등)
        if (TryGetComponent<EnemyAI>(out var ai)) ai.enabled = false;
        if (TryGetComponent<AttackableArea>(out var atk)) atk.enabled = false;
        if (TryGetComponent<DetectionArea>(out var det)) det.enabled = false;
        if (TryGetComponent<Wolf>(out var hp)) hp.enabled = false;
    }
    
    public void 
        (float damage)
    {
        
        if (canStun())
        {
            stun();
        }
        enemy.SetDetectionRadius(100f);
        Debug.Log($"데미지 입음 : ");
        if (IsDeath || isStunned) return;
        if (Time.time - lastDamageTime < damageCooldown) return;
        enemy.SetIsHit(true);
        animator.SetTrigger("IsAttacked");
        lastDamageTime = Time.time;
        currentHP -= damage;
        enemy.SetIsHit(false);
        Debug.Log($"wolf의 남은 체력 : {currentHP}");
        //animator.SetTrigger("Hurt");

        if (currentHP <= 0)
        {
            IsDeath = true;
            enemy.IsDeath = true;
            Debug.Log("늑대 죽음");
            enemy.Die();
            isDie = true;
            FixDeathAnimation();

        }
        
    }
    
    bool canStun()
    {
        return Time.time >= lastDamageTime + damageCooldown;
    }

    void stun()
    {
        rb.linearVelocity = Vector2.zero;
    }

    public void SetAttackrangeRadius(float radius)
    {
        Transform AttackableArea = transform.Find("AttackableArea");
        if (AttackableArea == null)
        {
            Debug.LogWarning("AttackableArea 오브젝트를 찾을 수 없습니다.");
            return;
        }

        CircleCollider2D collider = AttackableArea.GetComponent<CircleCollider2D>();
        if (collider == null)
        {
            Debug.LogWarning("AttackableArea에 CircleCollider2D가 없습니다.");
            return;
        }

    }

}*/
