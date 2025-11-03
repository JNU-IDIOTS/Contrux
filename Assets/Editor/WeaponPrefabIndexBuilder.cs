#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class WeaponPrefabIndexBuilder
{
    // 무기 프리팹이 들어있는 폴더들 (필요하면 추가)
    private static readonly string[] SearchFolders = new[]
{
    // 너가 썼다고 한 경로
    "Assets/PreFab/GeneratedWeaponPrefabs",

    // 흔히 쓰는 철자/복수형까지 모두 커버
    "Assets/Prefab/GeneratedWeaponPrefabs",
    "Assets/Prefabs/GeneratedWeaponPrefabs",
    "Assets/PreFab",
    "Assets/Prefab",
    "Assets/Prefabs"
};

    // 인덱스 SO 저장 위치 (런타임 로드를 위해 Resources에 둠)
    private const string IndexAssetPath = "Assets/Resources/WeaponPrefabIndex.asset";

    [MenuItem("Tools/Weapons/Rebuild Prefab Index")]
    public static void RebuildIndex()
    {
        var index = AssetDatabase.LoadAssetAtPath<WeaponPrefabIndex>(IndexAssetPath);
        if (index == null)
        {
            var dir = Path.GetDirectoryName(IndexAssetPath);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            index = ScriptableObject.CreateInstance<WeaponPrefabIndex>();
            AssetDatabase.CreateAsset(index, IndexAssetPath);
        }

        var map = new Dictionary<string, List<GameObject>>(System.StringComparer.OrdinalIgnoreCase);

        int totalFound = 0;
        int skippedBadName = 0;

        foreach (var folder in SearchFolders)
        {
            if (!AssetDatabase.IsValidFolder(folder))
            {
                Debug.LogWarning($"[IDX] 폴더 없음: {folder}");
                continue;
            }

            var guids = AssetDatabase.FindAssets("t:Prefab", new[] { folder });
            Debug.Log($"[IDX] {folder} 에서 Prefab {guids.Length}개 발견");
            totalFound += guids.Length;

            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (!go) continue;

                var tokens = go.name.Split('_');
                if (tokens.Length < 3)
                {
                    Debug.LogWarning($"[IDX] 이름 규칙 불일치 → 스킵: {go.name}  (경로: {path})");
                    skippedBadName++;
                    continue;
                }

                string key = $"{tokens[0]}_{tokens[1]}_{tokens[2]}";
                if (!map.TryGetValue(key, out var list))
                {
                    list = new List<GameObject>();
                    map.Add(key, list);
                }
                list.Add(go);
            }
        }

        Debug.Log($"[IDX] 총 스캔 결과: {totalFound}개 / 규칙 불일치 스킵: {skippedBadName}개");

        index.entries.Clear();
        foreach (var kv in map)
        {
            index.entries.Add(new WeaponPrefabIndex.Entry { key = kv.Key, prefabs = kv.Value });
            Debug.Log($"[IDX] 등록 키: {kv.Key} / 프리팹 {kv.Value.Count}개");
        }

        EditorUtility.SetDirty(index);
        AssetDatabase.SaveAssets();
        Debug.Log($"[WeaponPrefabIndex] 재빌드 완료: {index.entries.Count}개");
    }
    // 선택: 플레이 전 자동 보정 (인덱스 없으면 자동 생성)
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void EnsureIndexOnPlay()
    {
        var idx = Resources.Load<WeaponPrefabIndex>("WeaponPrefabIndex");
        if (idx == null)
        {
            RebuildIndex();
            Debug.Log("[WeaponPrefabIndex] 없어서 자동 생성했어요.");
        }
    }
}
#endif
