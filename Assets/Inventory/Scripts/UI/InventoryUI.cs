using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

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

        if (_btnMove != null) _btnMove.clicked += OnMoveClicked;
        if (_btnDiscard != null) _btnDiscard.clicked += OnDiscardClicked;

        // [중요] 그리드 배열 초기화
        int totalGridSize = rows * columns;
        _gridItems = new ItemData[totalGridSize];

        InitializeSlots();
        LoadInitialItemsToGrid(); // 초기 아이템을 그리드 배열에 배치
        DistributeItems();        // UI 그리기
        ShowPlayerStats();
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
            Debug.Log("이동할 곳을 선택하세요.");
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
}
