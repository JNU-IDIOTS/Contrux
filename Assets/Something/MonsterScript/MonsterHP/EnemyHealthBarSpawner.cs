/*using UnityEngine;

public class EnemyHealthBarSpawner : MonoBehaviour
{
    [Header("UI Canvas (World Space)")]
    [SerializeField] private Canvas uiCanvas;

    [Header("Health Bar Prefab")]
    [SerializeField] private GameObject healthBarPrefab;

    [Header("Layer Filter (Optional)")]
    [Tooltip("체력바를 띄울 Enemy 레이어만 체크 (안 쓰면 All on)")]
    [SerializeField] private LayerMask enemyLayerMask = ~0;

    [Header("Default Bar Offset")]
    [SerializeField] private Vector3 defaultBarOffset = new Vector3(0, 1.5f, 0);

    void Start()
    {
        var allHps = Object.FindObjectsByType<MonsterHP>(FindObjectsSortMode.None);
        Debug.Log($"[Spawner] MonsterHP 개수: {allHps.Length}");

        foreach (var hp in allHps)
        {
            if (((1 << hp.gameObject.layer) & enemyLayerMask) == 0)
                continue;

            var settings = hp.GetComponent<EnemyHealthBarSettings>();
            var offset = settings != null
                           ? settings.healthBarOffset
                           : defaultBarOffset;

            // Instantiate
            var barGO = Instantiate(healthBarPrefab, uiCanvas.transform);
            Debug.Log($"[Spawner] Instantiate healthBarPrefab for {hp.gameObject.name}");

            // Initialize
            var barUI = barGO.GetComponent<EnemyHealthBarUI>();
            if (barUI == null)
            {
                Debug.LogError("[Spawner] Instantiate된 오브젝트에 EnemyHealthBarUI가 없습니다!");
                continue;
            }
            barUI.Initialize(hp, hp.transform, offset);
            Debug.Log($"[Spawner] Initialized HealthBar for {hp.gameObject.name} with offset {offset}");
        }
    }
}*/
