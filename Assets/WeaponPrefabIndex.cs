using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponPrefabIndex", menuName = "Game/Weapon Prefab Index")]
public class WeaponPrefabIndex : ScriptableObject
{
    [System.Serializable]
    public class Entry
    {
        public string key;                // 예: "Epic_Spear_30"
        public List<GameObject> prefabs;  // 해당 키의 프리팹들
    }

    public List<Entry> entries = new();

    private Dictionary<string, List<GameObject>> _map;

    public void BuildCache()
    {
        if (_map != null) return;

        _map = new Dictionary<string, List<GameObject>>(System.StringComparer.OrdinalIgnoreCase);

        foreach (var e in entries)
        {
            if (e == null || string.IsNullOrWhiteSpace(e.key)) continue;

            var k = e.key.Trim();
            if (!_map.TryGetValue(k, out var list))
            {
                list = new List<GameObject>();
                _map.Add(k, list);
            }
            if (e.prefabs != null)
            {
                // null 제거
                for (int i = 0; i < e.prefabs.Count; i++)
                    if (e.prefabs[i] != null) list.Add(e.prefabs[i]);
            }
        }
    }

    public List<GameObject> Get(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) return null;
        BuildCache();
        return _map != null && _map.TryGetValue(key.Trim(), out var list) ? list : null;
    }
}
