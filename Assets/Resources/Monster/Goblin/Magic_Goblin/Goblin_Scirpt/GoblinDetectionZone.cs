using UnityEngine;

public class GoblinDetectionZone : MonoBehaviour
{
    private MagicGoblinAI goblinAI;

    void Awake()
    {
        // 부모 오브젝트에서 AI 스크립트를 자동으로 찾는다
        goblinAI = GetComponentInParent<MagicGoblinAI>();
        if (goblinAI == null)
            Debug.LogError("부모에 MagicGoblinAI가 없습니다!", this);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            goblinAI.OnPlayerDetected(other.transform);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            goblinAI.OnPlayerLost(other.transform);
    }
}
