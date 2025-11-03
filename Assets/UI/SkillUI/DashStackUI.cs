using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//[DefaultExecutionOrder(50)]
public class DashStackUI : MonoBehaviour
{
    [Header("스택 관련")]
    public float cooldownTime = 5f;

    [Header("스택바 프레임")]
    public GameObject dashStackBarFrame2;
    public GameObject dashStackBarFrame3;

    [Header("스택 이미지")]
    public Image[] stackBars2;
    public Image[] stackBars3;

    private Image[] currentStackBars;
    private int currentStacks;
    private bool[] isCoolingDown;

    // (스택 실제 위치, 쿨다운을 표시할 시각적 위치)
    Queue<int> cooldownQueue = new(); // 시각 인덱스는 나중에 결정
    int visualCooldownIndex = 0;

    private int nextVisualIndex = 0;
    private bool isCooldownRunning = false;

    [SerializeField] private ChooseOne chooseone;

    [SerializeField] private DashSkill steampunkdashSkill;
    [SerializeField] private DashSkill magicdashSkill;

    public void SteamPunkDash()
    {
        GameObject steampunkobj = GameObject.Find("Ark/SteamPunk");
        steampunkdashSkill = steampunkobj.GetComponent<DashSkill>();
        currentStacks = steampunkdashSkill.maxStacks;
        SetupUI();
        isCoolingDown = new bool[currentStackBars.Length];
        for (int i = 0; i < currentStackBars.Length; i++)
        {
            currentStackBars[i].fillAmount = 1f;
        }
    }

    public void MagicDash()
    {
        Debug.Log("룬마력");
        GameObject magicobj = GameObject.Find("Ark/Mana");
        magicdashSkill = magicobj.GetComponent<DashSkill>();
        currentStacks = magicdashSkill.maxStacks;
        SetupUI();
        isCoolingDown = new bool[currentStackBars.Length];
        for (int i = 0; i < currentStackBars.Length; i++)
        {
            currentStackBars[i].fillAmount = 1f;
        }
    }

    void SetupUI()
    {
        if (chooseone.SystemSteamPunk)
        {
            dashStackBarFrame2.SetActive(steampunkdashSkill.maxStacks == 2);
            dashStackBarFrame3.SetActive(steampunkdashSkill.maxStacks == 3);
            currentStackBars = (steampunkdashSkill.maxStacks == 2) ? stackBars2 : stackBars3;
        }
        else
        {
            dashStackBarFrame2.SetActive(magicdashSkill.maxStacks == 2);
            dashStackBarFrame3.SetActive(magicdashSkill.maxStacks == 3);
            currentStackBars = (magicdashSkill.maxStacks == 2) ? stackBars2 : stackBars3;
        }
    }

    public bool HasAvailableStack()
    {
        return currentStacks > 0;
    }

    public void UseDash()
    {
        if (currentStacks <= 0)
            return;

        // 오른쪽부터 사용 가능한 스택 찾기
        for (int i = currentStackBars.Length - 1; i >= 0; i--)
        {
            if (!isCoolingDown[i])
            {
                isCoolingDown[i] = true;
                cooldownQueue.Enqueue(i); // usedIndex만 저장
                Debug.Log($"[DASH] 사용된 스택: 실제 인덱스={i}, 시각적 인덱스={nextVisualIndex}");

                nextVisualIndex++;
                currentStacks--;
                UpdateStackUI();
                TryStartNextCooldown();
                return;
            }
        }
    }

    private void TryStartNextCooldown()
    {
        if (isCooldownRunning || cooldownQueue.Count == 0)
            return;

        int usedIndex = cooldownQueue.Dequeue();
        int visualIndex = visualCooldownIndex++;

        Debug.Log($"[COOLDOWN 시작] 실제 인덱스={usedIndex}, 시각적 인덱스={visualIndex}");

        StartCoroutine(CooldownRoutine(usedIndex, visualIndex));
    }

    private IEnumerator CooldownRoutine(int usedIndex, int visualIndex)
    {
        isCooldownRunning = true;

        float timer = cooldownTime;
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            float ratio = 1f - Mathf.Clamp01(timer / cooldownTime);
            currentStackBars[visualIndex].fillAmount = ratio;
            yield return null;
        }

        currentStackBars[visualIndex].fillAmount = 1f;
        isCoolingDown[usedIndex] = false;
        currentStacks++;
        Debug.Log($"[COOLDOWN 완료] 실제 스택 복구: {usedIndex}, 시각 바 초기화: {visualIndex}");

        UpdateStackUI();

        isCooldownRunning = false;

        if (currentStacks == currentStackBars.Length)
        {
            visualCooldownIndex = 0;
            Debug.Log("[RESET] 모든 스택 복구됨, 시각 인덱스 초기화");
        }

        TryStartNextCooldown();
    }

    private void UpdateStackUI()
    {
        int filled = 0;

        // 시각적으로 왼쪽부터 채움 (0 -> 1 -> 2)
        for (int i = 0; i < currentStackBars.Length; i++)
        {
            if (!isCoolingDown[i] && filled < currentStacks)
            {
                currentStackBars[i].fillAmount = 1f;
                filled++;
            }
            else if (!isCoolingDown[i])
            {
                currentStackBars[i].fillAmount = 0f;
            }
            // 쿨다운 중인 건 건드리지 않음 (Coroutine이 처리 중)
        }
        DebugPrintStackStatus();
    }

    void DebugPrintStackStatus()
    {
        string status = "";
        for (int i = 0; i < currentStackBars.Length; i++)
        {
            status += $"[{i}] fill: {currentStackBars[i].fillAmount:F2} | ";
        }
        Debug.Log($"[스택 상태] {status}");
    }
}
