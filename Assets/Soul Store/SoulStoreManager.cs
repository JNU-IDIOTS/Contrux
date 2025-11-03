using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// 소울 상점 전체 관리 + ESC 스택 대응(BackPanelBase 상속)
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class SoulStoreManager : BackPanelBase
{
    // ESC 닫힘 우선순위(스택 기반이라 의미적 값) - 상점/인벤토리 계열은 높게
    public override int BackPriority => 100;

    [Header("외부 참조")]
    [SerializeField] private UIDocument soulToolTipDocument; // 별도 툴팁 문서(선택)

    // UI 루트(BackPanelBase가 잡아준 viewRoot를 씀)
    private VisualElement root;
    private Label soulLabel;

    // 툴팁
    private VisualElement soulToolTip;
    private Label soulToolTipLabel;

    // 설명 텍스트 (에러 해결: 클래스 내부에 반드시 존재)
    private readonly string[] attackDescriptions = new[]
    {
        "전사의 본능\n\n 힘 스탯 + 1",
        "전장의 박동\n\n 이동 속도 +10%",
        "죽음을 꿰뚫는 눈\n\n 치명타 확률 +5%",
        "육신이 클수록 강하다\n\n 최대 체력 비례 추가 피해",
        "한 방의 격멸\n\n 치명타 데미지 +10%",
        "복수의 일격\n\n 피격 후 3초 이내 공격 데미지 +30%\n(쿨타임 10초)",
        "저주받은 연격\n\n 공격마다 추가 공격 속도 획득\n(최대 5회 중첩)"
    };

    private readonly string[] defenceDescriptions = new[]
    {
        "철의 결의\n\n 받는 피해 5% 감소",
        "강철의 육신\n\n 최대 체력 +100",
        "숨겨진 회복 본능\n\n 초당 체력 재생 +2%",
        "운명 개입\n\n 30초마다 자동 발동 피해 무효화",
        "고통의 마법진\n\n 추가 피해 감소 5%",
        "망령의 보호막\n\n 스킬 사용 시 방어막 +10\n(쿨타임 30초)",
        "죽음의 문턱에서\n\n 체력 1일 때 5초간 무적 발동\n(각 플레이마다 1회 사용)"
    };

    private readonly string[] specialDescriptions = new[]
    {
        "혼돈의 선물\n\n 무작위 효과 획득\n(템, 스탯, 버프 중 하나)",
        "탐욕의 본능\n\n 소울 획득량 +20%",
        "전리품 중독자\n\n 아이템 드랍률 상승",
        "거래의 재협상\n\n 상점 옵션 1회 리롤 가능",
        "성장의 축복\n\n 레벨당 능력치 상승량 +1",
        "바람처럼\n\n 대시 횟수 +1회",
        "숙명의 무기\n\n 내 스탯에 맞는 무기 드랍 확률 상승"
    };

    // 저장 경로
    private string SaveFilePath => Path.Combine(Application.persistentDataPath, "Player_Data.json");

    // ===== 생명주기 =====
    protected override void Awake()
    {
        base.Awake();
        root = viewRoot; // BackPanelBase가 name="Root" or 루트를 잡아줌

    }

    private void Start()
    {
        Hide();
        UINavigator.Instance.Register(this);
        // 툴팁 문서 준비
        if (soulToolTipDocument != null)
        {
            var tipRoot = soulToolTipDocument.rootVisualElement;
            soulToolTip = tipRoot.Q<VisualElement>("SoulToolTip");
            soulToolTipLabel = tipRoot.Q<Label>("SoulToolTipLabel");

            if (soulToolTip != null)
            {
                soulToolTip.style.display = DisplayStyle.None; // visible 대신 display
                soulToolTip.style.position = Position.Absolute;
                soulToolTip.pickingMode = PickingMode.Ignore;
            }
            if (soulToolTipLabel != null)
            {
                soulToolTipLabel.style.whiteSpace = WhiteSpace.Normal;
                soulToolTipLabel.style.maxWidth = 380;
            }
        }

        soulLabel = root.Q<Label>("soulLabel");
        if (soulLabel != null && PlayerExpManager.PlayerData != null)
            soulLabel.text = $"Soul: {PlayerExpManager.PlayerData.soulExp}";

        SetupUnlockButtons("attack", 7, 50);
        SetupUnlockButtons("deffence", 7, 50); // 버튼 name은 'deffence'로 되어있음(원본 유지)
        SetupUnlockButtons("special", 7, 100);

        UpdateAllUnlockStatus();
        LoadSoulStoreData();

        // 루트에서 마우스가 나가면 툴팁 강제 종료
        root.RegisterCallback<MouseLeaveEvent>((_) =>
        {
            if (soulToolTip != null)
                soulToolTip.style.display = DisplayStyle.None;
        });
    }

    // ===== ESC 스택 연동(열기/닫기) =====
    public override void Show()
    {
        base.Show();           // display:flex + 스택 Open
        PauseManager.Pause();  // 상점 열리면 게임 멈춤
        UpdateAllUnlockStatus();
    }

    public override void Hide()
    {
        SaveSoulStoreData();
        base.Hide();           // display:none + 스택 Close
        if (PauseManager.IsPaused)
            PauseManager.Resume();
    }

    // ===== 버튼/툴팁/언락 =====
    void SetButtonImage(Button btn, string imageName)
    {
        var tex = Resources.Load<Texture2D>(imageName);
        if (tex != null)
        {
            btn.style.backgroundImage = new StyleBackground(tex);
            btn.style.backgroundColor = Color.clear;
        }
    }

    void SetupUnlockButtons(string prefix, int count, int costPerUpgrade)
    {
        for (int i = 0; i < count; i++)
        {
            var btn = root.Q<Button>($"{prefix}{i}");
            if (btn == null)
            {
                Debug.LogWarning($"{prefix}{i} 버튼을 못 찾았어!");
                continue;
            }

            int upgradeCost = costPerUpgrade + (i * 100);
            btn.text = $"○ ({upgradeCost})";
            btn.SetEnabled(false);
            int currentIndex = i;

            btn.clicked += () =>
            {
                if (PlayerExpManager.PlayerData == null) return;

                if (PlayerExpManager.PlayerData.soulExp < upgradeCost)
                {
                    Debug.LogWarning($"소울이 부족해! {upgradeCost} 필요함!");
                    return;
                }

                // 소울 차감 + 언락 처리
                PlayerExpManager.PlayerData.soulExp -= upgradeCost;
                btn.userData = true;
                btn.SetEnabled(false);

                string key = $"{prefix}{currentIndex}";
                if (!PlayerExpManager.PlayerData.unlockedButtons.Contains(key))
                    PlayerExpManager.PlayerData.unlockedButtons.Add(key);

                // 버프 적용
                SoulBuffManager.ApplyBuffByButtonId($"{prefix}{currentIndex}");

                // UI 갱신 (FindFirstObjectByType로 경고 해결)
                var ui = Object.FindFirstObjectByType<UIController>();
                if (ui != null) ui.RefreshUI();

                if (soulLabel != null)
                    soulLabel.text = $"Soul: {PlayerExpManager.PlayerData.soulExp}";

                UpdateAllUnlockStatus();
            };

            // Tooltip On
            btn.RegisterCallback<MouseEnterEvent>((evt) =>
            {
                if (soulToolTip != null && soulToolTipLabel != null)
                {
                    string desc = prefix switch
                    {
                        "attack" => attackDescriptions[currentIndex],
                        "deffence" => defenceDescriptions[currentIndex],
                        "special" => specialDescriptions[currentIndex],
                        _ => "정보 없음"
                    };
                    soulToolTipLabel.text = desc;

                    Vector2 mousePos = root.WorldToLocal(evt.mousePosition);
                    float tooltipWidth = soulToolTip.resolvedStyle.width > 0 ? soulToolTip.resolvedStyle.width : 400f;
                    float tooltipHeight = soulToolTip.resolvedStyle.height > 0 ? soulToolTip.resolvedStyle.height : 500f;
                    float viewWidth = root.resolvedStyle.width;
                    float viewHeight = root.resolvedStyle.height;

                    float xOffset = 20f, yOffset = 20f;
                    float x = (mousePos.x + tooltipWidth + xOffset > viewWidth)
                        ? mousePos.x - tooltipWidth - xOffset
                        : mousePos.x + xOffset;
                    float y = Mathf.Clamp(mousePos.y + yOffset, 0, viewHeight - tooltipHeight);

                    soulToolTip.style.left = x;
                    soulToolTip.style.top = y;

                    soulToolTip.style.display = DisplayStyle.None;
                    root.schedule.Execute(() =>
                    {
                        if (soulToolTip.style.display == DisplayStyle.None)
                            soulToolTip.style.display = DisplayStyle.Flex;
                    }).ExecuteLater(1);
                }
            });

            // Tooltip Off
            btn.RegisterCallback<MouseLeaveEvent>((_) =>
            {
                if (soulToolTip != null)
                    soulToolTip.style.display = DisplayStyle.None;
            });
        }

        UpdateUnlockStatus(prefix, count, costPerUpgrade);
    }

    void UpdateUnlockStatus(string prefix, int count, int costPerUpgrade)
    {
        int currentSoul = PlayerExpManager.PlayerData != null ? PlayerExpManager.PlayerData.soulExp : 0;

        for (int i = 0; i < count; i++)
        {
            var btn = root.Q<Button>($"{prefix}{i}");
            if (btn == null) continue;

            int upgradeCost = costPerUpgrade + (i * 10);
            bool wasClicked = (btn.userData is bool clickedFlag) && clickedFlag;

            // 이전 단계 언락 여부
            bool isPrevUnlocked = (i == 0);
            if (i > 0)
            {
                var prevBtn = root.Q<Button>($"{prefix}{i - 1}");
                if (prevBtn != null && prevBtn.userData is bool prevClicked && prevClicked)
                    isPrevUnlocked = true;
            }

            bool canUnlock = !wasClicked && currentSoul >= upgradeCost && isPrevUnlocked;

            if (wasClicked)
            {
                btn.SetEnabled(false);
                SetButtonImage(btn, "button_unlocked");
            }
            else if (canUnlock)
            {
                btn.SetEnabled(true);
                SetButtonImage(btn, "button_active");
            }
            else
            {
                btn.SetEnabled(false);
                SetButtonImage(btn, "button_locked");
            }
        }
    }

    void UpdateAllUnlockStatus()
    {
        UpdateUnlockStatus("attack", 7, 50);
        UpdateUnlockStatus("deffence", 7, 50);
        UpdateUnlockStatus("special", 7, 100);
    }

    public void AddSoul(int amount)
    {
        if (PlayerExpManager.PlayerData == null) return;
        PlayerExpManager.PlayerData.soulExp += amount;

        if (soulLabel != null)
            soulLabel.text = $"Soul: {PlayerExpManager.PlayerData.soulExp}";

        UpdateAllUnlockStatus();
    }

    public void SaveSoulStoreData()
    {
        if (PlayerExpManager.PlayerData == null) return;

        var saveData = PlayerExpManager.PlayerData;

        foreach (var category in new[] { "attack", "deffence", "special" })
        {
            for (int i = 0; i < 9; i++)
            {
                var btn = root.Q<Button>($"{category}{i}");
                if (btn != null && btn.userData is bool clicked && clicked)
                {
                    string key = $"{category}{i}";
                    if (!saveData.unlockedButtons.Contains(key))
                        saveData.unlockedButtons.Add(key);
                }
            }
        }

        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(SaveFilePath, json, Encoding.UTF8);
        // Debug.Log($"소울 상점 저장 완료: {SaveFilePath}");
    }

    public void LoadSoulStoreData()
    {
        if (!File.Exists(SaveFilePath)) return;

        string json = File.ReadAllText(SaveFilePath, Encoding.UTF8);
        var saveData = JsonUtility.FromJson<PlayerData>(json);
        if (saveData == null) return;

        if (PlayerExpManager.PlayerData != null)
            PlayerExpManager.PlayerData = saveData;

        if (soulLabel != null)
            soulLabel.text = $"Soul: {saveData.soulExp}";

        foreach (var category in new[] { "attack", "deffence", "special" })
        {
            for (int i = 0; i < 9; i++)
            {
                var btn = root.Q<Button>($"{category}{i}");
                if (btn == null) continue;

                string key = $"{category}{i}";
                bool isUnlocked = saveData.unlockedButtons.Contains(key);
                btn.userData = isUnlocked;
            }
        }

        UpdateAllUnlockStatus();
    }
}
