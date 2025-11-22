// using System;
// using UnityEngine;
// using System.Collections;
// // UnityEngine.Rendering, UnityEditor, Unity.VisualScripting, IAttackAnimEvents 등은 삭제된 상태로 유지합니다.

// public class SteamPunk_Attack : MonoBehaviour 
// {
//     [Header("Combo Dash (Stepping)")]
//     public float step1Distance = 0.35f;
//     public float step1Duration = 0.08f;
//     public float step2Distance = 0.28f;
//     public float step2Duration = 0.07f;
//     public AnimationCurve stepCurve; 

//     [Header("Combo & Command Settings")]
//     [Tooltip("Required hold time for charged attack (if applicable).")]
//     public float requiredHoldTime = 0.35f;
    
//     private const float MaxCommandWindow = 1f; 

//     // Attack State
//     public float _timeSinceAttack = 0.0f;
//     public int _currentAttack = 0;
    
//     [Tooltip("True when player is in the time window for command input (3rd attack option).")]
//     public bool isCommandWindow = false; 
    
//     public float commandWindowTimer = 0f; 
//     public float BonusDamage = 1f;

//     // Speed & Buffs
//     public float SpeedMulti = 1.2f;
//     // public static bool AttackCountReady = false; 

//     // Component References
//     private SteamPressureSystem _steamSystem;
//     private PlayerMoveController _move; 
    
//     // 🚨 PlayerAnimationSync 참조 복원
//     private PlayerAnimationSync _sync;

//     void Awake()
//     {
//         // 🚨 PlayerAnimationSync 참조 초기화
//         _sync = GetComponentInParent<PlayerAnimationSync>();
//         _steamSystem = GetComponentInParent<SteamPressureSystem>();
//         _move = GetComponentInParent<PlayerMoveController>(); 
        
//         // AttackSpeed.RegisterRunner(this); 
//     }

//     private void Update()
//     {
//         // 🚨 애니메이션 상태 체크 로직 복원 (공격 애니메이션 중 입력 막기)
//         if (_sync != null)
//         {
//             var state = _sync.CurrentStateInfo;
//             bool inAttackAnim = (state.IsName("Attack 1") || state.IsName("Attack 2") || state.IsName("OverHit_Attack 1")
//                  || state.IsName("OverHit_Attack 2") || state.IsName("OverHit_Attack1") || state.IsName("Side_Command") || state.IsName("Down_Command"));
//             if (inAttackAnim && state.normalizedTime < 1f)
//                 return;
//         }

//         // 1) 기본 콤보 타이머 업데이트
//         _timeSinceAttack += Time.deltaTime;

//         // 2) 커맨드 입력 창 처리 (콤보 2타 후 3타 선택 입력)
//         if (isCommandWindow)
//         {
//             if (_steamSystem != null && !_steamSystem.isOverheated)
//             {
//                 commandWindowTimer += Time.deltaTime;

//                 // 2-1) 시간 초과 시 콤보 리셋
//                 if (commandWindowTimer >= MaxCommandWindow)
//                 {
//                     isCommandWindow = false;
//                     _currentAttack = 0;
//                     _timeSinceAttack = 0f;
//                     return;
//                 }

//                 // 2-2) A 키 입력 시 커맨드 스킬 발동
//                 if (Input.GetKeyDown(KeyCode.A))
//                 {
//                     // AttackCountReady = true; 
//                     // AttackSpeed.AttackSpeedUP();
//                     // AttackSpeed.SpeedUPSize(); 
                    
//                     if (TryGetComponent<SoulBuffAttack>(out var buff))
//                     {
//                         buff.TryBuffAttack(); 
//                     }

//                     // A + DownArrow (아래 커맨드)
//                     if (Input.GetKey(KeyCode.DownArrow))
//                     {
//                         Debug.Log("Down Command Attack");
//                         // 🟢 애니메이션/이동 잠금 호출 복원
//                         if (_move != null) _move.AttackSpeedDownDuringAnimation(0.3f);
//                         if (_move != null) _move.SetAttackLock(true);
//                         if (_sync != null) _sync.DownCommand(); 

//                         isCommandWindow = false;
//                         _currentAttack = 0;
//                         _timeSinceAttack = 0f;
//                         if (_steamSystem != null)
//                              _steamSystem.ApplyCommandSkill(_steamSystem.pressureIncreasePerSkill_2);
//                         return;
//                     }

//                     // A + LeftArrow/RightArrow (좌우 커맨드)
//                     if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow))
//                     {
//                         Debug.Log("Side Command Attack");
//                         // 🟢 애니메이션/이동 잠금 호출 복원
//                         if (_move != null) _move.AttackSpeedDownDuringAnimation(0.3f);
//                         if (_move != null) _move.SetAttackLock(true);
//                         if (_sync != null) _sync.SideCommand();

//                         isCommandWindow = false;
//                         _currentAttack = 0;
//                         _timeSinceAttack = 0f;
//                         if (_steamSystem != null)
//                             _steamSystem.ApplyCommandSkill(_steamSystem.pressureIncreasePerSkill_1);
//                         return;
//                     }
//                 }
//             }
//             return; 
//         }


//         // 3) 일반 콤보 공격 처리 (A 키를 누를 때)
//         if (Input.GetKeyDown(KeyCode.A) && _timeSinceAttack > 0.1f)
//         {
//             // AttackCountReady = true;
//             // AttackSpeed.AttackSpeedUP();
//             // AttackSpeed.SpeedUPSize();
            
//             if (TryGetComponent<SoulBuffAttack>(out var buff))
//             {
//                 buff.TryBuffAttack(); 
//             }

//             _currentAttack++;

//             BonusDamage = 2f; 

//             // 3-1) 콤보 간격 시간 초과 시 초기화
//             if (_timeSinceAttack > 2.0f)
//                 _currentAttack = 1;

//             // 3-2) 콤보가 3단계를 넘어가면 1단계로 순환
//             if (_currentAttack > 2)
//                 _currentAttack = 1;

//             // 🟢 애니메이션 트리거 발동 로직 복원
//             if (_steamSystem != null && _steamSystem.isOverheated)
//             {
//                 if (_move != null) _move.AttackSpeedDownDuringAnimation(0.3f);
//                 if (_sync != null) _sync.ApplyAttackSpeed();
//                 if (_sync != null) _sync.OverHitAttack(_currentAttack);
//                 _timeSinceAttack = 0f;
//             }
//             else
//             {
//                 if (_move != null) _move.AttackSpeedDownDuringAnimation(0.3f);
//                 if (_sync != null) _sync.ApplyAttackSpeed();
//                 if (_sync != null) _sync.NomalAttack(_currentAttack);
//                 _timeSinceAttack = 0f;
//             }

//             // 3-4) 2타 공격 후 커맨드 입력 창 열기
//             if (_currentAttack == 2)
//             {
//                 if (_steamSystem != null && _steamSystem.isOverheated)
//                 {
//                     isCommandWindow = false; // 과열 시 커맨드 봉인
//                 }
//                 else
//                 {
//                     isCommandWindow = true; // 커맨드 입력 가능
//                     commandWindowTimer = 0f;
//                 }
//             }
//         }
        
//         // 4) 디버그 입력 (임시 주석 처리)
//         if (Input.GetKeyDown(KeyCode.Alpha9))
//         {
//             Debug.Log("Attack Speed UP");
//         }
//         if (Input.GetKeyDown(KeyCode.Alpha8))
//         {
//             Debug.Log("Attack Speed Reset");
//         }
        
//         // 🚨 애니메이션 이벤트 대체 로직은 복원하지 않음 (이동 잠금 해제는 애니메이션 이벤트에 의존)
//     }

//     // 🚨 IAttackAnimEvents 인터페이스 구현부 (애니메이션 이벤트 핸들러)도 원본에 따라 복원해야 하지만, 
//     // 현재 코드에서는 해당 인터페이스가 제거되었으므로 함수 정의만 남겨둡니다.
//     // 이 함수들은 애니메이터 클립에서 이벤트로 호출됩니다.
    
//     public void OnStep(int index)
//     {
//         if (index == 1)
//             _move.StepForward(step1Distance, step1Duration, stepCurve);
//         else if (index == 2)
//             _move.StepForward(step2Distance, step2Duration, stepCurve);
//     }

//     public void OnAttackEnd()
//     {
//         if (_move != null) _move.SetAttackLock(false);
//     }

//     public void OnSkillStart()
//     {
//         if (_move != null) _move.SetAttackLock(true);
//     }

//     public void OnSkillEnd()
//     {
//         if (_move != null) _move.SetAttackLock(false);
//     }
// }