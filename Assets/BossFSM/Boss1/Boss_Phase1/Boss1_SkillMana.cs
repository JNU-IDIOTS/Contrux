using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.CompilerServices;
using NUnit.Framework;

public class Boss1_SkillManager : MonoBehaviour
{                                                                   // 캐스트 타임과 쿨타임은 별개로 시간이 작동하고 캐스트 타임은 패턴이 실제로 걸리는 시간. 쿨타임은 이 시간이 끝날때 까지 시전을 하지 못함.
    private List<BossSkill> phase1Skills = new List<BossSkill>();   // 되도록 캐스트 타임과 쿨타임은 동일하게 할 것.
    private List<BossSkill> phase2Skills = new List<BossSkill>();
    private List<BossSkill> groggySkills = new List<BossSkill>();
    private List<BossSkill> currentActiveSkillList;
    private List<BossSkill> currentSkillList;
    private BossSkill lastUsedSkill = null;
    private Coroutine skillLoopCoroutine;
    private Coroutine castingCoroutine;
    private Coroutine currentSkillCoroutine;
    private List<Coroutine> runningSkillCoroutines = new List<Coroutine>();

    public Boss1_Coroutine coroutine;
    public Boss1_WheelWind wheelWind;
    public Boss1_CatchThrow catchThrow;
    public Boss1_BackDash backdash;
    public Boss1_Jump jump;
    public Boss1_Animation ani;
    public Transform player;
    public Boss1_FSM fsm;
    public float skillCheckInterval = 8f;
    public bool isCasting = false;
    public float dashDistance = 4f;
    public bool CantDash = false;

    //public bool isGroggy = false;
    private float damageAccumulator = 0f;
    [SerializeField] private float groggyThreshold = 10f;
    [SerializeField] private float groggyDuration = 5f;
    private Coroutine groggyCoroutine;

    private void Start()
    {
        // 컴포넌트 연결
        coroutine = GetComponent<Boss1_Coroutine>();
        wheelWind = GetComponent<Boss1_WheelWind>();
        catchThrow = GetComponent<Boss1_CatchThrow>();
        backdash = GetComponent<Boss1_BackDash>();
        jump = GetComponent<Boss1_Jump>();
        ani = GetComponent<Boss1_Animation>();
        fsm = GetComponent<Boss1_FSM>();
        InitSkills();
        SetPhase(1); // 초기 페이즈 설정

        if (player == null)
        {
            var go = GameObject.FindWithTag("Player");
            if (go != null) player = go.transform;
        }
    }

    public void InitSkills()
    {
        groggySkills.Add(new BossSkill
        {
            skillName = "GroggyStun",
            skillAction = () =>
            {
                StartCoroutine(HandleGroggyStun());
            },
            cooldown = 0f,
            castTime = groggyDuration,
            lastUsedTime = -999f
        });
        // 페이즈 1 스킬
        phase1Skills.Add(new BossSkill
        {
            skillName = "Nattack",
            skillAction = () =>
            {
                Debug.Log("스킬 액션 진입: Nattack");
                StartCoroutine(coroutine.Nattack());
                Debug.Log("시작");
            },
            cooldown = 8f,
            castTime = 4f,
            lastUsedTime = -999f
        });
        phase1Skills.Add(new BossSkill
        {
            skillName = "WheelWind",
            skillAction = () =>
            {
                Debug.Log("스킬 액션 진입: WheelWind");
                StartCoroutine(coroutine.FinalWheel());
                Debug.Log("시작");
            },
            cooldown = 12f,
            castTime = 8f,
            lastUsedTime = -999f
        });
        /*phase1Skills.Add(new BossSkill
        {
            skillName = "CatchThrow",
            skillAction = () =>
            {
                catchThrow.LaunchBoomerang();
                Debug.Log("시작");
            },
            cooldown = 8f,
            castTime = 5f,
            lastUsedTime = -999f
        });*/

        phase1Skills.Add(new BossSkill
        {
            skillName = "Jump",
            skillAction = () =>
            {
                StartCoroutine(coroutine.JumpFinal());
                Debug.Log("시작");
            },
            cooldown = 10f,
            castTime = 8f,
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
            cooldown = 11f,
            castTime = 11f,
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
            cooldown = 10f,
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
            cooldown = 7f,
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
        //Debug.Log($"[스킬 페이즈 전환] 페이즈 {phase}");
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
    public IEnumerator SkillLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(skillCheckInterval);

            if (isCasting) continue;

            currentActiveSkillList = fsm.isGroggy ? groggySkills : currentSkillList;
            // 가능한 스킬 목록 중 준비된 것만 고름
            var readySkills = currentActiveSkillList.FindAll(skill =>
            {
                float dist = Vector3.Distance(transform.position, player.position);
                // Dash라면 거리 체크
                if (skill.skillName == "Dash")
                {
                    return skill.IsReady() && skill != lastUsedSkill && dist > dashDistance && !CantDash;
                }
                if (skill.skillName == "Nattack")
                {
                    return skill.IsReady() && skill != lastUsedSkill && dist <= 3f;
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
    /*public void ReceiveDamage(float amount)
    {
        float finalDamage = isGroggy ? amount + 5f : Mathf.Max(1f, amount - 5f);
        damageAccumulator += finalDamage;

        Debug.Log($"[보스 피격] 데미지: {finalDamage}, 누적: {damageAccumulator}");

        if (!isGroggy && damageAccumulator >= groggyThreshold)
        {
            StartGroggy();
        }
    }*/

    public void StartGroggy()
    {
        if (currentSkillCoroutine != null)
        {
            Debug.Log("[스킬 중단] 현재 스킬 코루틴 강제 종료");
            StopCoroutine(currentSkillCoroutine);
            currentSkillCoroutine = null;
        }

        if (fsm.isGroggy) return;

        fsm.isGroggy = true;

        if (castingCoroutine != null)
        {
            
            StopCoroutine(castingCoroutine);
            castingCoroutine = null;
        }

        isCasting = false;
        damageAccumulator = 0f;

        ani.ResetAllTriggers();
        //ani.Groggy();
        ani.SetTriggerSafe("Groggy");

        Debug.Log("[보스 상태] 그로기 진입");

        var groggySkill = groggySkills.Find(s => s.skillName == "GroggyStun");
        if (groggySkill != null)
        {
            groggyCoroutine = StartCoroutine(CastSkill(groggySkill));
        }
    }

    private IEnumerator HandleGroggyStun()
    {
        isCasting = true;
        Debug.Log("[그로기 스킬] 기절 애니메이션 등 실행");

        yield return new WaitForSeconds(groggyDuration);

        Debug.Log("[그로기 스킬] 종료");
        fsm.isGroggy = false;
        isCasting = false;
        ani.GroggyEnd();
    }

    private IEnumerator CastSkill(BossSkill skill)
    {
        if (isCasting)
        {
            Debug.Log($"[{skill.skillName}] 시도했지만 이미 다른 스킬 시전 중입니다.");
            yield break;
        }

        if (castingCoroutine != null)
        {
            StopCoroutine(castingCoroutine);
            castingCoroutine = null;
        }

        isCasting = true;
        Debug.Log($"{DateTime.Now:HH:mm:ss}, {skill.skillName} 캐스트 시작");

        skill.Use();  // 바로 실행

        castingCoroutine = StartCoroutine(SkillCastTimer(skill));
        yield return castingCoroutine;
    }
    private IEnumerator SkillCastTimer(BossSkill skill)
    {
        yield return new WaitForSeconds(skill.castTime);

        if (fsm.isGroggy)
        {
            castingCoroutine = null;
            yield break;
        }

        // ★ 캐스트가 끝난 '지금'을 사용 시각으로 기록 → 쿨다운은 종료 후부터
        skill.lastUsedTime = Time.time;

        lastUsedSkill = skill;
        isCasting = false;
        castingCoroutine = null;
    }
    public void TrackSkillCoroutine(Coroutine c)
    {
        if (c != null)
            runningSkillCoroutines.Add(c);
    }
    public void StopAllSkillCoroutines()
    {
        foreach (var c in runningSkillCoroutines)
        {
            if (c != null)
                StopCoroutine(c);
        }
        runningSkillCoroutines.Clear();
    }
}
