using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SynergyManager : MonoBehaviour
{
    [Header("시너지 설정")]
    [SerializeField] private SynergyData[] allSynergies;
    [SerializeField] private InventoryUI inventoryUI;
    
    [Header("디버그 정보")]
    [SerializeField] private bool showDebugLogs = false;
    
    // 현재 활성화된 시너지 정보 (시너지ID -> 레벨)
    private Dictionary<string, int> activeSynergies = new Dictionary<string, int>();
    
    // 태그별 아이템 개수 (가중치 포함)
    private Dictionary<string, int> tagCounts = new Dictionary<string, int>();
    
    // 태그별 실제 아이템 리스트 (디버그용)
    private Dictionary<string, List<ItemData>> tagItems = new Dictionary<string, List<ItemData>>();
    
    // 시너지 변경 이벤트
    public System.Action<Dictionary<string, int>> OnSynergiesChanged;
    public System.Action<Dictionary<string, int>> OnTagCountsChanged;
    
    // 공개 속성들
    public Dictionary<string, int> ActiveSynergies => new Dictionary<string, int>(activeSynergies);
    public Dictionary<string, int> TagCounts => new Dictionary<string, int>(tagCounts);
    public SynergyData[] AllSynergies => allSynergies;

    void Start()
    {
        LoadAllSynergies();
        ConnectToInventory();
        
        // 초기 시너지 계산
        if (inventoryUI != null)
        {
            RecalculateAllSynergies();
        }
    }
    
    /// <summary>
    /// Resources 폴더에서 모든 시너지 데이터를 로드
    /// </summary>
    private void LoadAllSynergies()
    {
        if (allSynergies == null || allSynergies.Length == 0)
        {
            allSynergies = Resources.LoadAll<SynergyData>("Synergies");
            
            if (showDebugLogs)
                Debug.Log($"[SynergyManager] {allSynergies.Length}개의 시너지를 로드했습니다.");
        }
    }
    
    /// <summary>
    /// InventoryUI와 연결
    /// </summary>
    private void ConnectToInventory()
    {
        if (inventoryUI == null)
            inventoryUI = FindObjectOfType<InventoryUI>();
            
        // 여기서 InventoryUI의 아이템 변경 이벤트를 구독해야 함
        // 현재는 수동으로 RecalculateAllSynergies 호출
    }
    
    /// <summary>
    /// 모든 시너지를 재계산 (인벤토리 변경 시 호출)
    /// </summary>
    public void RecalculateAllSynergies()
    {
        var allItems = GetAllInventoryItems();
        RecalculateSynergies(allItems);
    }
    
    /// <summary>
    /// 인벤토리의 모든 아이템을 수집
    /// </summary>
    private List<ItemData> GetAllInventoryItems()
    {
        var allItems = new List<ItemData>();
        
        if (inventoryUI == null) return allItems;
        
        // 그리드 아이템들 추가 (InventoryUI의 _gridItems 배열에서)
        // 현재 InventoryUI의 _gridItems가 private이므로 public 접근자가 필요
        
        // 초기 아이템들에서 가져오기 (임시 방법)
        foreach (var item in inventoryUI.initialItems)
        {
            if (item != null) allItems.Add(item);
        }
        
        return allItems;
    }
    
    /// <summary>
    /// 주어진 아이템 목록으로 시너지 재계산
    /// </summary>
    public void RecalculateSynergies(List<ItemData> items)
    {
        // 1. 태그별 개수 집계 (가중치 포함)
        CountItemsByTag(items);
        
        // 2. 시너지 활성화 계산
        var newActiveSynergies = CalculateActiveSynergies();
        
        // 3. 상호 배타 시너지 처리
        ProcessIncompatibleSynergies(newActiveSynergies);
        
        // 4. 변경사항이 있으면 이벤트 발생
        if (HasSynergyChanges(newActiveSynergies))
        {
            activeSynergies = newActiveSynergies;
            OnSynergiesChanged?.Invoke(activeSynergies);
            
            if (showDebugLogs)
                LogSynergyChanges();
        }
        
        // 태그 카운트 변경 이벤트
        OnTagCountsChanged?.Invoke(tagCounts);
    }
    
    /// <summary>
    /// 태그별 아이템 개수를 집계 (티어와 가중치 고려)
    /// </summary>
    private void CountItemsByTag(List<ItemData> items)
    {
        tagCounts.Clear();
        tagItems.Clear();
        
        foreach (var item in items)
        {
            if (item == null || !item.IsSynergyItem()) continue;
            
            foreach (var tag in item.synergyTags)
            {
                // 가중치가 적용된 기여도 계산
                int contribution = item.GetSynergyContribution();
                
                tagCounts[tag] = tagCounts.GetValueOrDefault(tag, 0) + contribution;
                
                // 디버그용 아이템 목록 저장
                if (!tagItems.ContainsKey(tag))
                    tagItems[tag] = new List<ItemData>();
                tagItems[tag].Add(item);
            }
        }
    }
    
    /// <summary>
    /// 활성화될 시너지들을 계산
    /// </summary>
    private Dictionary<string, int> CalculateActiveSynergies()
    {
        var newActiveSynergies = new Dictionary<string, int>();
        
        foreach (var synergy in allSynergies)
        {
            if (synergy == null) continue;
            
            // 해당 태그의 아이템 개수 확인
            int itemCount = tagCounts.GetValueOrDefault(synergy.requiredTag, 0);
            
            // 최대 활성화 가능한 레벨 계산
            int maxLevel = synergy.GetMaxLevel(itemCount);
            
            if (maxLevel > 0)
            {
                newActiveSynergies[synergy.synergyId] = maxLevel;
            }
        }
        
        return newActiveSynergies;
    }
    
    /// <summary>
    /// 상호 배타 시너지 처리
    /// </summary>
    private void ProcessIncompatibleSynergies(Dictionary<string, int> synergies)
    {
        var toRemove = new HashSet<string>();
        
        foreach (var kvp in synergies)
        {
            var synergy = GetSynergyData(kvp.Key);
            if (synergy == null) continue;
            
            // 상호 배타 시너지들을 비활성화 목록에 추가
            foreach (var incompatibleId in synergy.incompatibleSynergyIds)
            {
                if (synergies.ContainsKey(incompatibleId))
                {
                    // 더 높은 레벨의 시너지를 우선
                    if (synergies[incompatibleId] < kvp.Value)
                    {
                        toRemove.Add(incompatibleId);
                    }
                    else if (synergies[incompatibleId] > kvp.Value)
                    {
                        toRemove.Add(kvp.Key);
                    }
                }
            }
        }
        
        // 제거
        foreach (var id in toRemove)
        {
            synergies.Remove(id);
        }
    }
    
    /// <summary>
    /// 시너지 변경사항이 있는지 확인
    /// </summary>
    private bool HasSynergyChanges(Dictionary<string, int> newSynergies)
    {
        if (activeSynergies.Count != newSynergies.Count)
            return true;
            
        foreach (var kvp in newSynergies)
        {
            if (!activeSynergies.ContainsKey(kvp.Key) || 
                activeSynergies[kvp.Key] != kvp.Value)
                return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// 시너지 데이터 찾기
    /// </summary>
    public SynergyData GetSynergyData(string synergyId)
    {
        return allSynergies.FirstOrDefault(s => s.synergyId == synergyId);
    }
    
    /// <summary>
    /// 특정 태그의 현재 레벨과 다음 레벨 정보 반환
    /// </summary>
    public (int currentLevel, int nextThreshold, string synergyName) GetSynergyInfo(string tag)
    {
        var synergy = allSynergies.FirstOrDefault(s => s.requiredTag == tag);
        if (synergy == null) return (0, -1, "");
        
        int itemCount = tagCounts.GetValueOrDefault(tag, 0);
        int currentLevel = synergy.GetMaxLevel(itemCount);
        int nextThreshold = synergy.GetItemsNeededForNextLevel(itemCount);
        
        return (currentLevel, nextThreshold, synergy.displayName);
    }
    
    /// <summary>
    /// 디버그용 로그
    /// </summary>
    private void LogSynergyChanges()
    {
        Debug.Log("=== 시너지 상태 변경 ===");
        
        foreach (var kvp in tagCounts)
        {
            Debug.Log($"[{kvp.Key}] 아이템 {kvp.Value}개");
        }
        
        foreach (var kvp in activeSynergies)
        {
            var synergy = GetSynergyData(kvp.Key);
            string name = synergy?.displayName ?? kvp.Key;
            Debug.Log($"[활성화] {name} Lv.{kvp.Value}");
        }
    }
    
    /// <summary>
    /// 외부에서 강제로 시너지 재계산 요청
    /// </summary>
    public void ForceRecalculate()
    {
        RecalculateAllSynergies();
    }
}