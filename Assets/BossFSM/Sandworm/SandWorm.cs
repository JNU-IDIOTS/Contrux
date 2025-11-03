using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class SandWorm : MonoBehaviour
{
    public Transform Boss;
    public Transform player;

    public Sandworm_Coroutine coroutine;
    public Sandworm_Animation ani;

    private List<BossSkill> SandwormSkills = new List<BossSkill>();

    private BossSkill lastUsedSkill = null;
    public float skillCheckInterval = 4f;
    public bool isCasting = false;
    public float dashDistance = 4f;
    public bool CantDash = false;

    public void Awake()
    {
        coroutine = GetComponent<Sandworm_Coroutine>();
        ani = GetComponent<Sandworm_Animation>();
    }
    public void InitSkills()
    {
        SandwormSkills.Add(new BossSkill
        {
            skillName = "Nattack",
            skillAction = () =>
            {
                Debug.Log("스킬 액션 진입: Nattack");
                StartCoroutine(coroutine.NAttack());
                Debug.Log("시작");
            },
            cooldown = 8f,
            castTime = 8f,
            lastUsedTime = -999f
        });
        SandwormSkills.Add(new BossSkill
        {
            skillName = "jumpAttack",
            skillAction = () =>
            {
                StartCoroutine(coroutine.Jump(player, 1f));
                Debug.Log("스킬 액션 진입 : JumpAttack");
            },
            cooldown = 8f,
            castTime = 8f,
            lastUsedTime = -999f
        });
        SandwormSkills.Add(new BossSkill
        {
            skillName = "spit",
            skillAction = () =>
            {
                StartCoroutine(coroutine.Spit());
                Debug.Log("스킬 액션 진입 : spit");
            },
            cooldown = 8f,
            castTime = 8f,
            lastUsedTime = -999f
        });
        SandwormSkills.Add(new BossSkill
        {
            skillName = "Jump Out",
            skillAction = () =>
            {
                Debug.Log("스킬 액션 진입 : Jump Out");
                StartCoroutine(coroutine.JumpOut(player, 1f));
            },
            cooldown = 15f,
            castTime = 15f,
            lastUsedTime = -999f
        });


    }

    public IEnumerator SkillLoop()
    {
        while (true)
        {
            Debug.Log("스킬루프 들어옴.");
            yield return new WaitForSeconds(skillCheckInterval);

            Debug.Log("스킬 대기 기간");
            if (isCasting) continue;

            // 가능한 스킬 목록 중 준비된 것만 고름
            var readySkills = SandwormSkills.FindAll(skill =>
            {
                // Dash라면 거리 체크
                if (skill.skillName == "NAttack")
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
        if (isCasting)
        {
            Debug.Log($"[{skill.skillName}] 시도했지만 이미 다른 스킬 시전 중입니다."); //방어코드 중복방지
            yield break;
        }

        isCasting = true;
        Debug.Log($"{DateTime.Now:HH:mm:ss}, {skill.skillName} 캐스트 시작");
        skill.Use();
        yield return new WaitForSeconds(skill.castTime);
        Debug.Log($"{DateTime.Now:HH:mm:ss}, {skill.skillName} 캐스트 종료");
        lastUsedSkill = skill;
        isCasting = false;
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        
    }
}
