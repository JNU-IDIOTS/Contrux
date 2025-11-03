using System.Linq;
using UnityEngine;

public class SceneMove : MonoBehaviour
{
    [Header("오브젝트 참조")]
    [SerializeField] GameObject Player;             // 플레이어 오브젝트
    [SerializeField] GameObject Portal;             // 포탈 본체
    [SerializeField] GameObject PortalImpact;       // 포탈 활성화 이펙트

    [Header("씬 흐름")]
    [SerializeField] SceneFlowLoader flowLoader;    // 자동 씬 전환을 위한 SceneFlowLoader 연결

    private int layerToFind;
    private bool layerExistsInScene;
    private bool isPlayerInPortalZone = false;

    void Start()
    {
        layerToFind = LayerMask.NameToLayer("enemy");
        UpdateEnemyPresence();
    }

    void Update()
    {
        UpdateEnemyPresence();

        // F 키 누르고 플레이어가 포탈 범위 안에 있으며, 포탈이 활성화되어 있으면 씬 전환
        if (isPlayerInPortalZone && PortalImpact.activeInHierarchy && Input.GetKeyDown(KeyCode.F))
        {
            SaveSystemManager.SaveOnSceneTransition(); //씬이 바뀔때의 저장 시스템 통합관리 이 부분만 수정
            flowLoader.LoadNextScene(); // 여기서 실제 씬 전환 발생

        }
    }

    void UpdateEnemyPresence()
    {
        GameObject[] allObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);

        layerExistsInScene = allObjects.Any(obj => obj.layer == layerToFind);
        int enemyCount = 0;

        foreach (var obj in allObjects)
        {
            if (obj.layer == layerToFind)
            {
                enemyCount++;
                Debug.Log($"Enemy Object: {obj.name}, Active: {obj.activeSelf}");
            }
        }

        layerExistsInScene = enemyCount > 0;
        Debug.Log($"[SceneMove] Enemy count: {enemyCount}, Portal 활성화 여부: {!layerExistsInScene}");

        // Enemy가 없으면 포탈 활성화
        PortalImpact.SetActive(!layerExistsInScene);
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInPortalZone = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInPortalZone = false;
        }
    }
}
