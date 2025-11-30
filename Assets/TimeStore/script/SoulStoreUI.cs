using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Collections;

public class SoulStoreUI : MonoBehaviour
{
    public static SoulStoreUI Instance;

    private UIDocument _uiDocument;
    private VisualElement _root;
    private VisualElement _board, _npcDialogueArea, _currencyBar;     

    private Label _soulLabel, _descTitle, _descLevel, _descBody, _lblCostValue, _lblNextStat, _npcText, _npcNameTag;
    private Button _confirmBtn, _closeBtn;

    [Header("상점 아이템 목록")]
    public List<ShopItemData> _shopItems = new List<ShopItemData>();
    private List<Button> _uiButtons = new List<Button>(); 
    private int _selectedIndex = -1;

    public bool IsOpen { get; private set; } = false;
    private bool _isShopMode = false; 
    private int _openFrameCount = -1;
    private NPCData _currentNPCData;

    private readonly Color _lineColorLocked = new Color(0.2f, 0.2f, 0.2f, 1f);
    private readonly Color _lineColorUnlocked = new Color(1f, 0.8f, 0.2f, 1f);

    [System.Serializable]
    public class ShopItemData
    {
        public string key;
        public string name;
        [TextArea] public string descTemplate;
        public int maxLevel;
        public int baseCost;
        
        public float valuePerLevel; 
        public string valueFormat = "+{0}%"; 

        public string GetValueText(int currentLevel)
        {
            float val = currentLevel * valuePerLevel;
            return string.Format(valueFormat, val); 
        }
    }

    private void OnValidate()
    {
        if (_uiDocument == null) _uiDocument = GetComponent<UIDocument>();
        if (_uiDocument != null && _root == null) _root = _uiDocument.rootVisualElement;
        if (_root != null && _selectedIndex != -1) SelectStat(_selectedIndex); 
    }

    private void Awake() { if (Instance == null) Instance = this; }

    private void Start()
    {
        _uiDocument = GetComponent<UIDocument>();
        if (_uiDocument == null) return;
        _root = _uiDocument.rootVisualElement;
        if (_root == null) return;

        if (_shopItems.Count == 0) InitializeShopData();

        _board = _root.Q<VisualElement>(className: "board"); 
        _npcDialogueArea = _root.Q<VisualElement>(className: "info-wrap"); 
        _currencyBar = _root.Q<VisualElement>(className: "currency-bar"); 
        if (_currencyBar != null) _soulLabel = _currencyBar.Q<Label>("SoulLabel");
        _descTitle = _root.Q<Label>(className: "tab-left");
        _descLevel = _root.Q<Label>("DescLevel");
        _descBody = _root.Q<Label>("DescBody");
        List<Label> costLabels = _root.Query<Label>("DescCost").ToList();
        if (costLabels.Count > 0) _lblCostValue = costLabels[0];
        if (costLabels.Count > 1) _lblNextStat = costLabels[1];
        if (_npcDialogueArea != null) _npcText = _npcDialogueArea.Q<Label>(className: "currency-value");

        List<Label> leftLabels = _root.Query<Label>(className: "currency-left").ToList();
        foreach(var lbl in leftLabels) { if (lbl.text == "NPC") { _npcNameTag = lbl; break; } }
        if (_npcNameTag != null && _npcDialogueArea != null) { _npcNameTag.RemoveFromHierarchy(); _npcDialogueArea.Add(_npcNameTag); _npcNameTag.style.position = Position.Absolute; _npcNameTag.style.top = -25; _npcNameTag.style.left = 0; _npcNameTag.style.width = 150; _npcNameTag.style.height = 40; }

        _confirmBtn = _root.Q<Button>("BtnConfirm");
        _closeBtn = _root.Q<Button>("BtnClose");
        if (_confirmBtn != null) _confirmBtn.RegisterCallback<ClickEvent>(OnConfirmClicked);
        if (_closeBtn != null) _closeBtn.RegisterCallback<ClickEvent>(OnCloseClicked);

        _uiButtons.Clear(); 
        List<VisualElement> rows = _root.Query<VisualElement>(className: "row").ToList();
        if (rows.Count > 0) SetupRowButtons(rows[0], 0); 
        if (rows.Count > 1) SetupRowButtons(rows[1], 4); 
        if (rows.Count > 2) SetupRowButtons(rows[2], 8); 

        HideUI();
    }

    private void Update()
    {
        if (!IsOpen) return;
        if (Time.frameCount == _openFrameCount) return;

        if (Input.GetKeyDown(KeyCode.Escape)) { if (_isShopMode) SwitchToDialogueMode(); else { if (_closeBtn != null) PlayBounceAnimation(_closeBtn); HideUI(); } }
        if (_isShopMode) HandleKeyboardNavigation();
        if (Input.GetKeyDown(KeyCode.F)) { if (!_isShopMode) { if (_confirmBtn != null) PlayBounceAnimation(_confirmBtn); SwitchToShopMode(); } else { if (_confirmBtn != null) PlayBounceAnimation(_confirmBtn); TryPurchase(); } }
    }

    // 키보드 이동
    private void HandleKeyboardNavigation()
    {
        int newIndex = _selectedIndex; bool inputDetected = false;
        if (Input.GetKeyDown(KeyCode.RightArrow)) { if ((_selectedIndex + 1) % 4 != 0 && _selectedIndex + 1 < _shopItems.Count) { newIndex++; inputDetected = true; } }
        else if (Input.GetKeyDown(KeyCode.LeftArrow)) { if (_selectedIndex % 4 != 0 && _selectedIndex - 1 >= 0) { newIndex--; inputDetected = true; } }
        else if (Input.GetKeyDown(KeyCode.DownArrow)) { if (_selectedIndex + 4 < _shopItems.Count) { newIndex += 4; inputDetected = true; } }
        else if (Input.GetKeyDown(KeyCode.UpArrow)) { if (_selectedIndex - 4 >= 0) { newIndex -= 4; inputDetected = true; } }
        if (inputDetected && newIndex != _selectedIndex) { SelectStat(newIndex); HighlightButton(newIndex); }
    }
    private void HighlightButton(int index) { foreach (var btn in _uiButtons) btn.style.scale = new Scale(Vector3.one); if (index >= 0 && index < _uiButtons.Count) { var targetBtn = _uiButtons[index]; if (IsItemUnlocked(index)) { targetBtn.style.transitionDuration = new List<TimeValue> { new TimeValue(0.1f) }; targetBtn.style.scale = new Scale(new Vector3(1.15f, 1.15f, 1f)); } } }
    public void StartInteraction(NPCData data) { if (_root == null) return; IsOpen = true; _currentNPCData = data; _openFrameCount = Time.frameCount; _root.style.display = DisplayStyle.Flex; Time.timeScale = 0f; UnityEngine.Cursor.visible = true; UnityEngine.Cursor.lockState = CursorLockMode.None; SwitchToDialogueMode(); }
    private void SwitchToDialogueMode() { _isShopMode = false; if (_board != null) _board.style.display = DisplayStyle.None; if (_currencyBar != null) _currencyBar.style.display = DisplayStyle.None; if (_npcText != null && _currentNPCData != null) _npcText.text = _currentNPCData.greetingDialogue; if (_npcNameTag != null && _currentNPCData != null) _npcNameTag.text = _currentNPCData.npcName; if (_confirmBtn != null) _confirmBtn.text = "강화하기 (F)"; if (_closeBtn != null) _closeBtn.text = "대화 종료 (ESC)"; }
    private void SwitchToShopMode() { _isShopMode = true; if (_board != null) _board.style.display = DisplayStyle.Flex; if (_currencyBar != null) _currencyBar.style.display = DisplayStyle.Flex; UpdateCurrencyUI(); if (_npcText != null) _npcText.text = "신중하게 선택하게나..."; if (_confirmBtn != null) _confirmBtn.text = "강화 (F)"; if (_closeBtn != null) _closeBtn.text = "뒤로 가기 (ESC)"; RefreshUIState(); int startIdx = Mathf.Max(0, _selectedIndex); if (_selectedIndex == -1) startIdx = 0; SelectStat(startIdx); HighlightButton(startIdx); }
    private void HideUI() { IsOpen = false; if (_root != null) _root.style.display = DisplayStyle.None; Time.timeScale = 1f; }
    private void OnConfirmClicked(ClickEvent evt) { if (!_isShopMode) { PlayBounceAnimation(_confirmBtn); SwitchToShopMode(); } else { TryPurchase(); } }
    private void OnCloseClicked(ClickEvent evt) { PlayBounceAnimation(_closeBtn); if (_isShopMode) SwitchToDialogueMode(); else HideUI(); }
    
    private void TryPurchase()
    {
        if (_selectedIndex == -1 || SoulDataManager.Instance == null) return;
        Button currentBtn = (_selectedIndex >= 0 && _selectedIndex < _uiButtons.Count) ? _uiButtons[_selectedIndex] : null;
        if (!IsItemUnlocked(_selectedIndex)) { if (_confirmBtn != null) PlayShakeAnimation(_confirmBtn); if (currentBtn != null) PlayShakeAnimation(currentBtn); return; }
        ShopItemData item = _shopItems[_selectedIndex];
        int curLv = SoulDataManager.Instance.GetStatLevel(item.key);
        int cost = item.baseCost + (curLv * 50);
        if (SoulDataManager.Instance.saveData.timeSand < cost) { if (_confirmBtn != null) PlayShakeAnimation(_confirmBtn); if (_lblCostValue != null) PlayShakeAnimation(_lblCostValue); if (currentBtn != null) PlayShakeAnimation(currentBtn); return; }
        if (SoulDataManager.Instance.TryUpgradeStat(item.key, cost, item.maxLevel)) { if (_confirmBtn != null) PlayBounceAnimation(_confirmBtn); if (currentBtn != null) PlayBounceAnimation(currentBtn); UpdateCurrencyUI(); RefreshUIState(); SelectStat(_selectedIndex); GameObject player = GameObject.FindGameObjectWithTag("Player"); if (player != null) { var status = player.GetComponent<PlayerStatus>(); if (status != null) status.CalculateStats(); } } else { if (_confirmBtn != null) PlayShakeAnimation(_confirmBtn); }
    }
    private void UpdateCurrencyUI() { if (_soulLabel != null && SoulDataManager.Instance != null) _soulLabel.text = SoulDataManager.Instance.saveData.timeSand.ToString(); }
    
    private void SetupRowButtons(VisualElement row, int startIndex)
    {
        List<Button> buttons = row.Query<Button>().ToList();
        for (int i = 0; i < buttons.Count; i++)
        {
            int dataIndex = startIndex + i;
            if (dataIndex >= _shopItems.Count) break;
            Button btn = buttons[i]; int captureIndex = dataIndex; _uiButtons.Add(btn);
            
            // 클릭 시에만 선택/강조
            btn.RegisterCallback<ClickEvent>(evt => { SelectStat(captureIndex); HighlightButton(captureIndex); PlayBounceAnimation(btn); });
            
            // 마우스 오버 시 살짝 커짐 (선택은 안 함)
            btn.RegisterCallback<MouseEnterEvent>(evt => { 
                if (IsItemUnlocked(captureIndex) && _selectedIndex != captureIndex) { 
                    btn.style.transitionDuration = new List<TimeValue> { new TimeValue(0.1f) }; 
                    btn.style.scale = new Scale(new Vector3(1.05f, 1.05f, 1f)); 
                } 
            });
            // 마우스 나갈 때 복구
            btn.RegisterCallback<MouseLeaveEvent>(evt => { 
                if (_selectedIndex != captureIndex) {
                    btn.style.transitionDuration = new List<TimeValue> { new TimeValue(0.1f) }; 
                    btn.style.scale = new Scale(Vector3.one); 
                }
            });
        }
    }
    private void PlayBounceAnimation(VisualElement target) { target.style.transitionDuration = new List<TimeValue> { new TimeValue(0.05f) }; target.style.scale = new Scale(new Vector3(0.9f, 0.9f, 1f)); target.schedule.Execute(() => { target.style.transitionDuration = new List<TimeValue> { new TimeValue(0.1f) }; target.style.scale = new Scale(Vector3.one); }).StartingIn(50); }
    private void PlayShakeAnimation(VisualElement target) { StartCoroutine(ShakeElement(target)); }
    private IEnumerator ShakeElement(VisualElement element) { float elapsed = 0f; while (elapsed < 0.3f) { element.style.translate = new Translate(Mathf.Sin(elapsed * 50f) * 10f, 0, 0); elapsed += Time.deltaTime; yield return null; } element.style.translate = new Translate(0, 0, 0); }
    private void RefreshUIState() { List<VisualElement> rows = _root.Query<VisualElement>(className: "row").ToList(); int itemIndex = 0; foreach (var row in rows) { List<Button> buttons = row.Query<Button>().ToList(); List<VisualElement> lines = row.Query<VisualElement>(className: "line").ToList(); for (int i = 0; i < buttons.Count; i++) { if (itemIndex >= _shopItems.Count) break; UpdateButtonState(buttons[i], itemIndex); if (i < lines.Count) { int curLv = SoulDataManager.Instance.GetStatLevel(_shopItems[itemIndex].key); bool isPathOpen = IsItemUnlocked(itemIndex) && curLv > 0; lines[i].style.backgroundColor = isPathOpen ? _lineColorUnlocked : _lineColorLocked; } itemIndex++; } } }
    private void UpdateButtonState(Button btn, int index) { var badgeLbl = btn.Q<Label>(className: "badge"); if (badgeLbl != null && SoulDataManager.Instance != null) { badgeLbl.text = SoulDataManager.Instance.GetStatLevel(_shopItems[index].key).ToString(); bool unlocked = IsItemUnlocked(index); btn.style.opacity = unlocked ? 1.0f : 0.5f; } }
    private bool IsItemUnlocked(int index) { if (index % 4 == 0) return true; ShopItemData prevItem = _shopItems[index - 1]; return SoulDataManager.Instance.GetStatLevel(prevItem.key) > 0; }

    private void SelectStat(int index)
    {
        _selectedIndex = index; if (index < 0 || index >= _shopItems.Count) return;
        ShopItemData item = _shopItems[index]; 
        int curLv = 0; if (SoulDataManager.Instance != null) curLv = SoulDataManager.Instance.GetStatLevel(item.key);

        // 🚀 [수정] PlayerStatus가 아니라 StatDataManager에게 물어봄
        string curText = "0"; string nextText = "MAX";
        if (StatDataManager.Instance != null) 
        { 
            curText = StatDataManager.Instance.GetStatPreview(item.key, curLv); 
            if (curLv < item.maxLevel) nextText = StatDataManager.Instance.GetStatPreview(item.key, curLv + 1); 
        } 
        else 
        { 
            curText = item.GetValueText(curLv); 
            if (curLv < item.maxLevel) nextText = item.GetValueText(curLv + 1); 
        }

        if (SoulDataManager.Instance != null && !IsItemUnlocked(index)) { if (_descTitle != null) _descTitle.text = "🔒 잠금됨"; if (_descLevel != null) _descLevel.text = "-"; if (_descBody != null) _descBody.text = "이전 단계 강화 필요"; if (_lblCostValue != null) _lblCostValue.text = ""; if (_lblNextStat != null) _lblNextStat.text = ""; return; }

        int cost = item.baseCost + (curLv * 50); bool isMax = curLv >= item.maxLevel;
        if (_descTitle != null) _descTitle.text = item.name; if (_descLevel != null) _descLevel.text = $"Lv. {curLv}/{item.maxLevel}";
        if (_descBody != null) _descBody.text = $"{item.descTemplate}\n현재: <color=#FFD700>{curText}</color>"; if (_lblNextStat != null) _lblNextStat.text = isMax ? "최고 레벨" : $"다음: <color=#00FF00>{nextText}</color>"; if (_lblCostValue != null) _lblCostValue.text = isMax ? "" : $"필요 시간의 모래: {cost}";
    }

    // (InitializeShopData는 기존에 수정해드린 버전을 유지하거나, 우클릭으로 초기화하시면 됩니다)
    [ContextMenu("초기 데이터 로드")]
    private void InitializeShopData() { _shopItems.Clear(); _shopItems.Add(new ShopItemData { key = "physicalattack", name = "공격력 강화", maxLevel = 10, baseCost = 100, descTemplate = "물리/마법 공격력 증가", valuePerLevel = 5, valueFormat = "+{0}%" }); _shopItems.Add(new ShopItemData { key = "criticalchance", name = "정밀 타격", maxLevel = 10, baseCost = 150, descTemplate = "치명타 확률/피해 증가", valuePerLevel = 1, valueFormat = "+{0}%" }); _shopItems.Add(new ShopItemData { key = "itemcooldownspeed", name = "빠른 손놀림", maxLevel = 2, baseCost = 300, descTemplate = "아이템 쿨다운 감소", valuePerLevel = 5, valueFormat = "-{0}%" }); _shopItems.Add(new ShopItemData { key = "skillcooldownspeed", name = "마력 순환", maxLevel = 2, baseCost = 300, descTemplate = "스킬 쿨다운 감소", valuePerLevel = 10, valueFormat = "-{0}%" }); _shopItems.Add(new ShopItemData { key = "health", name = "강인한 육체", maxLevel = 10, baseCost = 100, descTemplate = "최대 체력 증가", valuePerLevel = 5, valueFormat = "+{0} HP" }); _shopItems.Add(new ShopItemData { key = "defensivepower", name = "단단한 피부", maxLevel = 10, baseCost = 100, descTemplate = "방어력 증가", valuePerLevel = 5, valueFormat = "+{0} Def" }); _shopItems.Add(new ShopItemData { key = "def_dmg_reduce", name = "충격 완화", maxLevel = 2, baseCost = 250, descTemplate = "받는 피해 감소", valuePerLevel = 5, valueFormat = "-{0}%" }); _shopItems.Add(new ShopItemData { key = "def_revive", name = "불사의 의지", maxLevel = 2, baseCost = 500, descTemplate = "부활 능력 부여", valuePerLevel = 1, valueFormat = "Lv.{0}" }); _shopItems.Add(new ShopItemData { key = "sp_cost_reduce", name = "효율 개선", maxLevel = 10, baseCost = 200, descTemplate = "강화 비용 감소", valuePerLevel = 1, valueFormat = "-{0}%" }); _shopItems.Add(new ShopItemData { key = "sp_cd_reduce", name = "과부하 제어", maxLevel = 10, baseCost = 200, descTemplate = "장치 쿨타임 감소", valuePerLevel = 5, valueFormat = "-{0}%" }); _shopItems.Add(new ShopItemData { key = "sp_dash_stack", name = "제트 추진", maxLevel = 1, baseCost = 1000, descTemplate = "대쉬 횟수 추가", valuePerLevel = 1, valueFormat = "+{0}회" }); _shopItems.Add(new ShopItemData { key = "sp_overheat", name = "한계 돌파", maxLevel = 1, baseCost = 1000, descTemplate = "오버히트 시 치명타 적용", valuePerLevel = 1, valueFormat = "Lv.{0}" }); }
}