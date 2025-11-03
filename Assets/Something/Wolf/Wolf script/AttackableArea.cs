using UnityEngine;

public class AttackableArea : MonoBehaviour
{
    private EnemyAI enemyAI;

    private void Awake()
    {
        enemyAI = GetComponentInParent<EnemyAI>();
        if (enemyAI == null)
            Debug.LogError("Goblin에서 EnemyAI를 찾을 수 없습니다! 오브젝트: " + gameObject.name);
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (enemyAI == null) return;

        // 플레이어 레이어 + 태그 둘 다 확인
        if (other.gameObject.layer == LayerMask.NameToLayer("Player") &&
            other.CompareTag("Player"))
        {
            enemyAI.playerAttackable = true;
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (enemyAI == null) return;

        if (other.gameObject.layer == LayerMask.NameToLayer("Player") &&
            other.CompareTag("Player"))
        {
            enemyAI.playerAttackable = true;
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player") &&
            other.CompareTag("Player"))
        {
            enemyAI.playerAttackable = false;
        }
    }
}
