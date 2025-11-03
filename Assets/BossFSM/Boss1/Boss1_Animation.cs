using System.Collections;
using UnityEngine;

public class Boss1_Animation : MonoBehaviour 
{
    [Header("Animator")]
    public Animator ani;

    // 보스가 사용하는 "모든 트리거" 목록 (Animator 파라미터 이름과 정확히 일치해야 함)
    [Header("All Trigger Names (Triggers only)")]
    [SerializeField] private string[] allTriggers = new string[] {
        "IsJump",
        "IsTop",
        "IsPrepareJump",
        "IsThrow",
        "IsCatch",
        "IsWheel",
        "WheelStart",
        "WheelEnd",
        "IsBackstep",
        "ISCHANGING",
        "Enthrow",
        "Dash",
        "Groggy",
        "GroggyEnd",
        "Nattack",
        "Nattackprepare"
    };

    private void Awake()
    {
        if (!ani) ani = GetComponent<Animator>();
    }

    // -------------- 트리거 안전 헬퍼 --------------
    /// 모든 트리거 리셋
    public void ResetAllTriggers()
    {
        if (!ani) return;
        for (int i = 0; i < allTriggers.Length; i++)
        {
            var t = allTriggers[i];
            if (!string.IsNullOrEmpty(t)) ani.ResetTrigger(t);
        }
    }

    /// 대상 트리거만 남기고 나머지 전부 리셋 후 Set
    public void SetTriggerSafe(string trigger)
    {
        if (!ani || string.IsNullOrEmpty(trigger)) return;

        // 다른 모든 트리거를 먼저 리셋
        for (int i = 0; i < allTriggers.Length; i++)
        {
            var t = allTriggers[i];
            if (!string.IsNullOrEmpty(t) && t != trigger)
                ani.ResetTrigger(t);
        }

        // 대상 트리거 발화
        ani.SetTrigger(trigger);
    }

    /// 한 프레임 유예까지 포함(Animator가 전이를 평가할 시간 제공)
    public IEnumerator SetTriggerAndYield(string trigger)
    {
        SetTriggerSafe(trigger);
        yield return null; // 1프레임 대기 → 트리거 소비 기회 확보
    }
    // ---------------------------------------------

    // ===== 여기서부터 기존 메서드들을 모두 안전 헬퍼로 변경 =====
    public void Jump()                 => SetTriggerSafe("IsJump");
    public void JumpAttack()           => SetTriggerSafe("IsTop");

    public void KeepGoing()            => ani.SetBool("KeepGoing", true);   // Bool은 그대로 유지
    public void KeepEnd()              => ani.SetBool("KeepGoing", false);

    public void PrepareJump()          => SetTriggerSafe("IsPrepareJump");
    public void ThrowBoomerang()       => SetTriggerSafe("IsThrow");
    public void CatchBoomerang()       => SetTriggerSafe("IsCatch");

    public void WheelPrepare()         => SetTriggerSafe("IsWheel");
    public void WheelStart()           => SetTriggerSafe("WheelStart");
    public void WheelEnd()             => SetTriggerSafe("WheelEnd");

    public void BackStep()             => SetTriggerSafe("IsBackstep");
    public void PhaseChange()          => SetTriggerSafe("ISCHANGING");

    public void EnThrow()              => SetTriggerSafe("Enthrow");
    public void Dash()                 => SetTriggerSafe("Dash");

    public void Groggy()               => SetTriggerSafe("Groggy");
    public void GroggyEnd()            => SetTriggerSafe("GroggyEnd");

    public void Nattack()              => SetTriggerSafe("Nattack");
    public void Nattackprepare()       => SetTriggerSafe("Nattackprepare");
}
