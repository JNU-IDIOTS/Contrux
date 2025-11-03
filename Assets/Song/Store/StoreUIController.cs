using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class StoreUIController : MonoBehaviour
{
    public static StoreUIController Instance { get; private set; }

    [SerializeField] private List<GameObject> weaponPrefabs;
    [SerializeField] private List<GradeProbability> gradeChances;
    [SerializeField] private int rerollCost = 50;

    private Dictionary<WeaponGrade, List<WeaponPrefabData>> prefabByGrade = new();
    private List<WeaponPrefabData> storeItems = new();
    private VisualElement root;

    [SerializeField] private VisualTreeAsset tooltipUXML;
    private TooltipController tooltipController = new();

    private bool isOpen = false;

    private static readonly Dictionary<WeaponGrade, Color> gradeColors = new()
    {
        { WeaponGrade.Common,     new Color32(200, 200, 200, 255) },
        { WeaponGrade.Rare,       new Color32(70, 130, 255, 255) },
        { WeaponGrade.Epic,       new Color32(186, 85, 211, 255) },
        { WeaponGrade.Unique,     new Color32(255, 140, 0, 255) },
        { WeaponGrade.Legendary,  new Color32(255, 215, 0, 255) }
    };

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // 씬 전환 후에도 유지
    }

    private void OnEnable()
    {
        InitUI();
    }

    private void InitUI()
    {
        var uiDoc = GetComponent<UIDocument>();
        root = uiDoc.rootVisualElement;
        root.style.display = DisplayStyle.None;
        isOpen = false;

        tooltipController.Initialize(root, tooltipUXML);

        prefabByGrade.Clear();
        foreach (var prefab in weaponPrefabs)
        {
            var holder = prefab.GetComponent<GroundItem>();
            if (holder?.data != null)
            {
                var data = holder.data;
                if (!prefabByGrade.ContainsKey(data.grade))
                    prefabByGrade[data.grade] = new List<WeaponPrefabData>();
                prefabByGrade[data.grade].Add(data);
            }
        }
    }

    private void Update()
    {
        if (isOpen && (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Z)))
        {
            CloseStoreUI();
        }
    }

    private WeaponGrade RollGradeByProbability()
    {
        float totalWeight = 0f;
        foreach (var entry in gradeChances)
            totalWeight += entry.weight;

        float roll = Random.Range(0, totalWeight);
        float cumulative = 0f;

        foreach (var entry in gradeChances)
        {
            cumulative += entry.weight;
            if (roll < cumulative)
                return entry.grade;
        }

        return WeaponGrade.Common;
    }

    private void GenerateStoreItems()
    {
        storeItems.Clear();
        for (int i = 0; i < 4; i++)
        {
            WeaponGrade chosenGrade = RollGradeByProbability();
            if (prefabByGrade.TryGetValue(chosenGrade, out var list) && list.Count > 0)
            {
                var item = list[Random.Range(0, list.Count)];
                storeItems.Add(item);
            }
        }
    }

    private void BindToUI(List<WeaponPrefabData> items)
    {
        for (int i = 0; i < items.Count && i < 4; i++)
        {
            var item = items[i];
            var slot = root.Q<VisualElement>((i + 1).ToString());
            var nameLabel = slot.Q<Label>("WeaponName");

            string displayName = string.IsNullOrEmpty(item.weaponName)
                ? $"{item.grade} {item.type}"
                : item.weaponName;

            nameLabel.text = displayName;
            nameLabel.style.color = gradeColors.TryGetValue(item.grade, out var color) ? color : Color.white;
            slot.Q<Label>("ReqGold").text = $"{item.price} G";

            if (item.icon == null)
            {
                string path = $"Weapon/{item.grade}/{item.type}/{item.grade}_{item.type}";
                item.icon = Resources.Load<Sprite>(path);
            }

            var image = slot.Q<VisualElement>("WeaponImage");
            if (item.icon != null)
                image.style.backgroundImage = new StyleBackground(item.icon);

            image.RegisterCallback<MouseEnterEvent>(evt => tooltipController.Show(item, evt.mousePosition));
            image.RegisterCallback<MouseLeaveEvent>(evt => tooltipController.Hide());

            slot.Q<Button>("Buy").clicked += () => TryBuy(item);
            int capturedIndex = i;
            slot.Q<Button>("Re").clicked += () => ReplaceItemAt(capturedIndex);
        }
    }

    private void TryBuy(WeaponPrefabData item)
    {
        if (PlayerGoldManager.Instance.Gold < item.price)
        {
            Debug.Log("골드 부족!");
            return;
        }

        if (!InventoryUIController.Instance.TryAddItem(item))
        {
            Debug.Log("인벤토리 가득 참");
            return;
        }

        PlayerGoldManager.Instance.SpendGold(item.price);
        Debug.Log($"구매 성공: {item.grade} {item.type}");
    }

    private void ReplaceItemAt(int index)
    {
        if (PlayerGoldManager.Instance.Gold < rerollCost)
        {
            Debug.Log("골드 부족! 무기 교체 불가");
            return;
        }

        WeaponGrade newGrade = RollGradeByProbability();

        if (prefabByGrade.TryGetValue(newGrade, out var list) && list.Count > 0)
        {
            var current = storeItems[index];
            var candidates = list.FindAll(x => x != current);

            if (candidates.Count == 0)
            {
                Debug.Log("중복 제외 시 선택 가능한 무기가 없음 → 기존 무기 유지");
                return;
            }

            var newItem = candidates[Random.Range(0, candidates.Count)];
            storeItems[index] = newItem;
            RefreshSlot(index, newItem);

            PlayerGoldManager.Instance.SpendGold(rerollCost);
            Debug.Log($"슬롯 {index + 1}번 무기 재추첨 완료 => {newItem.grade} {newItem.type}");
        }
    }

    private void RefreshSlot(int index, WeaponPrefabData item)
    {
        var slot = root.Q<VisualElement>((index + 1).ToString());
        var nameLabel = slot.Q<Label>("WeaponName");

        nameLabel.text = string.IsNullOrEmpty(item.weaponName)
            ? $"{item.grade} {item.type}"
            : item.weaponName;

        nameLabel.style.color = gradeColors.TryGetValue(item.grade, out var color) ? color : Color.white;
        slot.Q<Label>("ReqGold").text = $"{item.price} G";

        if (item.icon == null)
        {
            string path = $"Weapon/{item.grade}/{item.type}/{item.grade}_{item.type}";
            item.icon = Resources.Load<Sprite>(path);
        }

        var image = slot.Q<VisualElement>("WeaponImage");
        if (item.icon != null)
            image.style.backgroundImage = new StyleBackground(item.icon);
    }

    public void OpenStoreUI()
    {
        if (root == null)
            InitUI();

        if (storeItems.Count == 0)
            GenerateStoreItems();

        BindToUI(storeItems);
        root.style.display = DisplayStyle.Flex;
        isOpen = true;
    }

    public void CloseStoreUI()
    {
        if (root != null)
            root.style.display = DisplayStyle.None;
        isOpen = false;
    }
}
