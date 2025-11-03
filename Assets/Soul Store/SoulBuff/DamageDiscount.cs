using Unity.VisualScripting;
using UnityEngine;
using System.Collections;
using JetBrains.Annotations;

public class DamageDiscount : MonoBehaviour
{
    EnemyAI enemyAI;
    
    public static bool SoulBClicked = false;

    void Awake()
    {
        if (enemyAI == null)
        {
            enemyAI = GameObject.FindWithTag("Enemy")?.GetComponent<EnemyAI>();
        }
    }

    public static void enemyAttackDis()
    {
        Debug.Log("[enemyAttackDis] 호출됨");
        SoulBClicked = true;
        PrepareDamageDiscount.PreDamageDiscount();
        Debug.Log("버튼 트리거 ON");
    }

   

}
