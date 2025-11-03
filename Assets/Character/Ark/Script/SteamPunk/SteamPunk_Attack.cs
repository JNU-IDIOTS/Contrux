using System;
using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;
using UnityEditor;
using Unity.VisualScripting;

public class SteamPunk_Attack : MonoBehaviour, IAttackAnimEvents
{
    [Header("평타 스텝")]
    public float step1Distance = 0.35f;
    public float step1Duration = 0.08f;
    public float step2Distance = 0.28f;
    public float step2Duration = 0.07f;
    public AnimationCurve stepCurve; // 인스펙터에서 간단한 EaseOut 커브 지정 권장

    public float requiredHoldTime = 0.35f; // 차징 강공격 차징 시간
    private const float maxCommandWindow = 1f;  // 커맨드 입력 가능 최대 시간 (예: 0.5초)

    //Attack
    public float m_timeSinceAttack = 0.0f;
    public int m_currentAttack = 0;
    public bool isCommandWindow = false;         // 2번째 공격 후 커맨드 입력 대기 중인지 여부
    public float commandWindowTimer = 0f;        // 커맨드 입력 가능 시간 누적용 타이머
    public float BonusDamage = 1f;

    //speed
    public float SpeedMulti = 1.2f;
    public static bool AttackCountReady = false; //공격이 발동했는지 확인하는 bool문

    //참조
    private SteamPressureSystem steamSystem;
    private PlayerAnimationSync sync;
    private Player_move move;

    void Awake()
    {
        sync = GetComponentInParent<PlayerAnimationSync>();
        steamSystem = GetComponentInParent<SteamPressureSystem>();
        move = GetComponentInParent<Player_move>();
        AttackSpeed.RegisterRunner(this);  // 실행 주체 등록
    }

    private void Update()
    {
        // ── [애니메이션 잠금] 현재 공격 모션(Attack1/Attack2)이 끝나기 전까지 모든 입력 무시
        var state = sync.CurrentStateInfo;
        bool inAttackAnim = (state.IsName("Attack 1") || state.IsName("Attack 2") || state.IsName("OverHit_Attack 1")
            || state.IsName("OverHit_Attack 2") || state.IsName("OverHit_Attack1") || state.IsName("Side_Command") || state.IsName("Down_Command"));
        if (inAttackAnim && state.normalizedTime < 1f)
            return;

        // 1) 기본 타이머 업데이트
        m_timeSinceAttack += Time.deltaTime;

        // 2) 커맨드 윈도우 처리 (3번째 공격 후 입력 대기)
        if (isCommandWindow)
        {
            // 오버히트 상태가 아니어야 커맨드 입력과 시간 진행
            if (steamSystem != null && !steamSystem.isOverheated)
            {
                commandWindowTimer += Time.deltaTime;

                // 2-1) 시간 초과 → 윈도우 종료
                if (commandWindowTimer >= maxCommandWindow)
                {
                    isCommandWindow = false;
                    m_currentAttack = 0;
                    m_timeSinceAttack = 0f;
                    return;
                }

                // 2-2) A 키 누름 → 아랫 방향키 있으면 아랫 강공
                if (Input.GetKeyDown(KeyCode.A))
                {
                    AttackCountReady = true;
                    AttackSpeed.AttackSpeedUP();
                    AttackSpeed.SpeedUPSize();
                    if (TryGetComponent<SoulBuffAttack>(out var buff))
                    {
                        buff.TryBuffAttack(); // 버프 상태만 갱신 (return 안 써도 됨)
                    }

                    if (Input.GetKey(KeyCode.DownArrow))
                    {
                        Debug.Log("아랫 강공");
                        move.AttackSpeedDownDuringAnimation(0.3f);
                        move.SetAttackLock(true);
                        sync.DownCommand();


                        isCommandWindow = false;
                        m_currentAttack = 0;
                        m_timeSinceAttack = 0f;
                        steamSystem.ApplyCommandSkill(steamSystem.pressureIncreasePerSkill_2);
                        return;
                    }

                    if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow))
                    {
                        Debug.Log("옆 강공");
                        move.AttackSpeedDownDuringAnimation(0.3f);
                        move.SetAttackLock(true);
                        sync.SideCommand();

                        isCommandWindow = false;
                        m_currentAttack = 0;
                        m_timeSinceAttack = 0f;
                        steamSystem.ApplyCommandSkill(steamSystem.pressureIncreasePerSkill_1);
                        return;
                    }
                }
            }


            // 커맨드 윈도우 중이므로 일반 공격 무시
            return;
        }


        // 3) 일반 콤보 입력 처리 (커맨드 윈도우 아닐 때만 A 키로 공격)
        if (Input.GetKeyDown(KeyCode.A) && m_timeSinceAttack > 0.1f)
        {
            AttackCountReady = true;
            AttackSpeed.AttackSpeedUP();
            AttackSpeed.SpeedUPSize();
            if (TryGetComponent<SoulBuffAttack>(out var buff))
            {
                buff.TryBuffAttack(); // 버프 상태만 갱신 (return 안 써도 됨)
            }

            m_currentAttack++;

            BonusDamage = 2f; //오버히트가 되면 데미지 2배

            // 3-1) 콤보 유지 시간 초과 시 초기화
            if (m_timeSinceAttack > 2.0f)
                m_currentAttack = 1;

            // 3-2) 콤보가 3단계를 넘어가면 1단계로 순환
            if (m_currentAttack > 2)
                m_currentAttack = 1;

            // 3-3) 애니메이터 트리거 발동
            if (steamSystem.isOverheated)
            {
                move.AttackSpeedDownDuringAnimation(0.3f);
                sync.ApplyAttackSpeed();
                sync.OverHitAttack(m_currentAttack);
                m_timeSinceAttack = 0f;
            }
            else
            {
                move.AttackSpeedDownDuringAnimation(0.3f);
                sync.ApplyAttackSpeed();
                sync.NomalAttack(m_currentAttack);
                m_timeSinceAttack = 0f;
            }

            // 3-4) 3번째 공격이 발동됐을 경우 커맨드 윈도우 진입
            if (m_currentAttack == 2)
            {
                if (steamSystem.isOverheated)
                {
                    isCommandWindow = false;
                }
                else
                {
                    isCommandWindow = true;
                    commandWindowTimer = 0f;
                }
            }
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

    // === 애니메이션 이벤트가 릴레이 통해 호출 ===
    public void OnStep(int index)
    {
        if (index == 1)
            move.StepForward(step1Distance, step1Duration, stepCurve);
        else if (index == 2)
            move.StepForward(step2Distance, step2Duration, stepCurve);
    }

    public void OnAttackEnd()
    {
        move.SetAttackLock(false);
    }

    public void OnSkillStart()
    {
        move.SetAttackLock(true);
    }

    public void OnSkillEnd()
    {
        move.SetAttackLock(false);
    }
}