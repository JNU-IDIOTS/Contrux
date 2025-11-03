using UnityEngine;

public class Dome:MonoBehaviour
{
    public Boss1_SkillManager SkillManager;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Boss1"))  // 충돌 대상 태그 확인 (필요 시 조정)
        {
            SkillManager.CantDash = true;
            Debug.Log("대쉬 금지 영역 진입");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Boss1"))
        {
            SkillManager.CantDash = false;
            Debug.Log("대쉬 금지 영역 탈출");
        }
    }
}
