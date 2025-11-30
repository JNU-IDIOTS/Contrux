using UnityEngine;
using System.Collections;
using System;

public class NewAttackSkill : MonoBehaviour
{
    // --- 설정 변수 ---
    [Header("New Attack Combo Settings")]
    public float comboResetTime = 1.0f;
    private const float minTimeBetweenAttacks = 0.1f; // 🚨 변수명 수정
    private const float maxCommandWindow = 1f; // 🚨 변수명 수정

    // --- 상태 변수 ---
    [Header("Attack State")]
    public float _timeSinceAttack = 0.0f;
    public int _currentAttack = 0;
    public bool isCommandWindow = false;
    public float commandWindowTimer = 0f;
    public bool _isAttacking = false; // 후딜레이/공격 진행 중 플래그

    // 🚀 [추가] 데미지 보너스
    public float BonusDamageMultiplier = 1f; 

    // --- 컴포넌트 참조 ---
    private PlayerRef _ref;

    private void Awake()
    {
        _ref = GetComponentInParent<PlayerRef>();
    }

    private void Update()
    {
        // 🚀 [수정(우현)] 일시정지 시 차단
        if (Time.timeScale == 0) return;

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
            if (_ref._SteamPressureSystem == null || !_ref._SteamPressureSystem.isOverheated)
            {
                commandWindowTimer += Time.deltaTime;

                // 2-1. 시간 초과 시 콤보 리셋
                if (commandWindowTimer >= maxCommandWindow) 
                {
                    ResetComboState(); 
                    return;
                }

                // 🚀 [수정(우현)] 공격 키 (KeyAttack)
                if (Input.GetKeyDown(KeyManager.Instance.KeyAttack))
                {
                    // 🚀 [수정(우현)] DownCommand: 아래 방향키 + 공격 키
                    if (Input.GetKey(KeyManager.Instance.KeyDown))
                    {
                        ExecuteCommandSkill(true); 
                        return;
                    }

                    // 🚀 [수정(우현)] SideCommand: 좌우 방향키 + 공격 키
                    if (Input.GetKey(KeyManager.Instance.KeyLeft) && Input.GetKey(KeyManager.Instance.KeyRight))
                    {
                        ExecuteCommandSkill(false); 
                        return;
                    }

                    // A키만 눌렀을 경우: 커맨드 입력 창을 닫고 콤보를 리셋해야 함
                    ResetComboState(); 
                    return;
                }
            }
            return;
        }

        // 3. 일반 콤보 공격 처리 (A 키를 누를 때)
        // 🚀 [수정(우현)] KeyManager 사용
        if (Input.GetKeyDown(KeyManager.Instance.KeyAttack) && _timeSinceAttack > minTimeBetweenAttacks) 
        {
            // 콤보 타이머 초과 시 1타로 초기화
            if (_timeSinceAttack > comboResetTime)
                _currentAttack = 0;

            _currentAttack++;

            // 콤보가 2타를 넘어가면 1타로 순환
            if (_currentAttack > 2)
                _currentAttack = 1;
            
            // 🟢 [공격 시작]
            _isAttacking = true;

            if (_ref._Move != null && _ref._Ground != null && _ref._Ground.isGrounded) 
            {
                _ref._Move.SetAttackLock(true); 
            }

            // 🟢 [핵심 수정]: 애니메이션 호출
            bool isOverheated = _ref._SteamPressureSystem != null && _ref._SteamPressureSystem.isOverheated;

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
                    isCommandWindow = false; 
                }
                else
                {
                    isCommandWindow = true; 
                    commandWindowTimer = 0f;
                }
            }

            // 타이머 리셋
            _timeSinceAttack = 0f;
        }
    }

    public void ExecuteCommandSkill(bool isDownCommand) 
    {
        _isAttacking = true;

        if (_ref._Move != null && _ref._Ground != null && _ref._Ground.isGrounded)
        {
            _ref._Move.SetAttackLock(true); 
        }

        // 1. 애니메이션 호출
        if (_ref._AnimSync != null)
        {
            if (isDownCommand)
                _ref._AnimSync.DownCommand();
            else
                _ref._AnimSync.SideCommand();
        }

        Debug.Log($"Command Executed: {(isDownCommand ? "Down" : "Side")}");

        // 타이머 리셋
        _timeSinceAttack = 0f;
        isCommandWindow = false; 
    }

    public void OnAttackEnd()
    {
        _isAttacking = false;

        if (_ref._Move != null && _ref._Ground != null && _ref._Ground.isGrounded)
        {
            _ref._Move.SetAttackLock(false);
        }

        if (!isCommandWindow && _currentAttack > 0)
        {
            Debug.Log("Attack End: Combo status preserved for next input.");
        }
    }

    public void ResetComboState() 
    {
        isCommandWindow = false;
        _currentAttack = 0;
        _timeSinceAttack = 0f;
    }
    
    // 🚀 [수정(우현)] 최종 데미지 계산 함수 추가 (상점 스탯 연동)
    public (float damage, bool isCrit) GetFinalDamage()
    {
        // (NewAttackSkill.cs에서 쓰는 baseDamage 변수가 필요하면 선언해야 함. 현재 코드엔 없어서 10f로 가정하거나 추가 필요)
        float baseDamage = 10f; 

        float multiplier = 1.0f;
        float critChance = 0f;
        float critDamage = 1.5f; 

        if (_ref._Status != null)
        {
            multiplier = _ref._Status.AtkMultiplier; 
            critChance = _ref._Status.CritChanceBonus; 
            critDamage += _ref._Status.CritDamageBonus; 
        }

        if (_ref._SteamPressureSystem != null && _ref._SteamPressureSystem.isOverheated)
        {
            if (_ref._Status != null && _ref._Status.IsOverheatEnhanced)
            {
                critChance += 0.2f; 
                critDamage += 0.3f; 
            }
        }

        bool isCrit = UnityEngine.Random.value < critChance;
        float finalDmg = baseDamage * multiplier * BonusDamageMultiplier;
        if (isCrit) finalDmg *= critDamage;

        return (finalDmg, isCrit);
    }
}