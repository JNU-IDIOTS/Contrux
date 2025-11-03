using UnityEngine;
using UnityEngine.InputSystem.XR.Haptics;
using static UnityEngine.GraphicsBuffer;
using System.Collections;
using System.ComponentModel;
using System.Collections.Generic;
using Unity.VisualScripting;

public class Boss1_FSM : MonoBehaviour
{
    public IEnemyState bossState;
    public Boss1_Phase1 phase1;
    public Boss1_Phase2 phase2;
    public Animator animator;
    public Boss1_Coroutine Coroutine;
    public Boss1_SkillManager skillManager;
    private Rigidbody2D targetRb;
    public Boss1_Phasechange Phasechange;
    public Rigidbody2D rb;
    public Boss1_Animation ani;
    public static bool cphase2 = false; // 넉백 위한 분기점


    public static float maxHP;
    public static float currentHP;


    [Header("보스 제원")]
    [SerializeField] private Transform Nattack;
    public Transform target;
    public float attackRadius = 1.5f;
    public LayerMask playerLayer;
    public int damageAmount = 20;
    public float searchRadius;
    public bool hit = false;

    [Header("보스 기절 변수")]
    [SerializeField] public bool isGroggy = false;
    public float damageAccumulator = 0;// -------------------------------------------> 데미지 담는 변수 여기에 딜 데미지 넣으면서 증가증가
    public float groggyGauge = 500; // -----------------------------------------------> 기절 게이지 총량 보스 UI넣을 요소 

    [Header("기둥 넣을것들(Pillar)")]
    [SerializeField] public List<Boss1_Pillar> pillars = new List<Boss1_Pillar>();

    [Header("기둥 쿨다운")]
    [SerializeField] private float pillarDamageCooldown = 2f;  // 데미지 사이 최소 간격
    private float lastPillarDamageTime = -Mathf.Infinity;

    private void Awake()
    {
        //targetRb = target.GetComponent<Rigidbody2D>();
        maxHP = hp.boss1_health;
        currentHP = hp.boss1_health;
    }

    public void FindTarget()
    {
        /*Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, searchRadius);

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                target = hit.transform;
                //Debug.Log($"[타겟 획득] {target.name} 위치: {target.position}");
                return;
            }
        }*/

        if (target == null)
        {
            var go = GameObject.FindWithTag("Player");
            if (go != null) target = go.transform;
        }

        Debug.Log("Talen Ark를 찾을 수 없습니다.");
    }

    

    private void Start()
    {
        ani = GetComponent<Boss1_Animation>();
        phase1 = new Boss1_Phase1(this);
        Coroutine = GetComponent<Boss1_Coroutine>();
        phase1.CatchThrow = GetComponent<Boss1_CatchThrow>();
        phase1.CatchThrow.FSM = this;
        phase2 = new Boss1_Phase2(this);
        phase1.Jump = GetComponent<Boss1_Jump>();
        phase1.Jump.FSM = this;
        Coroutine.Init(this);
        skillManager = GetComponent<Boss1_SkillManager>();
        Phasechange = GetComponent<Boss1_Phasechange>();

        // 1) 내 함수로 찾기
        FindTarget();

        if (target == null)
        {
            var go = GameObject.FindWithTag("Player");
            if (go != null) target = go.transform;
        }

        // 3) 최종 바인딩
        if (target != null) targetRb = target.GetComponent<Rigidbody2D>();
        else
        {
            Debug.LogError("[Boss1_FSM] target이 비었습니다. 인스펙터 할당 또는 Player 태그 확인하세요.", this);
            return;
        }

        ChangeState(phase1);
        skillManager.SetPhase(1);
        //ChangeState(phase2);
        Boss1_Pillar[] foundPillars = Object.FindObjectsByType<Boss1_Pillar>(FindObjectsSortMode.None);
        pillars.AddRange(foundPillars);


    }
    private void Update()
    {
        bossState?.Update();
        pillars.RemoveAll(p => p == null);

        if (pillars.Count == 0 && bossState is Boss1_Phase1)
        {
            Debug.Log("기둥 모두 파괴! 2페이즈로 전환");
            ChangeState(phase2);
            cphase2 = true;
        }
    }

    public void FuckFuck()
    {
        hit = true;
    }

    public void FaceDirection(int dir)
    {
        if (dir == 0) return;  // 정지 상태에서는 방향 유지
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * -dir;  // dir이 -1이면 왼쪽, 1이면 오른쪽
        transform.localScale = scale;
    }

    public void ChangeState(IEnemyState newState)
    {
        if (bossState != null)
        {
            bossState.Exit();  // 기존 상태 종료
            StopAllCoroutines();  // 기존 상태 관련 코루틴 모두 정지
        }
        bossState = newState;
        bossState.Enter();
    }
    public void BossDealDamage(CircleCollider2D collider, int damage)
    {
        Vector2 center = collider.bounds.center;
        float radius = collider.radius * collider.transform.lossyScale.x; // 스케일 반영

        Collider2D[] hits = Physics2D.OverlapCircleAll(center, radius, playerLayer);

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player") && hit.TryGetComponent(out PlayerHealth player))
            {
                player.TakeDamage(damage,target,0f,0f);
            }
        }
    }

    
    public void Getdamage(float damage)
    {
        float finalDamage = damage;
        if (isGroggy)
            finalDamage += 5f;
        else
            finalDamage = Mathf.Max(1f, finalDamage - 5f);

        currentHP -= finalDamage;
        if (!isGroggy)
            damageAccumulator += finalDamage;

        Debug.Log($"피격! 현재 HP: {currentHP}, 누적 데미지: {damageAccumulator}");

            //그로기 누적 수치에 도달했는지 체크
        if (!isGroggy && damageAccumulator >= groggyGauge)
        {
            Debug.Log("[FSM] 누적 데미지로 인해 그로기 진입 시도");
            skillManager.StartGroggy();   // ⭐ 여기서 실제 그로기 진입
            damageAccumulator = 0f;
            isGroggy = true;
        }

        if (currentHP <= 0f)
        {
            Debug.Log("보스 사망");
            Destroy(gameObject);
            return;
        }

    }
    
    // 디버그용 시각화
    private void OnDrawGizmosSelected()
    {
        if (Nattack != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(Nattack.position, attackRadius);
        }
    }

    public void DamagePillarsInRange(Collider2D skillCol, int damage)
    {
        if (Time.time - lastPillarDamageTime < pillarDamageCooldown)
            return;
        lastPillarDamageTime = Time.time;

        if (skillCol == null) return;

        Collider2D[] hits;

        // BoxCollider2D 검사
        if (skillCol is BoxCollider2D box)
        {
            hits = Physics2D.OverlapBoxAll(
                box.bounds.center,
                box.bounds.size,
                box.transform.eulerAngles.z    // 회전 적용
                                               // no layerMask 파라미터 → 모든 레이어에서 검사
            );
        }
        // CircleCollider2D 검사
        else if (skillCol is CircleCollider2D circle)
        {
            float radius = circle.radius * circle.transform.lossyScale.x;
            hits = Physics2D.OverlapCircleAll(
                circle.bounds.center,
                radius
            // no layerMask 파라미터 → 모든 레이어에서 검사
            );
        }
        else
        {
            // 그 외 콜라이더는 AABB 영역으로 대체
            var b = skillCol.bounds;
            hits = Physics2D.OverlapAreaAll(
                b.min,
                b.max
            // no layerMask → 모든 레이어
            );
        }

        // 범위 안 모든 콜라이더 중 Pillar 컴포넌트가 있는 것만 처리
        foreach (var col in hits)
        {
            if (!col.CompareTag("Pillar"))
                continue;

            if (col.TryGetComponent<Boss1_Pillar>(out var pillar))
            {
                pillar.TakeDamage(damage);
            }
        }
    }
    public IEnumerator KnockBack(Transform Player, float knockbackForce, float knockbackUpwardForce)
    {
        // 1. 넉백 방향 계산
        // 플레이어의 위치에서 공격한 대상의 위치를 빼서, 공격 대상으로부터 '멀어지는' 방향 벡터를 구합니다.
        Vector2 knockbackDir = (transform.position - Player.position).normalized;
        Rigidbody2D damageRb = Player.GetComponent<Rigidbody2D>();
        // 2. 기존 속도 초기화
        rb.linearVelocity = Vector2.zero;
        yield return new WaitForFixedUpdate(); // 물리 프레임 대기
        // 3. 계산된 방향으로 힘 적용
        Vector2 forceToApply = new Vector2(knockbackDir.x * knockbackForce, knockbackUpwardForce);
        damageRb.AddForce(forceToApply, ForceMode2D.Impulse);
        Debug.Log($"플레이어가 {Player.name}로부터 넉백당했습니다!");
    }
}
