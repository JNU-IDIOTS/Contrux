using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
/// 씬 로드 시 자동으로 \"SpawnPoint\" 위치를 찾아 플레이어를 그 위치로 이동시킴.
public class PlayerSpawnManager : MonoBehaviour
{
    [SerializeField] private GameObject player; // Player는 외부에서 할당되거나 자동 탐색

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(DelayedSpawn());
    }

    private IEnumerator DelayedSpawn()
    {
        yield return null; // 한 프레임 기다리면 씬 안의 오브젝트들이 모두 초기화 완료됨

        SpawnPoint spawn = FindFirstObjectByType<SpawnPoint>(); // 이름 대신 타입으로 찾기
        GameObject player = GameObject.FindWithTag("Player");

        Debug.Log("[SpawnTest] SpawnPoint 찾음: " + (spawn != null));
        Debug.Log("[SpawnTest] Player 찾음: " + (player != null));

        if (spawn != null && player != null)
        {
            player.transform.position = spawn.transform.position;
            ChildPositionRestorer restorer = player.GetComponent<ChildPositionRestorer>();
            restorer?.RestoreLocalPositions();
            Debug.Log($"[SpawnTest] 플레이어 이동 완료 → {spawn.transform.position}");
        }
        else
        {
            Debug.LogWarning("[SpawnTest] 위치 이동 실패");
        }
    }
}