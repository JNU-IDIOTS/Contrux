using UnityEngine;
using UnityEngine.Rendering;

public class PrepareDamageDiscount
{
    
    public static void PreDamageDiscount()
    {
        Debug.Log("PreDamageDiscount 실행됨");
        PlayerHealth.discountDamage += 0.05f;
        Debug.Log($"데미지 감소율 : {PlayerHealth.discountDamage}");
    }

   
}
