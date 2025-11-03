using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;

/// <summary>
/// 인벤토리 UI를 실제로 게임오브젝트와 기능하게하는 클래스
/// 기능 목록
/// 1. UXML/USS로 만든 UI를 화면에 띄움
/// 2. 인벤토리 슬롯 25개(slot-x-y)를 코드에서 찾음
/// 3. 장비 슬롯 3개(equipslot-1, 2, 3)도 코드에서 찾음
/// 4. 각 슬롯 클릭 시 어떤 일이 일어나는지 등록 -> 추후에 툴팁연동
/// 5. 테스트 아이템 아이콘 하나를 첫 번째 슬롯에 표시
/// 6. drag&drop 기능
/// </summary>
public class InventoryUIController : MonoBehaviour
{
    public VisualTreeAsset inventoryUXML;       // UXML 파일을 에디터에서 끌어다 넣을 수 있게 해주는 변수
    public StyleSheet inventoryUSS;             // USS 파일을 '' 해주는 변수
    public InventoryDrag_Drop drag_Drop;        // Drag_Drop 클래스 인스턴스 
    public static InventoryUIController Instance;

    private int draggingSlotIndex = -1;                         // 드래그 시작한 슬롯번호를 기억하는 변수 원래자리에서 아이템을 지울 때 사용
    private VisualElement root;                                 // UXML에서 만든 UI 전체를 나타내는 최상단 요소
    private List<InventorySlotData> inventorySlots = new();         // 인벤토리 슬롯 25개를 저장하는 리스트.
    private List<VisualElement> slotElements = new();           // 마우스 위치 기반 탐색용
    private List<EquipSlotData> equipSlots = new();             // 장비 슬롯 3개를 저장하는 리스트.

    private TooltipController tooltipController = new();

    //[Header("툴팁 UI")]
    public VisualTreeAsset tooltipUXML;

    [SerializeField] private ChooseOne chooseone;

    private void Awake()
    {
        // 씬에 로드되기는 하지만, 컴포넌트 로직(OnEnable 등)이 실행되지 않도록 비활성화
        this.enabled = false;

        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        // 첫 생성 인스턴스는 자신을 등록하고 유지
        Instance = this;
        DontDestroyOnLoad(this.gameObject);

    }

    /// <summary>
    /// 게임을 시작할 때, 오브젝트가 활성화 시 실행되는 함수
    /// UI를 불러오고 슬롯들을 찾아서 정리, 테스트 아이템을 배치, 드래그 처리를 준비함
    /// </summary>
    void OnEnable()
    {
        var uiDocument = GetComponent<UIDocument>();    // UIDocument 컴포넌트를 가져옴
        root = uiDocument.rootVisualElement;
        root.styleSheets.Add(inventoryUSS);             // 스타일 시트 추가
        Instance = this;

        // 슬롯들을 초기화하고 찾는 함수
        InitInventorySlots();
        drag_Drop = new InventoryDrag_Drop(root);       // 드래그 시스템 준비
        InitEquipSlots();

        LoadInventory();

        tooltipController.Initialize(root, tooltipUXML);


        // UI창 어디에서 마우스를 떼든 아이템 드래그 종료 처리를 하게 만들기 위한 부분


        root.RegisterCallback<PointerUpEvent>(evt =>
        {
            if (!drag_Drop.isDragging) return;

            VisualElement dropTarget = GetSlotUnderMouse(evt.position);
            int fromIndex = drag_Drop.fromSlotIndex;

            if (dropTarget != null)
            {
                int toIndex = slotElements.IndexOf(dropTarget);
                if (toIndex >= 0 && toIndex != fromIndex)
                {
                    var temp = inventorySlots[toIndex].weaponData;
                    inventorySlots[toIndex].SetItem(drag_Drop.currentItem);
                    inventorySlots[fromIndex].SetItem(temp);
                }
                else
                {
                    var equipSlot = equipSlots.Find(slot => slot.slotElement == dropTarget);
                    if (equipSlot != null)
                    {
                        WeaponStatPackage stats = EquipItem(equipSlot, drag_Drop.currentItem);
                        if (stats != null)
                        {
                            // 공격 스탯 적용
                            var player = GameObject.FindWithTag("Player");
                            if (player != null && player.TryGetComponent<Attack_Damage>(out var attack))
                            {
                                attack.SetWeaponStats(stats);
                            }
                            inventorySlots[fromIndex].Clear();
                        }
                        else
                            inventorySlots[fromIndex].SetItem(drag_Drop.currentItem);
                    }
                    else
                    {
                        inventorySlots[fromIndex].SetItem(drag_Drop.currentItem);
                    }
                }
            }
            else
            {
                inventorySlots[fromIndex].SetItem(drag_Drop.currentItem); // 원위치 복구
            }

            drag_Drop.EndDrag();
            draggingSlotIndex = -1;
        });

    }

    void InitInventorySlots()
    {
        var itemSlotRoot = root.Q<VisualElement>("itemslot");     // UXML에서 name="itemslot"인 부모 요소를 가져옴

        // 5x5 루프를 돌면서 slot-0-0 ~ slot-4-4 까지의 이름을 만듦
        for (int y = 0; y < 5; y++) // row
        {
            for (int x = 0; x < 5; x++) // col
            {
                string slotName = $"slot-{x}-{y}";
                var slot = itemSlotRoot.Q<VisualElement>(slotName);         // 만들어진 이름으로 실제 VisualElement를 찾음
                if (slot != null)
                {
                    var slotData = new InventorySlotData(slot);                               // 찾은 슬롯을 리스트에 저장
                    int index = inventorySlots.Count;
                    inventorySlots.Add(slotData);
                    slotElements.Add(slot);

                    int capturedIndex = index;


                    // 드래그 이벤트 시작
                    slot.RegisterCallback<PointerDownEvent>(evt =>
                    {
                        if (slotData.weaponData != null)
                        {
                            draggingSlotIndex = capturedIndex;
                            slotData.slotElement.Clear();
                            drag_Drop.BeginDrag(slotData.weaponData, capturedIndex);
                        }
                    });

                    // 드래그 이벤트 종료
                    root.schedule.Execute(() =>
                    {
                        slot.RegisterCallback<PointerUpEvent>(evt =>
                        {
                            if (drag_Drop.isDragging)
                            {
                                // 교체 대상 슬롯
                                int toIndex = capturedIndex;
                                // 교체 전 슬롯s
                                int fromIndex = drag_Drop.fromSlotIndex;

                                if (fromIndex != toIndex)
                                {
                                    // 아이템 교체
                                    var temp = inventorySlots[toIndex].weaponData;
                                    inventorySlots[toIndex].SetItem(drag_Drop.currentItem);
                                    inventorySlots[fromIndex].SetItem(temp);
                                }
                                else
                                {
                                    inventorySlots[fromIndex].SetItem(drag_Drop.currentItem);
                                }

                                drag_Drop.EndDrag();
                                draggingSlotIndex = -1;
                            }
                        });
                    });

                    slot.RegisterCallback<MouseEnterEvent>(evt =>
                    {
                        if (!drag_Drop.isDragging)
                            tooltipController.Show(slotData.weaponData, evt.mousePosition);
                    });
                    slot.RegisterCallback<MouseLeaveEvent>(evt =>
                    {
                        tooltipController.Hide();
                    });
                }
            }
        }
    }


    void InitEquipSlots()
    {
        var equipRoot = root.Q<VisualElement>("equipslot");

        // 장비 슬롯 3개를 순회함.
        for (int i = 1; i <= 3; i++)
        {
            var slotElement = equipRoot.Q<VisualElement>($"equipslot-{i}");             // UXML에서 이름으로 해당 슬롯을 찾음.
            if (slotElement != null)
            {
                var slotData = new EquipSlotData(slotElement);
                equipSlots.Add(slotData);

                slotElement.RegisterCallback<MouseEnterEvent>(evt =>
                {
                    if (!drag_Drop.isDragging)
                        tooltipController.Show(slotData.weaponData, evt.mousePosition);
                });
                slotElement.RegisterCallback<MouseLeaveEvent>(evt =>
                {
                    tooltipController.Hide();
                });
            }
        }
    }



    private VisualElement GetSlotUnderMouse(Vector2 mousePos)
    {
        foreach (var slot in slotElements)
        {
            if (slot.worldBound.Contains(mousePos))
                return slot;
        }
        foreach (var equipSlot in equipSlots)
        {
            if (equipSlot.slotElement.worldBound.Contains(mousePos))
                return equipSlot.slotElement;
        }
        return null;
    }

    private WeaponStatPackage EquipItem(EquipSlotData equipSlot, WeaponPrefabData data)
    {
        if (!Ark_stat.MeetsRequirement(data))
        {
            Debug.Log($"[장착 실패] 요구 스탯 부족 : 체력 {data.requiredHp}, 힘 {data.requiredStr}, 민첩{data.requiredDex}, 지력{data.requiredInt} ");
            return null;
        }

        var oldWeapon = equipSlot.weaponData;
        if (oldWeapon != null)
        {
            bool returned = TryAddItem(oldWeapon);
            if (!returned)
            {
                Debug.LogWarning("기존 무기를 인벤토리에 되돌릴 수 없습니다.");
            }
        }

        equipSlot.SetItem(data);
        Debug.Log($"장비 장착됨 : {data.grade} {data.type} ");
        return new WeaponStatPackage(
            data.attackPower,
            data.attackSpeed,
            data.critChance,
            data.critDamage,
            data.range
            );
    }
    /// <summary>
    /// 아이템을 인벤토리 빈 칸에 추가하려고 시도한다.
    /// 성공하면 true, 실패(꽉 참)하면 false
    /// </summary>
    public bool TryAddItem(WeaponPrefabData data)
    {
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            if (inventorySlots[i].weaponData == null)
            {
                inventorySlots[i].SetItem(data);
                Debug.Log($"[아이템 습득] {data.grade} {data.type} 인벤토리 {i}번 칸에 추가됨");
                return true;
            }
        }

        Debug.Log("인벤토리가 가득 찼습니다!");
        return false;
    }


    /// <summary>
    /// 인벤토리 저장 함수
    /// </summary>
    public void SaveInventory()
    {
        List<WeaponSaveData> saveList = new();
        foreach (var slot in inventorySlots)
        {
            if (slot.weaponData != null)
            {
                saveList.Add(ConvertToSaveData(slot.weaponData));
            }
        }

        List<WeaponSaveData> equipList = new();
        foreach (var slot in equipSlots)
        {
            if (slot.weaponData != null)
            {
                equipList.Add(ConvertToSaveData(slot.weaponData));
            }
        }


        string json = JsonUtility.ToJson(new SaveWrapper { list = saveList, equippedList = equipList }, true);
        File.WriteAllText(Path.Combine(Application.persistentDataPath, "inventory.json"), json);
        Debug.Log("인벤토리 저장 완료");
    }

    private WeaponSaveData ConvertToSaveData(WeaponPrefabData weapon)
    {
        return new WeaponSaveData
        {
            weaponName = weapon.weaponName,
            grade = weapon.grade,
            type = weapon.type,
            level = weapon.level,
            description = weapon.description,

            sellprice = weapon.sellprice,

            attackPower = weapon.attackPower,
            attackSpeed = weapon.attackSpeed,
            critChance = weapon.critChance,
            critDamage = weapon.critDamage,
            range = weapon.range,

            iconPath = $"Weapon/{weapon.grade}/{weapon.type}/{weapon.grade}_{weapon.type}",

            requiredHp = weapon.requiredHp,
            requiredStr = weapon.requiredStr,
            requiredDex = weapon.requiredDex,
            requiredInt = weapon.requiredInt,
            requiredluk = weapon.requiredluk
        };
    }

    public void LoadInventory()
    {
        string path = Path.Combine(Application.persistentDataPath, "inventory.json");
        if (!File.Exists(path))
        {
            Debug.LogWarning("저장된 인벤토리 파일 없음");
            return;
        }

        string json = File.ReadAllText(path);
        var wrapper = JsonUtility.FromJson<SaveWrapper>(json);

        if (wrapper == null || wrapper.list == null)
        {
            Debug.LogWarning("[Load Fail] Json 파싱 실패 또는 리스트 없음");
            return;
        }

        //Debug.Log($"[LOAD SUCCESS] 무기 개수: {wrapper.list.Count}");

        for (int i = 0; i < inventorySlots.Count; i++)
        {
            inventorySlots[i].Clear(false);
            if (i < wrapper.list.Count)
            {
                var data = wrapper.list[i];
                inventorySlots[i].SetItem(ConvertToWeaponData(data));
                //Debug.Log($"[LoadInventory] 슬롯 {i}번에 무기 넣기 시도 중");
            }
        }

        for (int i = 0; i < equipSlots.Count; i++)
        {
            var slot = equipSlots[i];
            slot.Clear();

            if (wrapper.equippedList != null && i < wrapper.equippedList.Count && wrapper.equippedList[i] != null)
            {
                var weapon = ConvertToWeaponData(wrapper.equippedList[i]);
                slot.SetItem(weapon);

                if (i == 0)
                {
                    var player = GameObject.FindWithTag("Player");
                    if (player != null && player.TryGetComponent<Attack_Damage>(out var attack))
                    {
                        attack.SetWeaponStats(new WeaponStatPackage(
                            weapon.attackPower,
                            weapon.attackSpeed,
                            weapon.critChance,
                            weapon.critDamage,
                            weapon.range
                            ));

                    }
                }
            }
        }


        //Debug.Log("인벤토리 로드 완료");
    }

    private WeaponPrefabData ConvertToWeaponData(WeaponSaveData data)
    {
        return new WeaponPrefabData
        {
            weaponName = data.weaponName,
            grade = data.grade,
            type = data.type,
            level = data.level,
            description = data.description,

            sellprice = data.sellprice,

            attackPower = data.attackPower,
            attackSpeed = data.attackSpeed,
            critChance = data.critChance,
            critDamage = data.critDamage,
            range = data.range,

            icon = LoadIconByName(data.iconPath),
            requiredHp = data.requiredHp,
            requiredStr = data.requiredStr,
            requiredDex = data.requiredDex,
            requiredInt = data.requiredInt,
            requiredluk = data.requiredluk
        };
    }

    private Sprite LoadIconByName(string iconPath)
    {
        //Debug.Log("[LOAD START]");

        if (string.IsNullOrEmpty(iconPath))
        {
            //Debug.LogWarning("아이콘이 비어있음");
            return null;
        }

        var sprite = Resources.Load<Sprite>(iconPath); // 확장자 없이 경로만
        //Debug.Log($"[LoadIconByName] 경로 : {iconPath}, 결과 : {(sprite != null ? sprite.name : "null")}");

        return sprite;


    }
    public void ResetInventory()
    {
        root.schedule.Execute(() =>
        {
            foreach (var slot in new List<InventorySlotData>(inventorySlots))

                slot.Clear(true); // 데이터 + UI 모두 제거        

            foreach (var equipSlot in new List<EquipSlotData>(equipSlots))

                equipSlot.Clear(true); // EquipSlotData 구조 기준
        });

        // 저장된 파일 제거 (선택)
        string path = Path.Combine(Application.persistentDataPath, "inventory.json");
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log("[ResetInventory] 저장된 인벤토리 파일 삭제됨");
        }

        Debug.Log("[ResetInventory] 인벤토리 + 장비 슬롯 초기화 완료");
    }

    void OnApplicationQuit()
    {
        SaveInventory(); //  종료 직전에 자동 저장
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            ResetInventory();
        }
    }
    /// <summary>
    /// 플레이어가 방향을 선택했을 때나, 인벤토리를 열고 싶을 때 외부에서 호출
    /// </summary>
    public void OpenInventory()
    {
        if (!this.enabled)
        {
            this.enabled = true;     // OnEnable()이 호출되면서 UI가 띄워지고 드래그&드롭 같은 로직이 초기화됩니다
        }
    }
    /// <summary>
    /// 인벤토리 닫을 때
    /// </summary>
    public void CloseInventory()
    {
        if (this.enabled)
        {
            this.enabled = false;    // OnDisable()이 호출되면서 UI가 사라지고, 드래그 이벤트도 해제됩니다
        }
    }

    [System.Serializable]
    private class SaveWrapper
    {
        public List<WeaponSaveData> list;           // 인벤토리 슬롯
        public List<WeaponSaveData> equippedList;   // 장비 슬롯
    }


}
