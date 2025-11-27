using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;

public class InventoryUI : MonoBehaviour
{
    public UIDocument uiDocument;
    
    public int columns = 3;
    public int rows = 3;

    // 인스펙터에서 설정하는 초기 아이템 목록
    public List<ItemData> initialItems = new List<ItemData>();

    // [변경] 런타임에서 그리드 위치를 관리할 배열 (null이면 빈 칸)
    private ItemData[] _gridItems;

    private VisualElement _root;
    private VisualElement slotGrid; 
    private List<VisualElement> gridSlots = new List<VisualElement>(); 
    private List<VisualElement> equipmentSlots = new List<VisualElement>(); 

    private VisualElement _playerStatsContainer;
    private VisualElement _itemDetailsContainer;

    private Label nameLabel;
    private Label descriptionLabel;
    private Label statsLabel;
    private VisualElement iconImage;

    // 컨텍스트 메뉴 관련
    private VisualElement _contextMenu;
    private Button _btnMove;
    private Button _btnDiscard;
    private SlotInfo _selectedSlotInfo; 
    
    // 아이템 이동 관련
    private bool _isMovingItem = false;
    private SlotInfo _sourceSlotInfo;

    // [추가] 시너지 UI 관련
    private VisualElement _synergyContainer;
    private ScrollView _synergyScrollView;
    private Label _synergyCountText;
    private VisualElement _noSynergyMessage;
    private SynergyManager _synergyManager;
    private Dictionary<string, VisualElement> _synergyUIElements = new Dictionary<string, VisualElement>();
    private int _lastActiveSynergyCount = 0;

    private class SlotInfo
    {
        public int index; // 그리드 슬롯의 경우 0~8, 장비 슬롯은 별도 처리
        public ItemData item;
        public bool isEquipment; 
        public VisualElement visualElement;
    }

    void OnEnable()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying) return;
#endif
        var doc = uiDocument != null ? uiDocument : GetComponent<UIDocument>();
        if (doc == null) return;

        _root = doc.rootVisualElement;
        _root.RegisterCallback<PointerDownEvent>(OnRootClicked);

        slotGrid = _root.Q<VisualElement>("InventoryGrid");
        _playerStatsContainer = _root.Q<VisualElement>("PlayerStatsContainer");
        _itemDetailsContainer = _root.Q<VisualElement>("ItemDetailsContainer");

        nameLabel = _root.Q<Label>("ItemName");
        descriptionLabel = _root.Q<Label>("ItemDescription");
        statsLabel = _root.Q<Label>("ItemSubInfo");
        iconImage = _root.Q<VisualElement>("ItemIcon");

        _contextMenu = _root.Q<VisualElement>("ContextMenu");
        _btnMove = _root.Q<Button>("BtnMove");
        _btnDiscard = _root.Q<Button>("BtnDiscard");

        // [추가] 시너지 UI 요소들 가져오기
        _synergyContainer = _root.Q<VisualElement>("SynergyContainer");
        _synergyScrollView = _root.Q<ScrollView>("SynergyScrollView");
        _synergyCountText = _root.Q<Label>("SynergyCountText");
        _noSynergyMessage = _root.Q<VisualElement>("NoSynergyMessage");

        if (_btnMove != null) _btnMove.clicked += OnMoveClicked;
        if (_btnDiscard != null) _btnDiscard.clicked += OnDiscardClicked;

        // [중요] 그리드 배열 초기화
        int totalGridSize = rows * columns;
        _gridItems = new ItemData[totalGridSize];

        // [추가] 시너지 매니저 찾기 및 초기화
        InitializeSynergySystem();

        InitializeSlots();
        LoadInitialItemsToGrid(); // 초기 아이템을 그리드 배열에 배치
        DistributeItems();        // UI 그리기
        ShowPlayerStats();

        // [추가] 시너지 UI 업데이트
        UpdateSynergyUI();
        
        // [추가] Initial Items 스냅샷 초기화
        UpdateInitialItemsSnapshot();
    }

    // [추가] 시너지 시스템 초기화
    private void InitializeSynergySystem()
    {
        _synergyManager = FindObjectOfType<SynergyManager>();
        if (_synergyManager == null)
        {
            Debug.LogWarning("[InventoryUI] SynergyManager를 찾을 수 없습니다.");
            return;
        }

        // 시너지 변경 이벤트 구독 (SynergyManager에 이벤트가 있다면)
        // _synergyManager.OnSynergyChanged.AddListener(OnSynergyChanged);
    }

    // [수정] 시너지 UI 업데이트 - 발동되지 않은 시너지도 모두 표시
    private void UpdateSynergyUI()
    {
        if (_synergyContainer == null) 
        {
            ShowNoSynergyMessage(true);
            return;
        }

        // 현재 아이템들을 기반으로 시너지 계산
        List<ItemData> allItems = GetAllItems();
        Dictionary<string, int> tagCounts = CalculateTagCounts(allItems);
        Dictionary<string, int> activeSynergies = GetActiveSynergies(tagCounts);
        
        // [변경] 활성화되지 않은 시너지도 포함하여 모든 보유 시너지 표시
        HashSet<string> allSynergyTags = new HashSet<string>(tagCounts.Keys);
        
        // 전체 시너지 개수 업데이트 (활성+비활성)
        UpdateSynergyCount(activeSynergies.Count, allSynergyTags.Count);

        // 시너지가 하나도 없으면 메시지 표시
        if (allSynergyTags.Count == 0)
        {
            ShowNoSynergyMessage(true);
            ClearSynergyElements();
            return;
        }

        // 시너지가 있으면 메시지 숨기고 모든 시너지 표시
        ShowNoSynergyMessage(false);
        UpdateAllSynergyElements(allSynergyTags, activeSynergies, tagCounts, allItems);
    }

    // [수정] 시너지 개수 텍스트 업데이트 - 활성/전체 표시
    private void UpdateSynergyCount(int activeCount, int totalCount)
    {
        if (_synergyCountText != null)
        {
            _synergyCountText.text = $"시너지: {activeCount}/{totalCount} 활성화";
        }
    }

    // [수정] 모든 시너지 UI 요소들 업데이트 - 레벨 높은 순으로 정렬
    // [수정] UpdateAllSynergyElements 함수에 디버깅 추가
    private void UpdateAllSynergyElements(HashSet<string> allSynergyTags, Dictionary<string, int> activeSynergies, Dictionary<string, int> tagCounts, List<ItemData> allItems)
    {
        // if (enableSynergyOrderDebug)
        // {
        //     Debug.Log($"=== UpdateAllSynergyElements 호출됨 ===");
        //     Debug.Log($"전체 시너지 태그 수: {allSynergyTags.Count}");
        //     Debug.Log($"활성 시너지 수: {activeSynergies.Count}");
        // }
        
        // 기존에 없는 시너지는 제거
        var toRemove = _synergyUIElements.Keys.Where(key => !allSynergyTags.Contains(key)).ToList();
        foreach (var key in toRemove)
        {
            RemoveSynergyElement(key);
            if (enableSynergyOrderDebug)
                Debug.Log($"[제거] 시너지 태그: {key}");
        }

        // [새로 추가] 시너지를 레벨순으로 정렬하기 위한 리스트 생성
        List<SynergyDisplayInfo> synergyDisplayList = new List<SynergyDisplayInfo>();

        // 모든 보유 시너지에 대해 정보 수집
        foreach (var synergyTag in allSynergyTags)
        {
            var synergyData = GetSynergyDataByTag(synergyTag);
            if (synergyData == null) continue;
            
            bool isActive = activeSynergies.ContainsKey(synergyData.synergyId);
            int level = isActive ? activeSynergies[synergyData.synergyId] : 0;
            int currentCount = tagCounts.ContainsKey(synergyTag) ? tagCounts[synergyTag] : 0;
            
            synergyDisplayList.Add(new SynergyDisplayInfo
            {
                synergyTag = synergyTag,
                level = level,
                currentCount = currentCount,
                isActive = isActive
            });
        }

        // [디버깅] 정렬 전 상태 백업
        var beforeSort = new List<SynergyDisplayInfo>(synergyDisplayList);

        // [새로 추가] 레벨 높은 순으로 정렬
        synergyDisplayList.Sort((a, b) => 
        {
            if (a.isActive != b.isActive)
                return b.isActive.CompareTo(a.isActive);
            if (a.level != b.level)
                return b.level.CompareTo(a.level);
            return b.currentCount.CompareTo(a.currentCount);
        });

        // [디버깅] 정렬 전후 비교
        DebugSynergySort(beforeSort, synergyDisplayList);

        // [새로 추가] 정렬된 순서대로 UI 컨테이너 재정렬
        RearrangeSynergyContainer(synergyDisplayList);
        
        // 정렬된 순서대로 UI 생성 및 업데이트
        foreach (var info in synergyDisplayList)
        {
            if (!_synergyUIElements.ContainsKey(info.synergyTag))
            {
                CreateSynergyElement(info.synergyTag);
                if (enableSynergyOrderDebug)
                    Debug.Log($"[생성] 새 시너지 UI: {info.synergyTag}");
            }
            
            UpdateSynergyElement(info.synergyTag, info.level, info.currentCount, allItems, info.isActive);
        }
        
        // [디버깅] 최종 컨테이너 순서 출력
        DebugSynergyOrder();
    }

    // [새로 추가] 시너지 표시 정보를 담는 클래스
    private class SynergyDisplayInfo
    {
        public string synergyTag;
        public int level;
        public int currentCount;
        public bool isActive;
    }

    // [수정] 시너지 UI 요소 생성 - 시너지별 색상 클래스 추가
    private void CreateSynergyElement(string synergyTag)
    {
        if (_synergyContainer == null) return;

        var synergyItem = new VisualElement();
        synergyItem.AddToClassList("synergy-item");
        synergyItem.AddToClassList(synergyTag.ToLower()); // 시너지별 색상을 위한 클래스

        // 헤더 (아이콘 + 이름 + 레벨)
        var header = new VisualElement();
        header.AddToClassList("synergy-header");

        var icon = new VisualElement();
        icon.AddToClassList("synergy-icon");
        icon.AddToClassList(synergyTag.ToLower()); // 시너지별 아이콘 색상
        
        var nameLabel = new Label(GetSynergyDisplayName(synergyTag));
        nameLabel.AddToClassList("synergy-name");
        
        var levelLabel = new Label();
        levelLabel.AddToClassList("synergy-level");

        header.Add(icon);
        header.Add(nameLabel);
        header.Add(levelLabel);

        // 진행도 컨테이너
        var progressContainer = new VisualElement();
        progressContainer.AddToClassList("synergy-progress-container");

        var progressText = new Label();
        progressText.AddToClassList("synergy-progress-text");

        var progressBar = new VisualElement();
        progressBar.AddToClassList("synergy-progress-bar");

        var progressFill = new VisualElement();
        progressFill.AddToClassList("synergy-progress-fill");
        progressBar.Add(progressFill);

        progressContainer.Add(progressText);
        progressContainer.Add(progressBar);

        // 효과 텍스트
        var effectText = new Label();
        effectText.AddToClassList("synergy-effect-text");

        // 다음 레벨 정보
        var nextLevelText = new Label();
        nextLevelText.AddToClassList("synergy-next-level");

        // [추가] 비활성화 상태 라벨


        synergyItem.Add(header);
        synergyItem.Add(progressContainer);
        synergyItem.Add(effectText);
        synergyItem.Add(nextLevelText);

        _synergyContainer.Add(synergyItem);
        _synergyUIElements[synergyTag] = synergyItem;
    }

    // [수정] 시너지 UI 요소 업데이트 - SynergyData의 borderColor 적용
    private void UpdateSynergyElement(string synergyTag, int level, int currentCount, List<ItemData> allItems, bool isActive)
    {
        if (!_synergyUIElements.ContainsKey(synergyTag)) return;

        var synergyItem = _synergyUIElements[synergyTag];
        
        // [중요] 활성화 상태에 따른 클래스 추가/제거
        if (isActive)
        {
            synergyItem.AddToClassList("active");
        }
        else
        {
            synergyItem.RemoveFromClassList("active");
        }

        // [새로 추가] SynergyData에서 색상 정보 가져와서 적용
        ApplySynergyBorderColor(synergyItem, synergyTag, level, isActive);

        // 레벨 업데이트
        var levelLabel = synergyItem.Q<Label>(className: "synergy-level");
        if (levelLabel != null)
        {
            if (isActive)
            {
                levelLabel.text = $"LV.{level}";
                levelLabel.AddToClassList("active");
            }
            else
            {
                levelLabel.text = "대기";
                levelLabel.RemoveFromClassList("active");
            }
        }

        // [수정] SynergyData의 실제 thresholds를 사용한 진행도 업데이트
        var progressText = synergyItem.Q<Label>(className: "synergy-progress-text");
        var progressFill = synergyItem.Q<VisualElement>(className: "synergy-progress-fill");
        
        if (progressText != null && progressFill != null)
        {
            var synergyData = GetSynergyDataByTag(synergyTag);
            if (synergyData != null)
            {
                int nextThreshold;
                
                if (isActive && level < synergyData.thresholds.Length)
                {
                    // 활성화된 상태: 다음 레벨까지
                    nextThreshold = synergyData.thresholds[level]; // level은 0부터 시작하므로 다음 레벨 임계값
                }
                else if (!isActive && synergyData.thresholds.Length > 0)
                {
                    // 비활성화 상태: 첫 번째 레벨까지
                    nextThreshold = synergyData.thresholds[0];
                }
                else
                {
                    nextThreshold = 0; // 최고 레벨이거나 thresholds가 없음
                }
                
                if (nextThreshold > 0)
                {
                    float progress = Mathf.Clamp01((float)currentCount / nextThreshold);
                    progressText.text = $"{currentCount} / {nextThreshold}";
                    progressFill.style.width = Length.Percent(progress * 100);
                }
                else
                {
                    progressText.text = $"{currentCount}";
                    progressFill.style.width = Length.Percent(100);
                }
            }
        }

        // [수정] SynergyData의 effectDescription 사용
        var effectText = synergyItem.Q<Label>(className: "synergy-effect-text");
        if (effectText != null)
        {
            if (isActive)
            {
                var synergyData = GetSynergyDataByTag(synergyTag);
                if (synergyData != null)
                {
                    var effect = synergyData.GetEffect(level);
                    if (effect != null && !string.IsNullOrEmpty(effect.effectDescription))
                    {
                        effectText.text = effect.effectDescription;
                    }
                    else
                    {
                        effectText.text = GetSynergyEffectDescription(synergyTag, level); // 기본값
                    }
                }
                else
                {
                    effectText.text = GetSynergyEffectDescription(synergyTag, level); // 기본값
                }
                effectText.AddToClassList("active");
            }
            else
            {
                effectText.text = "아직 발동되지 않음";
                effectText.RemoveFromClassList("active");
            }
        }


        // [추가] 비활성화 라벨 표시/숨김
        var inactiveLabel = synergyItem.Q<Label>(className: "synergy-inactive-label");
        if (inactiveLabel != null)
        {
            inactiveLabel.style.display = isActive ? DisplayStyle.None : DisplayStyle.Flex;
        }
    }

    // [새로 추가] SynergyData에서 테두리 색상을 가져와서 적용하는 함수
    private void ApplySynergyBorderColor(VisualElement synergyItem, string synergyTag, int level, bool isActive)
    {
        if (_synergyManager == null) return;

        // SynergyManager에서 해당 시너지 데이터 찾기
        var synergyData = GetSynergyDataByTag(synergyTag);
        if (synergyData == null) return;

        if (isActive && level > 0)
        {
            // 활성화된 경우: SynergyEffect에서 설정한 색상 사용
            var effect = synergyData.GetEffect(level);
            if (effect != null)
            {
                // Unity Color를 UI Toolkit의 스타일로 적용
                synergyItem.style.borderLeftColor = effect.borderColor;
                synergyItem.style.borderRightColor = effect.borderColor;
                synergyItem.style.borderTopColor = effect.borderColor;
                synergyItem.style.borderBottomColor = effect.borderColor;
            }
        }
        else
        {
            // 비활성화된 경우: 기본 회색 색상
            Color inactiveColor = new Color(0.67f, 0.67f, 0.67f, 1f); // #AAA
            synergyItem.style.borderLeftColor = inactiveColor;
            synergyItem.style.borderRightColor = inactiveColor;
            synergyItem.style.borderTopColor = inactiveColor;
            synergyItem.style.borderBottomColor = inactiveColor;
        }
    }

    // [추가] 시너지 없음 메시지 표시/숨김
    private void ShowNoSynergyMessage(bool show)
    {
        if (_noSynergyMessage != null)
        {
            _noSynergyMessage.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }

    // [추가] 모든 아이템 가져오기
    private List<ItemData> GetAllItems()
    {
        List<ItemData> allItems = new List<ItemData>();
        
        // 그리드 아이템들 추가
        foreach (var item in _gridItems)
        {
            if (item != null) allItems.Add(item);
        }
        
        // 장비 아이템들 추가
        foreach (var item in initialItems)
        {
            if (item != null && item.itemType != 0 && !allItems.Contains(item))
            {
                allItems.Add(item);
            }
        }
        
        return allItems;
    }

    // [추가] 태그별 아이템 개수 계산
    private Dictionary<string, int> CalculateTagCounts(List<ItemData> items)
    {
        Dictionary<string, int> tagCounts = new Dictionary<string, int>();
        
        foreach (var item in items)
        {
            if (item != null && item.synergyTags != null)
            {
                foreach (var tag in item.synergyTags)
                {
                    if (!string.IsNullOrEmpty(tag))
                    {
                        if (!tagCounts.ContainsKey(tag))
                            tagCounts[tag] = 0;
                        tagCounts[tag] += item.GetSynergyContribution();
                    }
                }
            }
        }
        
        return tagCounts;
    }

    // [수정] SynergyManager를 사용한 정확한 시너지 계산
    private Dictionary<string, int> GetActiveSynergies(Dictionary<string, int> tagCounts)
    {
        Dictionary<string, int> activeSynergies = new Dictionary<string, int>();
        
        if (_synergyManager == null) return activeSynergies;
        
        // SynergyManager를 통해 정확한 계산
        foreach (var kvp in tagCounts)
        {
            string tag = kvp.Key;
            int itemCount = kvp.Value;
            
            // SynergyData에서 해당 태그의 시너지 찾기
            var synergyData = _synergyManager.GetSynergyDataByTag(tag);
            if (synergyData != null)
            {
                // SynergyData의 실제 thresholds를 사용하여 레벨 계산
                int level = synergyData.GetMaxLevel(itemCount);
                
                if (level > 0)
                {
                    activeSynergies[synergyData.synergyId] = level; // 시너지 ID를 키로 사용
                    
                    Debug.Log($"[GetActiveSynergies] {synergyData.displayName}: {itemCount}개 아이템으로 레벨 {level} 활성화 " +
                             $"(thresholds: [{string.Join(", ", synergyData.thresholds)}])");
                }
            }
            else
            {
                Debug.LogWarning($"[GetActiveSynergies] 태그 '{tag}'에 대한 SynergyData를 찾을 수 없습니다.");
            }
        }
        
        return activeSynergies;
    }

    // [추가] 시너지 UI 요소 제거
    private void RemoveSynergyElement(string synergyTag)
    {
        if (_synergyUIElements.ContainsKey(synergyTag))
        {
            var element = _synergyUIElements[synergyTag];
            if (element.parent != null)
            {
                element.parent.Remove(element);
            }
            _synergyUIElements.Remove(synergyTag);
        }
    }

    // [추가] 모든 시너지 UI 요소 정리
    private void ClearSynergyElements()
    {
        foreach (var kvp in _synergyUIElements)
        {
            if (kvp.Value.parent != null)
            {
                kvp.Value.parent.Remove(kvp.Value);
            }
        }
        _synergyUIElements.Clear();
    }

    // [수정] SynergyData의 displayName을 사용한 시너지 표시 이름 가져오기 + 디버깅
    private string GetSynergyDisplayName(string tag)
    {
        var synergyData = GetSynergyDataByTag(tag);  // SynergyManager에서 데이터 가져오기
        
        if (synergyData != null && !string.IsNullOrEmpty(synergyData.displayName))
        {
            return synergyData.displayName;  // 👈 여기서 displayName 반환
        }
        
        return tag;  // 못 찾으면 태그 그대로 반환
    }

    // [수정] 시너지 효과 설명 가져오기 - SynergyData에서 색상도 함께 관리하도록 확장 가능
    private string GetSynergyEffectDescription(string tag, int level)
    {
        // 태그로 SynergyData 찾기
        var synergyData = GetSynergyDataByTag(tag);
        if (synergyData == null || level <= 0) return "효과 없음";
        
        // 해당 레벨의 SynergyEffect 가져오기
        var effect = synergyData.GetEffect(level);
        if (effect == null) return "효과 없음";
        
        List<string> effectParts = new List<string>();
        
        // 각 보너스 값이 0이 아니면 텍스트에 추가
        if (effect.hpBonus != 0)
            effectParts.Add($"체력 {(effect.hpBonus > 0 ? "+" : "")}{effect.hpBonus}");
    
        if (effect.physicalAttackBonus != 0)
            effectParts.Add($"물리 공격력 {(effect.physicalAttackBonus > 0 ? "+" : "")}{effect.physicalAttackBonus}");
    
        if (effect.magicalAttackBonus != 0)
            effectParts.Add($"마법 공격력 {(effect.magicalAttackBonus > 0 ? "+" : "")}{effect.magicalAttackBonus}");
    
        if (effect.defenseBonus != 0)
            effectParts.Add($"방어력 {(effect.defenseBonus > 0 ? "+" : "")}{effect.defenseBonus}");
    
        if (effect.criticalChanceBonus != 0)
            effectParts.Add($"치명타 확률 {(effect.criticalChanceBonus > 0 ? "+" : "")}{effect.criticalChanceBonus}%");
    
        if (effect.attackSpeedBonus != 0)
            effectParts.Add($"공격 속도 {(effect.attackSpeedBonus > 0 ? "+" : "")}{effect.attackSpeedBonus}%");
    
        if (effect.moveSpeedBonus != 0)
            effectParts.Add($"이동 속도 {(effect.moveSpeedBonus > 0 ? "+" : "")}{effect.moveSpeedBonus}%");
    
        // Effect Description이 있으면 추가
        if (!string.IsNullOrEmpty(effect.effectDescription))
            effectParts.Add(effect.effectDescription);
    
        // 효과가 하나도 없으면
        if (effectParts.Count == 0)
            return "효과 없음";
    
        // 쉼표로 연결해서 반환
        return string.Join(", ", effectParts);
    }


    // [개선] 더 안전한 SynergyManager 참조 관리
    private SynergyData GetSynergyDataByTag(string tag)
    {
        // 1. 캐시된 참조 확인
        if (_synergyManager == null) 
        {
            _synergyManager = FindObjectOfType<SynergyManager>();
            
            // 2. SynergyManager를 찾을 수 없으면 경고 로그
            if (_synergyManager == null)
            {
                Debug.LogWarning("[InventoryUI] SynergyManager를 씬에서 찾을 수 없습니다. 시너지 기능이 작동하지 않습니다.");
                return null;
            }
        }
        
        // 3. SynergyManager가 비활성화되었을 가능성 체크
        if (!_synergyManager.enabled || !_synergyManager.gameObject.activeInHierarchy)
        {
            Debug.LogWarning("[InventoryUI] SynergyManager가 비활성화되어 있습니다.");
            return null;
        }
        
        return _synergyManager.GetSynergyDataByTag(tag);
    }

    // 초기 아이템 리스트를 그리드 배열과 장비 슬롯에 분배
    void LoadInitialItemsToGrid()
    {
        // 그리드 배열 초기화
        for (int i = 0; i < _gridItems.Length; i++) _gridItems[i] = null;

        int currentGridIndex = 0;

        foreach (var item in initialItems)
        {
            if (item == null) continue;

            if (item.itemType == 0)
            {
                // 시너지 아이템 -> 그리드 빈 칸에 순서대로 배치
                if (currentGridIndex < _gridItems.Length)
                {
                    _gridItems[currentGridIndex] = item;
                    currentGridIndex++;
                }
            }
            // 장비 아이템은 별도 배열 관리 안함 (슬롯에 직접 할당하거나 initialItems 참조 유지)
            // 여기서는 장비 아이템은 UI 갱신 시 initialItems에서 직접 가져오는 기존 방식 유지
        }
    }

    private void OnRootClicked(PointerDownEvent evt)
    {
        CloseContextMenu();
        
        if (!_isMovingItem)
        {
            DeselectAllSlots();
            ShowPlayerStats();
        }
        // 이동 중일 때 배경 클릭하면 취소하고 싶다면 여기에 else { _isMovingItem = false; ... } 추가
    }

    void InitializeSlots()
    {
        equipmentSlots.Clear();
        for (int i = 0; i < 8; i++)
        {
            string slotName = $"장비 {i}";
            var eqSlot = _root.Q<VisualElement>(slotName);
            
            if (eqSlot != null)
            {
                SetSlotItem(eqSlot, null, true, i);
                equipmentSlots.Add(eqSlot);
            }
        }

        if (slotGrid != null)
        {
            slotGrid.Clear();
            gridSlots.Clear();
            int total = rows * columns;

            for (int i = 0; i < total; i++)
            {
                var slot = new VisualElement();
                slot.AddToClassList("item-slot");
                slot.AddToClassList("placeholder-slot");
                SetSlotItem(slot, null, false, i);
                slotGrid.Add(slot);
                gridSlots.Add(slot);
            }
        }
    }

    // UI 갱신 함수
    void DistributeItems()
    {
        // 1. 장비 슬롯 갱신 (기존 방식: initialItems 리스트 전체 검색)
        foreach (var slot in equipmentSlots) SetSlotItem(slot, null, true, -1);
        
        // 장비 아이템 찾아서 넣기
        foreach (var item in initialItems)
        {
            if (item != null && item.itemType != 0)
            {
                int slotIndex = item.itemType - 1;
                if (slotIndex >= 0 && slotIndex < equipmentSlots.Count)
                {
                    // 장비 슬롯 인덱스 전달
                    SetSlotItem(equipmentSlots[slotIndex], item, true, slotIndex);
                }
            }
        }

        // 2. 그리드 슬롯 갱신 (변경 방식: _gridItems 배열 기반)
        for (int i = 0; i < gridSlots.Count; i++)
        {
            if (i < _gridItems.Length)
            {
                SetSlotItem(gridSlots[i], _gridItems[i], false, i);
            }
            else
            {
                SetSlotItem(gridSlots[i], null, false, i);
            }
        }

        // [추가] 아이템 변경 시 시너지 UI 업데이트
        UpdateSynergyUI();
    }

    public void SetSlotItem(VisualElement slot, ItemData item, bool isEquipment, int index)
    {
        slot.Clear();
        slot.tooltip = string.Empty;
        
        slot.RemoveFromClassList("occupied");
        if (!isEquipment) slot.AddToClassList("placeholder-slot");

        // index 정보 저장 (이동 시 사용)
        var info = new SlotInfo { item = item, isEquipment = isEquipment, visualElement = slot, index = index };
        slot.userData = info;

        slot.UnregisterCallback<PointerDownEvent>(OnSlotPointerDown);
        slot.RegisterCallback<PointerDownEvent>(OnSlotPointerDown);

        if (item == null) return;

        if (!isEquipment) slot.RemoveFromClassList("placeholder-slot");
        slot.AddToClassList("occupied");

        var iconVe = new VisualElement();
        iconVe.style.width = Length.Percent(100);
        iconVe.style.height = Length.Percent(100);
        iconVe.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;
        iconVe.pickingMode = PickingMode.Ignore;

        if (item.icon != null)
        {
            iconVe.style.backgroundImage = new StyleBackground(item.icon);
        }
        
        slot.Add(iconVe);
        slot.tooltip = item.itemName;
    }

    private void OnSlotPointerDown(PointerDownEvent evt)
    {
        evt.StopPropagation();
        var ve = evt.currentTarget as VisualElement;
        if (ve != null) OnSlotClicked(ve, evt);
    }

    void OnSlotClicked(VisualElement slot, PointerDownEvent evt)
    {
        var info = slot.userData as SlotInfo;

        // 1. 아이템 이동 중일 때 (목표 지점 클릭)
        if (_isMovingItem)
        {
            MoveItem(_sourceSlotInfo, info);
            _isMovingItem = false;
            DeselectAllSlots();
            return;
        }

        // 2. 일반 클릭
        DeselectAllSlots();
        slot.AddToClassList("selected");

        // 아이템이 있으면 상세 정보 + 메뉴
        if (info != null && info.item != null)
        {
            ShowItemDetails(info.item);
            OpenContextMenu(evt.position, info);
        }
        else
        {
            // 빈 슬롯 클릭 시
            ShowPlayerStats();
            CloseContextMenu();
        }
    }

    private void OpenContextMenu(Vector2 screenPos, SlotInfo info)
    {
        if (_contextMenu == null) return;

        _selectedSlotInfo = info;
        _contextMenu.style.display = DisplayStyle.Flex;
        _contextMenu.style.left = screenPos.x; 
        _contextMenu.style.top = screenPos.y;
    }

    private void CloseContextMenu()
    {
        if (_contextMenu != null) _contextMenu.style.display = DisplayStyle.None;
        _selectedSlotInfo = null;
    }

    private void OnDiscardClicked()
    {
        if (_selectedSlotInfo != null && _selectedSlotInfo.item != null)
        {
            // 리스트에서 제거
            initialItems.Remove(_selectedSlotInfo.item);
            
            // 그리드 배열에서도 제거
            if (!_selectedSlotInfo.isEquipment && _selectedSlotInfo.index >= 0 && _selectedSlotInfo.index < _gridItems.Length)
            {
                _gridItems[_selectedSlotInfo.index] = null;
            }

            DistributeItems(); 
            ShowPlayerStats(); 
        }
        CloseContextMenu();
    }

    private void OnMoveClicked()
    {
        if (_selectedSlotInfo != null && _selectedSlotInfo.item != null)
        {
            _isMovingItem = true;
            _sourceSlotInfo = _selectedSlotInfo;
            //Debug.Log("이동할 곳을 선택하세요.");
        }
        CloseContextMenu();
    }

    private void MoveItem(SlotInfo source, SlotInfo target)
    {
        if (source == null || target == null) return;
        if (source == target) return; 

        // [케이스 1] 그리드 -> 그리드 이동 (핵심 기능)
        if (!source.isEquipment && !target.isEquipment)
        {
            int srcIdx = source.index;
            int tgtIdx = target.index;

            // 배열 범위 체크
            if (srcIdx >= 0 && srcIdx < _gridItems.Length && tgtIdx >= 0 && tgtIdx < _gridItems.Length)
            {
                // 배열 내에서 위치 교환 (Swap)
                ItemData temp = _gridItems[srcIdx];
                _gridItems[srcIdx] = _gridItems[tgtIdx];
                _gridItems[tgtIdx] = temp;

                // UI 갱신
                DistributeItems();
            }
        }
        // [케이스 2] 장비 -> 그리드 (장착 해제)
        else if (source.isEquipment && !target.isEquipment)
        {
            // 장비 아이템은 현재 initialItems 리스트에만 의존하므로 로직이 복잡함.
            // 여기서는 간단히 "타겟 그리드가 비어있으면 이동"만 구현
            int tgtIdx = target.index;
            if (tgtIdx >= 0 && tgtIdx < _gridItems.Length && _gridItems[tgtIdx] == null)
            {
                // 장비 슬롯에서 빼고 그리드 배열에 넣음 (실제로는 itemType 변경 등이 필요할 수 있음)
                // 현재 구조상 itemType이 장비 슬롯을 결정하므로, 단순히 그리드로 옮기면
                // 다음 DistributeItems 때 다시 장비창으로 가버릴 수 있음.
                // *완벽한 구현을 위해서는 ItemData에 'IsEquipped' 상태가 필요함*
                Debug.Log("장비 해제는 현재 지원되지 않습니다.");
            }
        }
        // [케이스 3] 그리드 -> 장비 (장착)
        else if (!source.isEquipment && target.isEquipment)
        {
             Debug.Log("장착은 자동으로 이루어집니다.");
        }
    }

    private void DeselectAllSlots()
    {
        foreach(var s in gridSlots) s.RemoveFromClassList("selected");
        foreach(var s in equipmentSlots) s.RemoveFromClassList("selected");
    }

    private void ShowPlayerStats()
    {
        if (_playerStatsContainer != null) _playerStatsContainer.style.display = DisplayStyle.Flex;
        if (_itemDetailsContainer != null) _itemDetailsContainer.style.display = DisplayStyle.None;
    }

    private void ShowItemDetails(ItemData item)
    {
        if (_playerStatsContainer != null) _playerStatsContainer.style.display = DisplayStyle.None;
        if (_itemDetailsContainer != null) _itemDetailsContainer.style.display = DisplayStyle.Flex;

        if (nameLabel != null) nameLabel.text = item.itemName;
        if (descriptionLabel != null) descriptionLabel.text = item.description;
        if (statsLabel != null) statsLabel.text = item.performance;

        if (iconImage != null)
        {
            if (item.icon != null)
            {
                iconImage.style.backgroundImage = new StyleBackground(item.icon);
                iconImage.style.backgroundColor = Color.clear;
            }
            else
            {
                iconImage.style.backgroundImage = null;
            }
        }
    }

    // InventoryUI.cs에 추가할 디버깅 함수들

    [Header("디버그 설정")]
    [SerializeField] private bool enableSynergyOrderDebug = true; // Inspector에서 토글 가능

    /// <summary>
    /// 현재 시너지 컨테이너의 순서를 로그로 출력
    /// </summary>
    private void DebugSynergyOrder()
    {
        if (!enableSynergyOrderDebug) return;
        
        Debug.Log("=== 시너지 컨테이너 현재 순서 ===");
        
        if (_synergyContainer != null)
        {
            int index = 0;
            foreach (VisualElement child in _synergyContainer.Children())
            {
                var nameLabel = child.Q<Label>(className: "synergy-name");
                var levelLabel = child.Q<Label>(className: "synergy-level");
                
                string synergyName = nameLabel?.text ?? "알 수 없음";
                string levelText = levelLabel?.text ?? "레벨 없음";
                bool isActive = child.ClassListContains("active");
                
                Debug.Log($"[{index}] {synergyName} - {levelText} {(isActive ? "(활성화)" : "(비활성화)")}");
                index++;
            }
        }
        
        Debug.Log("=== 시너지 딕셔너리 정보 ===");
        foreach (var kvp in _synergyUIElements)
        {
            var element = kvp.Value;
            var nameLabel = element.Q<Label>(className: "synergy-name");
            var levelLabel = element.Q<Label>(className: "synergy-level");
            
            string synergyName = nameLabel?.text ?? "알 수 없음";
            string levelText = levelLabel?.text ?? "레벨 없음";
            bool isActive = element.ClassListContains("active");
            
            Debug.Log($"[Dict] {kvp.Key} -> {synergyName} - {levelText} {(isActive ? "(활성화)" : "(비활성화)")}");
        }
    }

    /// <summary>
    /// 시너지 정렬 전후 상태를 비교해서 로그 출력
    /// </summary>
    private void DebugSynergySort(List<SynergyDisplayInfo> beforeSort, List<SynergyDisplayInfo> afterSort)
    {
        if (!enableSynergyOrderDebug) return;
        
        Debug.Log("=== 시너지 정렬 전후 비교 ===");
        
        Debug.Log("-- 정렬 전 --");
        for (int i = 0; i < beforeSort.Count; i++)
        {
            var info = beforeSort[i];
            Debug.Log($"[{i}] {info.synergyTag} - Lv.{info.level} ({info.currentCount}개) {(info.isActive ? "활성" : "비활성")}");
        }
        
        Debug.Log("-- 정렬 후 --");
        for (int i = 0; i < afterSort.Count; i++)
        {
            var info = afterSort[i];
            Debug.Log($"[{i}] {info.synergyTag} - Lv.{info.level} ({info.currentCount}개) {(info.isActive ? "활성" : "비활성")}");
        }
    }

    // [수정] 아이템 관련 함수들에 디버깅 호출 추가

    public void AddItem(ItemData item, int slotIndex = -1)
    {
        // ... 기존 코드 ...
        
        if (enableSynergyOrderDebug)
            Debug.Log($"[아이템 추가] {item.itemName} -> 시너지 UI 업데이트 시작");
        
        UpdateSynergyUI();
    }

    public void RemoveItem(int slotIndex)
    {
        // ... 기존 코드 ...
        
        if (enableSynergyOrderDebug)
            Debug.Log($"[아이템 제거] 슬롯 {slotIndex} -> 시너지 UI 업데이트 시작");
        
        UpdateSynergyUI();
    }

    // [새로 추가] 시너지 컨테이너를 정렬된 순서대로 재배치
    private void RearrangeSynergyContainer(List<SynergyDisplayInfo> sortedList)
    {
        if (_synergyContainer == null) return;
        
        if (enableSynergyOrderDebug)
            Debug.Log("=== UI 컨테이너 순서 재정렬 시작 ===");
        
        // 1. 기존 UI 요소들을 모두 컨테이너에서 제거 (딕셔너리는 유지)
        foreach (var kvp in _synergyUIElements)
        {
            if (kvp.Value.parent != null)
            {
                kvp.Value.RemoveFromHierarchy();
            }
        }
        
        // 2. 정렬된 순서대로 다시 컨테이너에 추가
        foreach (var info in sortedList)
        {
            if (_synergyUIElements.ContainsKey(info.synergyTag))
            {
                var synergyElement = _synergyUIElements[info.synergyTag];
                _synergyContainer.Add(synergyElement);
                
                if (enableSynergyOrderDebug)
                    Debug.Log($"[재배치] {info.synergyTag} -> {(info.isActive ? "활성" : "비활성")} Lv.{info.level}");
            }
        }
        
        if (enableSynergyOrderDebug)
            Debug.Log("=== UI 컨테이너 순서 재정렬 완료 ===");
    }

    // [추가] Inspector 변경사항 실시간 반영
#if UNITY_EDITOR
void OnValidate()
{
    // Editor 모드에서만 실행
    if (!Application.isPlaying) return;
    
    // UI가 초기화되지 않았으면 실행하지 않음
    if (_root == null || _gridItems == null) return;
    
    if (enableSynergyOrderDebug)
        Debug.Log("[OnValidate] Inspector에서 Initial Items 변경 감지됨");
    
    // 변경사항을 게임에 반영
    StartCoroutine(RefreshUINextFrame());
}

// [추가] 다음 프레임에 UI 갱신 (OnValidate가 여러 번 호출되는 것을 방지)
private System.Collections.IEnumerator RefreshUINextFrame()
{
    yield return null; // 한 프레임 대기
    
    if (enableSynergyOrderDebug)
        Debug.Log("[RefreshUINextFrame] UI 갱신 실행");
        
    RefreshInventoryFromInspector();
}
#endif

// [새로 추가] Inspector의 Initial Items를 게임 상태에 반영
private void RefreshInventoryFromInspector()
{
    if (_gridItems == null) return;
    
    if (enableSynergyOrderDebug)
    {
        Debug.Log($"[RefreshInventoryFromInspector] Initial Items 개수: {initialItems.Count}");
        foreach (var item in initialItems)
        {
            if (item != null)
                Debug.Log($"  - {item.itemName} (Type: {item.itemType})");
        }
    }
    
    // 기존 그리드 초기화
    for (int i = 0; i < _gridItems.Length; i++)
        _gridItems[i] = null;
    
    // Inspector의 Initial Items를 다시 그리드에 배치
    LoadInitialItemsToGrid();
    
    // UI 갱신
    DistributeItems();
    
    if (enableSynergyOrderDebug)
        Debug.Log("[RefreshInventoryFromInspector] UI 갱신 완료");
}

// [추가] 런타임 Inspector 변경 감지용 변수들
[Header("런타임 Inspector 감지")]
[SerializeField] private bool enableRuntimeInspectorWatch = true;
private int _lastInitialItemsCount = 0;
private List<ItemData> _lastInitialItemsSnapshot = new List<ItemData>();

void Update()
{
    // Inspector 변경 감지가 활성화되어 있고, 게임이 실행 중일 때만
    if (!enableRuntimeInspectorWatch || !Application.isPlaying) return;
    
    // Initial Items 배열에 변화가 있는지 확인
    if (HasInitialItemsChanged())
    {
        if (enableSynergyOrderDebug)
            Debug.Log("[Update] Runtime에서 Initial Items 변경 감지됨");
            
        RefreshInventoryFromInspector();
        UpdateInitialItemsSnapshot();
    }
}

// [추가] Initial Items 배열이 변경되었는지 확인
private bool HasInitialItemsChanged()
{
    // 1. 배열 크기가 다르면 변경됨
    if (initialItems.Count != _lastInitialItemsCount)
        return true;
    
    // 2. 각 요소를 비교
    for (int i = 0; i < initialItems.Count; i++)
    {
        if (i >= _lastInitialItemsSnapshot.Count)
            return true;
            
        if (initialItems[i] != _lastInitialItemsSnapshot[i])
            return true;
    }
    
    return false;
}

// [추가] 현재 Initial Items 상태를 스냅샷으로 저장
private void UpdateInitialItemsSnapshot()
{
    _lastInitialItemsCount = initialItems.Count;
    _lastInitialItemsSnapshot.Clear();
    _lastInitialItemsSnapshot.AddRange(initialItems);
}
}
