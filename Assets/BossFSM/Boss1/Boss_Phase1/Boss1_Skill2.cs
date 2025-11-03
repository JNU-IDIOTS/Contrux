/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.CompilerServices;

public class Boss1_SkillMana : MonoBehaviour
{                                                                   // 캐스트 타임과 쿨타임은 별개로 시간이 작동하고 캐스트 타임은 패턴이 실제로 걸리는 시간. 쿨타임은 이 시간이 끝날때 까지 시전을 하지 못함.
    private List<BossSkill> phase1Skills = new List<BossSkill>();   // 되도록 캐스트 타임과 쿨타임은 동일하게 할 것.
    private List<BossSkill> phase2Skills = new List<BossSkill>();
    private List<BossSkill> currentSkillList;
    private BossSkill lastUsedSkill = null;
    private Coroutine skillLoopCoroutine;

    public Boss1_Coroutine coroutine;
    public Boss1_WheelWind wheelWind;
    public Boss1_CatchThrow catchThrow;
    public Boss1_BackDash backdash;
    public Boss1_Jump jump;

    public Transform player;
    public float skillCheckInterval = 8f;
    public bool isCasting = false;
    //public float dashDistance = 10f;
    public bool CantDash = false;

    private void Start()
    {
        // 컴포넌트 연결
        coroutine = GetComponent<Boss1_Coroutine>();
        wheelWind = GetComponent<Boss1_WheelWind>();
        catchThrow = GetComponent<Boss1_CatchThrow>();
        backdash = GetComponent<Boss1_BackDash>();
        jump = GetComponent<Boss1_Jump>();

        InitSkills();
        SetPhase(1); // 초기 페이즈 설정
    }

    public void InitSkills()
    {
        // 페이즈 1 스킬
        phase1Skills.Add(new BossSkill
        {
            skillName = "WheelWind",
            skillAction = () =>
            {
                Debug.Log("스킬 액션 진입: WheelWind");
                StartCoroutine(coroutine.FinalWheel());
                Debug.Log("시작");
            },
            cooldown = 9f,
            castTime = 9f,
            lastUsedTime = -999f
        });
        phase1Skills.Add(new BossSkill
        {
            skillName = "CatchThrow",
            skillAction = () =>
            {
                catchThrow.LaunchBoomerang();
                Debug.Log("시작");
            },
            cooldown = 9f,
            castTime = 6f,
            lastUsedTime = -999f
        });

        phase1Skills.Add(new BossSkill
        {
            skillName = "Jump",
            skillAction = () =>
            {
                StartCoroutine(coroutine.JumpFinal());
                Debug.Log("시작");
            },
            cooldown = 9f,
            castTime = 9f,
            lastUsedTime = -999f
        });

        // 페이즈 2 스킬
        phase2Skills.Add(new BossSkill
        {
            skillName = "EnWheelWind",
            skillAction = () =>
            {
                Debug.Log("스킬 액션 진입: EnWheelWind");
                StartCoroutine(coroutine.Phase2Wheelwind()); // 동일한 코루틴 사용
                Debug.Log("시작2");
            },
            cooldown = 12f,
            castTime = 12f,
            lastUsedTime = -999f
        });

        phase2Skills.Add(new BossSkill
        {
            skillName = "Dash",
            skillAction = () =>
            {
                Debug.Log("스킬 액션 진입: Dash");
                StartCoroutine(coroutine.dashCoroutine()); // 동일한 코루틴 사용
                Debug.Log("시작2");
            },
            cooldown = 20f,
            castTime = 5f,
            lastUsedTime = -999f
        });

        phase2Skills.Add(new BossSkill
        {
            skillName = "EnJump",
            skillAction = () =>
            {
                StartCoroutine(coroutine.JumpDashFinal());
                Debug.Log("시작2");
            },
            cooldown = 8f,
            castTime = 8f,
            lastUsedTime = -999f
        });
    }
    public void SetPhase(int phase)
    {
        // 기존 코루틴 중지
        if (skillLoopCoroutine != null)
            StopCoroutine(skillLoopCoroutine);

        // 페이즈에 맞는 스킬 리스트로 변경
        if (phase == 1)
        {
            currentSkillList = phase1Skills;
        }
        else if (phase == 2)
        {
            currentSkillList = phase2Skills;
        }

        // 새 코루틴 실행
        skillLoopCoroutine = StartCoroutine(SkillLoop());
        Debug.Log($"[스킬 페이즈 전환] 페이즈 {phase}");
    }

    /*public IEnumerator SkillLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(skillCheckInterval);

            if (isCasting) continue;

            var readySkills = currentSkillList.FindAll(skill => skill.IsReady() && skill != lastUsedSkill);
            if (readySkills.Count > 0)
            {
                int index = UnityEngine.Random.Range(0, readySkills.Count);
                var selected = readySkills[index];
                StartCoroutine(CastSkill(selected));
                Debug.Log($"[스킬 사용] {selected.skillName}");
            }
            else
            {
                Debug.Log("모든 스킬 쿨타임 중");
            }
        }
    }*/
   /* public IEnumerator SkillLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(skillCheckInterval);

            if (isCasting) continue;

            // 가능한 스킬 목록 중 준비된 것만 고름
            var readySkills = currentSkillList.FindAll(skill =>
            {
                // Dash라면 거리 체크
                if (skill.skillName == "Dash")
                {
                    float dist = Vector3.Distance(transform.position, player.position);
                    return skill.IsReady() && skill != lastUsedSkill && dist > dashDistance && !CantDash;
                }

                // 그 외 일반 스킬
                return skill.IsReady() && skill != lastUsedSkill;
            });

            if (readySkills.Count > 0)
            {
                int index = UnityEngine.Random.Range(0, readySkills.Count);
                var selected = readySkills[index];
                StartCoroutine(CastSkill(selected));
                Debug.Log($"[스킬 사용] {selected.skillName}");
            }
            else
            {
                Debug.Log("모든 스킬 쿨타임 중 또는 거리 조건 미충족");
            }
        }
    }


    private IEnumerator CastSkill(BossSkill skill)
    {
        isCasting = true;
        Debug.Log($"{DateTime.Now:HH:mm:ss}, {skill.skillName} 캐스트 시작");
        skill.Use();
        yield return new WaitForSeconds(skill.castTime);
        Debug.Log($"{DateTime.Now:HH:mm:ss}, {skill.skillName} 캐스트 종료");
        lastUsedSkill = skill;
        isCasting = false;
    }
}*/
