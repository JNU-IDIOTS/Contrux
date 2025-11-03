using UnityEngine;

public class BossSummoner : MonoBehaviour
{
    [Header("Boss 설정")]
    [SerializeField] private GameObject bossPrefab;   // 소환할 보스 프리팹
    [SerializeField] private Transform spawnPoint;    // 보스가 나타날 위치

    [Header("플레이어 감지")]
    [SerializeField] private string playerTag = "Player";
    private bool playerInRange = false;
    private bool bossSpawned = false;

    void Update()
    {
        if (playerInRange && !bossSpawned)
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                SpawnBoss();
            }
        }
    }

    private void SpawnBoss()
    {
        if (bossPrefab != null && spawnPoint != null)
        {
            Instantiate(bossPrefab, spawnPoint.position, spawnPoint.rotation);
            bossSpawned = true; // 중복 소환 방지
        }
        else
        {
            Debug.LogWarning("BossSummoner: bossPrefab 또는 spawnPoint가 지정되지 않음");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = false;
        }
    }
}
