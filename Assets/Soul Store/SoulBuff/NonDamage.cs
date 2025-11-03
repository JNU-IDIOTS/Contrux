using Unity.VisualScripting;
using UnityEngine;
using System.Collections;
using JetBrains.Annotations;

public static class NonDamage
{
    public static bool DefenceBClicked = false;
    public static bool negateNextDamage = false;
    public static bool isNegateReserved = false;
    public static void PreNonDamage()
    {
        DefenceBClicked = true;
    }
    public static IEnumerator InvokeEvery30Seconds()
    {
        while (DefenceBClicked == true)
        {
            if (!isNegateReserved) // 아직 예약 안 된 상태에서만
            {
                isNegateReserved = true; // 예약 중으로 표시
                yield return new WaitForSeconds(30f);
                negateNextDamage = true;
                Debug.Log("다음 피격 무효화 예약됨");
            }
            else
            {
                yield return null; // 이미 예약됨 → 대기
            }
        }
    }
}
