using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem.XR.Haptics;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class Boss1_Phase1 : IEnemyState 
{
    public Boss1_Jump Jump;
    public Boss1_WheelWind WheelWind;
    public Boss1_CatchThrow CatchThrow;
    public Boss1_FSM FSM;
    private MonoBehaviour mono;
    private Rigidbody2D rb;
    UnityEngine.Transform target;
    public Boss1_Animation ani;
    public Boss1_Coroutine Coroutine;
    public Boss1_WheelWind Wheel;
    public Boss1_BackDash BackDash;
    public Boss1_SkillManager skillManager;
    //public Boss1_Skill BossSkill;

    public Boss1_Phase1(Boss1_FSM boss)
    {
        FSM = boss.GetComponent<Boss1_FSM>();
        mono = boss.GetComponent<MonoBehaviour>();
        ani = boss.GetComponent<Boss1_Animation>();
        CatchThrow = boss.GetComponent<Boss1_CatchThrow>(); 
        Coroutine = boss.GetComponent<Boss1_Coroutine>();
        Wheel = boss.GetComponent <Boss1_WheelWind>();
        BackDash = boss.GetComponent<Boss1_BackDash>();
        //BossSkill = boss.GetComponent<Boss1_Skill>();
        skillManager = boss.GetComponent<Boss1_SkillManager>();
    }
    public void Awake()
    {

    }
    public void Enter()
    {

        rb = FSM.GetComponent<Rigidbody2D>();

        if (FSM.target == null)
        {
            Debug.LogError("보스 target이 null입니다. 점프 불가!");
            return;
        }

        Debug.Log("1페이즈 진입");

        //------------------------------------------------- 실행코드 구간.
        //mono.StartCoroutine(Coroutine.JumpFinal());/*점프*/
        //CatchThrow.LaunchBoomerang();/*부메랑*/
        //Wheel.StartCoroutine(Coroutine.FinalWheel());/*휠윈드*/
        //BackDash.backDash();/*백대쉬*/
        mono.StartCoroutine(skillManager.SkillLoop());/*스킬 찐*/
        //--------------------------------------------------
    }

    public IEnumerator Catchprepare()
    {
        ani.CatchBoomerang();
        yield return new WaitForSeconds(1.4f);
    }
    public void Exit()
    {

    }

    public void Update()
    {
        
    }
    
    public IEnumerator Phase1Pattern()
    {
        yield return null;
    }

    public void Nattack()
    {

    }
}
