using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Boss1_Phase2 : IEnemyState
{
    private Boss1_FSM FSM;
    private MonoBehaviour mono;
    public Boss1_Animation ani;
    public Boss1_BackDash BackDash;
    public Boss1_CatchThrow CatchThrow;
    public Boss1_WheelWind Wheel;
    public Boss1_Coroutine Coroutine;
    public Boss1_SkillManager skillManager;

    public Boss1_Phase2(Boss1_FSM boss)
    {
        FSM = boss.GetComponent<Boss1_FSM>();
        mono = boss.GetComponent<MonoBehaviour>();
        ani = boss.GetComponent<Boss1_Animation>();
        CatchThrow = boss.GetComponent<Boss1_CatchThrow>();
        Coroutine = boss.GetComponent<Boss1_Coroutine>();
        Wheel = boss.GetComponent<Boss1_WheelWind>();
        BackDash = boss.GetComponent<Boss1_BackDash>();
        skillManager = boss.GetComponent<Boss1_SkillManager>();
    }

    public void Enter()
    {
        mono.StartCoroutine(Coroutine.PhaseChange());
        skillManager.SetPhase(2);/*2페이즈 변환*/
        mono.StartCoroutine(skillManager.SkillLoop());
    }

    public void Exit()
    {

    }

    public void Update()
    {

    }

    public IEnumerator Phase2Pattern()
    {
        yield return null;
    }

    public void EnJump()
    {

    }
}
