using System.Threading;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class ScanMaxHP : MonoBehaviour 
{
    public static float detectedMaxHP;
    
    public void OnTriggerEnter2D(Collider2D other)
    {
        // 들어온 오브젝트에 MonsterHP 컴포넌트가 있는지 확인
        if (other.TryGetComponent<MonsterHP>(out MonsterHP monster))
        {
            detectedMaxHP = monster.maxHP;
            Debug.Log($"적 감지됨: {other.name}, 최대 체력: {detectedMaxHP}");

        }
        SoulBuffDamageUp.MaxHpDamageUp();


    }
}
