using System.Collections.Generic;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;

public class DropManager : MonoBehaviour
{
    public static DropManager Instance { get; private set; }

    public List<DropTableData> stageTables;

    // 인덱스 SO (Resources에 저장된 걸 런타임에 로드)
    private WeaponPrefabIndex prefabIndex;

    public void Initialize(string jsonPath)
    {
        stageTables = DropTableLoader.LoadDropTables(jsonPath);
    }

    void Start()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "GreedStageData.json");
        Initialize(path);

        foreach (var table in stageTables)
        {
            //Debug.Log($"[드랍테이블] {table.stageName} 무기 드랍 확률: {table.weaponDropRate}%");

            table.allowedTypes = new List<WeaponType>();
            foreach (var str in table.allowedTypeStrings)
            {
                if (System.Enum.TryParse(str, true, out WeaponType result))
                    table.allowedTypes.Add(result);
                else
                    Debug.LogWarning($"WeaponType 변환 실패: {str}");
            }
        }

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 인덱스 로드(런타임용)
        prefabIndex = Resources.Load<WeaponPrefabIndex>("WeaponPrefabIndex");
        if (prefabIndex == null)
            Debug.LogWarning("[DropManager] WeaponPrefabIndex가 없습니다. (Tools > Weapons > Rebuild Prefab Index 실행 필요)");
    }

    public void SpawnDrop(Vector2 dropPosition)
    {
        string currentScene = SceneManager.GetActiveScene().name.Trim().ToLower();
        SpawnDrop(currentScene, dropPosition);
    }

    public void SpawnDrop(string stageName, Vector2 dropPosition)
    {
        string normalized = stageName.Trim().ToLower();
        var table = stageTables.Find(t => t.stageName.Trim().ToLower() == normalized);
        if (table == null)
        {
            Debug.LogWarning($"스테이지 {stageName}에 대한 드랍 테이블이 없습니다.");
            return;
        }

        float roll = Random.Range(0f, 100f);
        if (roll > table.weaponDropRate)
        {
            Debug.Log($"무기 드랍되지 않음 (확률: {table.weaponDropRate}%, roll: {roll:F2})");
            return;
        }

        WeaponGrade grade = RollGrade(table);
        WeaponType type = RollWeaponType(table.allowedTypes);
        int level = RollWeaponLevel(table.allowedLevels);

        GameObject prefab = LoadRandomWeaponPrefab(grade, type, level);
        if (prefab == null)
        {
            Debug.LogWarning($"프리팹을 찾을 수 없습니다: {grade}_{type}_{level}");
            return;
        }
        Instantiate(prefab, dropPosition, Quaternion.identity);
    }

    private WeaponGrade RollGrade(DropTableData table)
    {
        float roll = Random.Range(0f, 100f);
        float cumulative = 0f;

        cumulative += table.legendaryRate;
        if (roll < cumulative) return WeaponGrade.Legendary;

        cumulative += table.uniqueRate;
        if (roll < cumulative) return WeaponGrade.Unique;

        cumulative += table.epicRate;
        if (roll < cumulative) return WeaponGrade.Epic;

        cumulative += table.rareRate;
        if (roll < cumulative) return WeaponGrade.Rare;

        return WeaponGrade.Common;
    }

    private WeaponType RollWeaponType(List<WeaponType> allowed)
    {
        return allowed[Random.Range(0, allowed.Count)];
    }

    private int RollWeaponLevel(List<int> allowedLevels)
    {
        int index = Random.Range(0, allowedLevels.Count);
        return allowedLevels[index];
    }

    // ★ 핵심: 인덱스 우선, 에디터에선 보정 가능
    private GameObject LoadRandomWeaponPrefab(WeaponGrade grade, WeaponType type, int level)
    {
        string key = $"{grade}_{type}_{level}";
        Debug.Log($"[DropManager] 찾는 키: {key}");

        if (prefabIndex != null)
        {
            var list = prefabIndex.Get(key);
            if (list != null && list.Count > 0)
            {
                Debug.Log($"[DropManager] 인덱스에서 {list.Count}개 발견");
                return list[Random.Range(0, list.Count)];
            }
            else
            {
                Debug.LogWarning($"[DropManager] 인덱스에 없음: {key}");
            }
        }
        // 1) 런타임/에디터 공통: 인덱스 우선
        if (prefabIndex != null)
        {
            var list = prefabIndex.Get(key);
            if (list != null && list.Count > 0)
                return list[Random.Range(0, list.Count)];
        }

#if UNITY_EDITOR
        // 2) 에디터 보정: 인덱스 없거나 빌드 전이라면 AssetDatabase로 즉시 탐색
        string[] guids = AssetDatabase.FindAssets($"t:Prefab {key}", new[] { "Assets/PreFab/GeneratedWeaponPrefabs" });
        if (guids != null && guids.Length > 0)
        {
            string chosenPath = AssetDatabase.GUIDToAssetPath(guids[Random.Range(0, guids.Length)]);
            return AssetDatabase.LoadAssetAtPath<GameObject>(chosenPath);
        }
#endif

        return null;
    }
}
