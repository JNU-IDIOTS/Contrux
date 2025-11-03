using UnityEngine;
using System.Collections;
using Unity.VisualScripting;
public class SoulBuffShield
{
    public static bool shieldReady = false;
    public static bool shieldCanUse = false;
    public static bool DefenceSClicked = false;
    
    public static void ShieldReadyOn()
    {
        DefenceSClicked = true;
    }
    public static IEnumerator ShieldEvery30Seconds()
    {
        while (DefenceSClicked == true)
        {
            if (!shieldReady) // 아직 예약 안 된 상태에서만
            {
                shieldReady = true; // 예약 중으로 표시
                yield return new WaitForSeconds(30f);
                shieldCanUse = true;
                Debug.Log("쉴드 발동함");
            }
            else
            {
                yield return null; // 이미 예약됨 → 대기
            }
        }
    }
}
