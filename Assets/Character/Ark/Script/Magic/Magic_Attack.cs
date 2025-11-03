using System;
using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;
// using UnityEditor;   // 런타임 스크립트에서 불필요. (빌드 에러 예방)
using Unity.VisualScripting;

public class Magic_Attack : MonoBehaviour
{
    public float requiredHoldTime = 0.35f; // 차징 강공격 차징 시간

    // Attack
    public float m_timeSinceAttack = 0.0f;
    public int m_currentAttack = 0;
    public float BonusDamage = 1f;

    // speed
    public float SpeedMulti = 1.2f;
    public static bool AttackCountReady = false; // 공격이 발동했는지 확인하는 bool문

    [Header("스킬")]
    [SerializeField] private GameObject slashPrefab;
    [SerializeField] private Transform slashSpawnPoint;
    [SerializeField] private float slashSpeed = 15f;
    [SerializeField] private bool destroySlashAtEnd = false; // 코루틴 종료 시 슬래시 제거 옵션

    // 참조
    private ManaSystem manaSystem;
    private PlayerSkill skill;
    private PlayerAnimationSync sync;
    private Player_move move;

    void Awake()
    {
        manaSystem = GetComponent<ManaSystem>();
        sync = GetComponentInParent<PlayerAnimationSync>();
        move = GetComponentInParent<Player_move>();
        skill = GetComponentInParent<PlayerSkill>();
        AttackSpeed.RegisterRunner(this);  // 실행 주체 등록
    }

    private void Update()
    {
        // ── [애니메이션 잠금] 현재 공격 모션(Attack1/Attack2)이 끝나기 전까지 모든 입력 무시
        var state = sync.CurrentStateInfo;
        bool inAttackAnim = (state.IsName("Attack 1") || state.IsName("Attack 2")
            || state.IsName("Down_Command") || state.IsName("Side_Command"));
        if (inAttackAnim && state.normalizedTime < 1f)
            return;

        // 1) 기본 타이머 업데이트
        m_timeSinceAttack += Time.deltaTime;

        // 3) 일반 콤보 입력 처리 (커맨드 윈도우 아닐 때만 A 키로 공격)
        if (Input.GetKeyDown(KeyCode.A) && m_timeSinceAttack > 0.1f)
        {
            AttackCountReady = true;
            AttackSpeed.AttackSpeedUP();
            AttackSpeed.SpeedUPSize();

            if (TryGetComponent<SoulBuffAttack>(out var buff))
            {
                buff.TryBuffAttack(); // 버프 상태 갱신
            }

            m_currentAttack++;
            BonusDamage = 1f; // 룬 마력은 오버히트 없으니까 상시 1배

            // 콤보 유지 시간 초과 시 초기화
            if (m_timeSinceAttack > 2.0f)
                m_currentAttack = 1;

            // 3단계 넘어가면 1단계로 순환
            if (m_currentAttack > 2)
                m_currentAttack = 1;

            // 애니메이터 트리거 발동
            move.AttackSpeedDownDuringAnimation(0.3f);
            sync.ApplyAttackSpeed();
            sync.NomalAttack(m_currentAttack);
            m_timeSinceAttack = 0f;
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            manaSystem.Q_Skill();
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            manaSystem.W_Skill();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            manaSystem.E_Skill();
        }

        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            Debug.Log("공속증가");
            //Ark_stat.SetAttackSpeed(SpeedMulti); // 공속 20% 증가
        }
        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            Debug.Log("공속 초기화");
            Ark_stat.ResetAttackSpeed();
        }
    }

    public void UseSlashSkill()
    {
        if (slashPrefab == null || slashSpawnPoint == null)
        {
            Debug.LogWarning("[UseSlashSkill] slashPrefab 또는 slashSpawnPoint가 비었습니다.");
            return;
        }

        // 1) Player_move 컴포넌트를 통해 플레이어 루트의 스케일 X를 가져옵니다.
        float playerScaleX = move != null ? move.transform.localScale.x : transform.localScale.x;
        int facingDir = playerScaleX > 0f ? 1 : -1;

        // 2) 검기 인스턴스화
        GameObject slash = Instantiate(slashPrefab, slashSpawnPoint.position, Quaternion.identity);

        // 3) SpriteRenderer가 있으면 flipX로 좌우 반전
        var sr = slash.GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.flipX = facingDir < 0;

        // 5) 이동 방향 설정
        Vector2 dir = Vector2.right * facingDir;

        // 6) 코루틴으로 이동 (인스턴스에서 duration 읽기)
        float dur = 0.3f; // 기본값
        var fx = slash.GetComponent<SlashEffect>();
        if (fx != null) dur = fx.duration;

        StartCoroutine(MoveSlashManual(slash.transform, dir, dur, destroySlashAtEnd));
    }

    private IEnumerator MoveSlashManual(Transform t, Vector2 dir, float duration, bool destroyWhenDone)
    {
        // 시작 시점에서 이미 파괴/누락된 경우 방지
        if (t == null) yield break;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            // 매 프레임 파괴/누락 체크 → MissingReferenceException 방지
            if (t == null) yield break;

            t.position += (Vector3)(dir * slashSpeed * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 종료 시 슬래시 제거 옵션
        if (destroyWhenDone && t != null)
        {
            var go = t.gameObject;
            if (go != null)
                Destroy(go);
        }
    }

    private void OnDisable()
    {
        // 이 스크립트가 비활성화될 때 안전 정리
        StopAllCoroutines();
    }

    private void OnDestroy()
    {
        // 오브젝트 파괴 시 코루틴 안전 정리
        StopAllCoroutines();
    }
}
