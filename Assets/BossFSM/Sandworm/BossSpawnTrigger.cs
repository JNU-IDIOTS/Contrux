using UnityEngine;

public class BossSpawnTrigger : MonoBehaviour
{
    [SerializeField] private GameObject bossObject; // 활성화할 보스 오브젝트

    private bool bossSpawned = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!bossSpawned && other.CompareTag("Player"))
        {
            bossObject.SetActive(true);  // 보스 등장
            bossSpawned = true;          // 중복 스폰 방지
            Debug.Log("보스가 스폰되었습니다!");
        }
    }
}
