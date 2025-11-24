using UnityEngine;
using System.Collections;
using System;

public class NewAttackSkill : MonoBehaviour
{
    // --- 설정 변수 ---
    [Header("New Attack Combo Settings")]
    public float comboResetTime = 1.0f;
    private const float minTimeBetweenAttacks = 0.1f; // 🚨 변수명 수정(우현)
    private const float maxCommandWindow = 1f; // 🚨 변수명 수정(우현)

    // --- 상태 변수 ---
    [Header("Attack State")]
    public float _timeSinceAttack = 0.0f;
    public int _currentAttack = 0;
    public bool isCommandWindow = false;
    public float commandWindowTimer = 0f;
    public bool _isAttacking = false; // 후딜레이/공격 진행 중 플래그

    // --- 컴포넌트 참조 ---
    // 🚨 [변경] 개별 참조 변수 제거 -> PlayerRef 사용
    private PlayerRef _ref;

    private void Awake()
    {
        // 🚨 [변경] PlayerRef 할당
        _ref = GetComponentInParent<PlayerRef>();
    }

    private void Update()
    {
        // 🚨 [후딜레이 적용]: 공격 애니메이션이 진행 중이면 모든 공격 입력을 차단합니다.
        if (_isAttacking)
        {
            return;
        }

        // 1. 콤보 타이머 업데이트
        _timeSinceAttack += Time.deltaTime;

        // 2. 커맨드 입력 창 처리 (2타 후 커맨드 입력 대기)
        if (isCommandWindow)
        {
            // 🚨 [변경] _steamSystem -> _ref._SteamPressureSystem
            if (_ref._SteamPressureSystem == null || !_ref._SteamPressureSystem.isOverheated)
            {
                commandWindowTimer += Time.deltaTime;

                // 2-1. 시간 초과 시 콤보 리셋
                if (commandWindowTimer >= maxCommandWindow) // 🚨 변수명 수정
                {
                    ResetComboState(); // 🚨 메서드명 수정
                    return;
                }

                // 2-2. A 키 입력 시 커맨드 스킬 분기
                if (Input.GetKeyDown(KeyCode.A))
                {
                    // DownCommand: 아래 방향키 + 공격 키
                    if (Input.GetKey(KeyCode.DownArrow))
                    {
                        ExecuteCommandSkill(true); // 🚨 메서드명 수정
                        return;
                    }

                    // 🚨 [핵심 수정]: SideCommand는 좌우 방향키를 모두 눌러야 발동
                    if (Input.GetKey(KeyCode.LeftArrow) && Input.GetKey(KeyCode.RightArrow))
                    {
                        ExecuteCommandSkill(false); // 🚨 메서드명 수정
                        return;
                    }

                    // A키만 눌렀을 경우: 커맨드 입력 창을 닫고 콤보를 리셋해야 함
                    ResetComboState(); // 🚨 메서드명 수정
                    return;
                }
            }
            return;
        }

        // 3. 일반 콤보 공격 처리 (A 키를 누를 때)
        if (Input.GetKeyDown(KeyCode.A) && _timeSinceAttack > minTimeBetweenAttacks) // 🚨 변수명 수정
        {
            // 콤보 타이머 초과 시 1타로 초기화
            if (_timeSinceAttack > comboResetTime)
                _currentAttack = 0;

            _currentAttack++;

            // 콤보가 2타를 넘어가면 1타로 순환 (2타 콤보이므로 3타를 막음)
            if (_currentAttack > 2)
                _currentAttack = 1;
            // 🟢 [공격 시작]: 후딜레이 플래그와 이동 잠금 설정
            _isAttacking = true;

            // 🚨 [변경] _move -> _ref._Move, _move._ground -> _ref._Ground (중요: Ground 직접 참조)
            if (_ref._Move != null && _ref._Ground != null && _ref._Ground.isGrounded) // 🚨 지상 체크 추가!
            {
                _ref._Move.SetAttackLock(true); // 🚨 지상일 때만 이동 잠금
            }

            // 🟢 [핵심 수정]: 여기서 해당 콤보 단계의 애니메이션을 직접 호출합니다.
            // 🚨 [변경] _steamSystem -> _ref._SteamPressureSystem
            bool isOverheated = _ref._SteamPressureSystem != null && _ref._SteamPressureSystem.isOverheated;

            // 🚨 [변경] _sync -> _ref._AnimSync
            if (_ref._AnimSync != null)
            {
                if (isOverheated)
                    _ref._AnimSync.OverHitAttack(_currentAttack);
                else
                    _ref._AnimSync.NomalAttack(_currentAttack);

                _ref._AnimSync.ApplyAttackSpeed();
            }

            Debug.Log($"Normal Attack Executed: Combo {_currentAttack}");

            // 4. 2타 공격 후 커맨드 입력 창 열기
            if (_currentAttack == 2)
            {
                if (isOverheated)
                {
                    isCommandWindow = false; // 과열 시 커맨드 봉인
                }
                else
                {
                    isCommandWindow = true; // 커맨드 입력 가능
                    commandWindowTimer = 0f;
                }
            }

            // 타이머 리셋
            _timeSinceAttack = 0f;
        }
    }

    public void ExecuteCommandSkill(bool isDownCommand) // 🚨 메서드명 수정
    {
        // 🟢 [공격 시작]: 후딜레이 플래그는 항상 설정 (공중이든 지상이든 공격 중 다른 공격 막기)
        _isAttacking = true;

        // 🚨 [핵심 수정]: 지상에 있을 때만 이동 잠금(SetAttackLock)을 실행합니다.
        // 🚨 [변경] _move -> _ref._Move, _move._ground -> _ref._Ground
        if (_ref._Move != null && _ref._Ground != null && _ref._Ground.isGrounded)
        {
            _ref._Move.SetAttackLock(true); // 🚨 지상일 때만 이동을 잠금
        }

        // 1. 애니메이션 호출
        // 🚨 [변경] _sync -> _ref._AnimSync
        if (_ref._AnimSync != null)
        {
            if (isDownCommand)
                _ref._AnimSync.DownCommand();
            else
                _ref._AnimSync.SideCommand();
        }

        // 2. 부가 효과 및 상태 변경 (커맨드 스킬은 즉시 리셋)
        Debug.Log($"Command Executed: {(isDownCommand ? "Down" : "Side")}");

        // 타이머 리셋
        _timeSinceAttack = 0f;
        isCommandWindow = false; // 커맨드 창 닫기
    }

    public void OnAttackEnd()
    {
        // 1. 공격 플래그 해제 (후딜레이 종료)
        _isAttacking = false;

        // 2. 이동 잠금 해제 
        // 🚨 [핵심 수정]: 지상일 때만 이동 잠금을 해제합니다.
        // 🚨 [변경] _move -> _ref._Move, _move._ground -> _ref._Ground
        if (_ref._Move != null && _ref._Ground != null && _ref._Ground.isGrounded)
        {
            _ref._Move.SetAttackLock(false);
        }

        // 3. 콤보 상태 유지/리셋
        if (!isCommandWindow && _currentAttack > 0)
        {
            Debug.Log("Attack End: Combo status preserved for next input.");
        }
    }


    public void ResetComboState() // 🚨 메서드명 수정
    {
        // 커맨드 시간 초과 시 콤보 상태 초기화
        isCommandWindow = false;
        _currentAttack = 0;
        _timeSinceAttack = 0f;
    }
}