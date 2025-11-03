using UnityEngine;
using System.Collections;

public class SoulBuffAttack : MonoBehaviour
{
    private bool isBuffReady = false;
    private bool isBuffOnCooldown = false;
    public static bool isA5ButtonUnlocked = false;

    [SerializeField] private float buffMultiplier = 2f;
    [SerializeField] private float buffDuration = 3f;
    [SerializeField] private float buffCooldown = 10f;

    public bool IsBuffActive { get; private set; } = false;
    public float BuffMultiplier => buffMultiplier;

    public static void ButtonUnlockScan()
    {
        isA5ButtonUnlocked = true;
    }

    // [1] 피격 시 버프 준비
    public void TryActivateBuffAfterHit()
    {
        if (isA5ButtonUnlocked && !isBuffOnCooldown)
        {
            isBuffReady = true;
            Debug.Log("피격됨 → 다음 공격 버프 준비 완료");
            
        }
    }

    // [2] 공격 시 버프 발동
    public bool TryBuffAttack()
    {
        if (isBuffReady && !isBuffOnCooldown)
        {
            StartCoroutine(BuffRoutine());
            return true;
        }
        return false;
    }

    // [3] 버프 활성화 및 쿨타임 처리
    private IEnumerator BuffRoutine()
    {
        IsBuffActive = true;
        isBuffReady = false;

        Debug.Log("버프 시작 → 3초 지속");

        yield return new WaitForSeconds(buffDuration);

        IsBuffActive = false;
        isBuffOnCooldown = true;

        Debug.Log("버프 종료 → 10초 쿨타임 시작");

        yield return new WaitForSeconds(buffCooldown); // 여기서 전체 10초 세기
        isBuffOnCooldown = false;

        Debug.Log("쿨타임 종료 → 다시 피격 시 발동 가능");
    }
}
