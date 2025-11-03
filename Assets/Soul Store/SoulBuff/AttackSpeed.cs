using Unity.VisualScripting;
using UnityEngine;
using System.Collections;

public class AttackSpeed : MonoBehaviour
{
    private static int _AttackCount;
    private const int maxAttackCount = 5;
    public static bool attack6Button = false;
    private static Coroutine resetCoroutine;
    private static MonoBehaviour coroutineRunner;  // 외부에서 넣어줄 것
    private const float resetDelay = 3f;

    private void Update()
    {
        Debug.Log($"공격 속도: {Ark_stat.GetAttackSpeed()}");
    }
    public static int AttackCount
    {
        get => _AttackCount;
        set => _AttackCount = Mathf.Clamp(value, 0, maxAttackCount);
    }

    public static void Attack6Button()
    {
        attack6Button = true;
    }
    public static void RegisterRunner(MonoBehaviour runner)
    {
        coroutineRunner = runner;
    }
    public static void AttackSpeedUP()
    {
        if (attack6Button && (SteamPunk_Attack.AttackCountReady || Magic_Attack.AttackCountReady))
        {
            AttackCount++; // 자동으로 Clamp 적용됨
            Debug.Log($"[공속 증가] 현재 값: {AttackCount}/{maxAttackCount}");
            SteamPunk_Attack.AttackCountReady = false;
            Magic_Attack.AttackCountReady = false;

            RestartResetTimer();
        }
    }

    private static void RestartResetTimer()
    {
        if (resetCoroutine != null && coroutineRunner != null)
        {
            coroutineRunner.StopCoroutine(resetCoroutine);
        }

        if (coroutineRunner != null)
        {
            resetCoroutine = coroutineRunner.StartCoroutine(ResetAfterDelay());
        }
    }

    private static IEnumerator ResetAfterDelay()
    {
        yield return new WaitForSeconds(resetDelay);
        AttackCount = 0;
        Debug.Log("[공속 스택] 3초 동안 공격 없어서 스택 초기화됨");
    }

    //public static void SpeedUPSize()
    //{
    //    if(AttackCount == 0)
    //    {
    //        Ark_stat.attackSpeedMultiplier = Ark_stat.GetAttackSpeed() * 1.1f;
    //        Debug.Log($"공속 증가함/시발 왜");
    //    }
    //    else if(AttackCount == 1)
    //    {
    //        Ark_stat.attackSpeedMultiplier = Ark_stat.GetAttackSpeed() * 1.2f;
    //        //Debug.Log($"공속 증가함");

    //    }
    //    else if (AttackCount == 2)
    //    {
    //        Ark_stat.attackSpeedMultiplier = Ark_stat.GetAttackSpeed() * 1.3f   ;
    //        //Debug.Log($"공속 증가함");

    //    }
    //    else if (AttackCount == 3)
    //    {
    //        Ark_stat.attackSpeedMultiplier = Ark_stat.GetAttackSpeed() * 1.4f;
    //        //Debug.Log($"공속 증가함");

    //    }
    //    else if (AttackCount == 4)
    //    {
    //        Ark_stat.attackSpeedMultiplier = Ark_stat.GetAttackSpeed() * 1.5f;
    //        //Debug.Log($"공속 증가함");

    //    }
    //    else if (AttackCount == 5)
    //    {
    //        Ark_stat.attackSpeedMultiplier = Ark_stat.GetAttackSpeed() * 1.6f;
    //        //Debug.Log($"공속 증가함");

    //    }
    //}
    public static void SpeedUPSize()
    {
        // Ark_stat 안에 baseAttackSpeed가 있다면 그걸 가져와 곱하세요.
        // 없다면 별도 static으로 저장.
        float[] mult = { 1.10f, 1.20f, 1.30f, 1.40f, 1.50f, 1.60f };
        int idx = Mathf.Clamp(AttackCount, 0, maxAttackCount);
        Ark_stat.attackSpeedMultiplier = mult[idx];
    }
}