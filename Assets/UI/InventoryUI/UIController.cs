using UnityEngine;
using UnityEngine.UIElements;

// 인벤토리/스탯 창을 ESC 스택에 연결하기 위해 BackPanelBase 상속
[RequireComponent(typeof(UIDocument))]
public class UIController : BackPanelBase
{
    // (의미상) 인벤토리는 다른 것보다 먼저 닫히게 높은 값 ? 스택 방식이라 숫자 영향은 적지만 유지
    public override int BackPriority => 100;

    // UI 요소들
    private Label remainingPointLabel;
    private Label hpLabel, strLabel, dexLabel, intLabel, LevelLabel;
    private Button hpPlus, strPlus, dexPlus, intPlus;

    private int prevLevel = -1;

    // base.Awake()가 viewRoot/doc 설정을 해줌
    protected override void Awake()
    {
        base.Awake();

        // 1) UI 요소 찾기
        LevelLabel = viewRoot.Q<Label>("Level");
        hpLabel = viewRoot.Q<Label>("hpLabel");
        strLabel = viewRoot.Q<Label>("strLabel");
        dexLabel = viewRoot.Q<Label>("dexLabel");
        intLabel = viewRoot.Q<Label>("intLabel");
        remainingPointLabel = viewRoot.Q<Label>("remainingPoint");

        hpPlus = viewRoot.Q<Button>("hpPlus");
        strPlus = viewRoot.Q<Button>("strPlus");
        dexPlus = viewRoot.Q<Button>("dexPlus");
        intPlus = viewRoot.Q<Button>("intPlus");

        // 2) 버튼 이벤트
        if (hpPlus != null) hpPlus.clicked += () => { if (Ark_stat.IncreaseStat(Ark_stat.StatType.Health)) RefreshUI(); };
        if (strPlus != null) strPlus.clicked += () => { if (Ark_stat.IncreaseStat(Ark_stat.StatType.Strength)) RefreshUI(); };
        if (dexPlus != null) dexPlus.clicked += () => { if (Ark_stat.IncreaseStat(Ark_stat.StatType.Dexterity)) RefreshUI(); };
        if (intPlus != null) intPlus.clicked += () => { if (Ark_stat.IncreaseStat(Ark_stat.StatType.Intelligence)) RefreshUI(); };

        // 3) 시작은 숨김 + 네비게이터에 등록(일시정지 아님)
        Hide();
        UINavigator.Instance.Register(this);

        // 최초 1회 갱신
        RefreshUI();
    }

    // 인벤토리 패널은 열려도 게임을 멈추지 않는다(Time.timeScale 건드리지 않음)
    // BackPanelBase.Show/Hide가 자동으로 스택에 통지하므로 override 필요 없음.
    // 포커스 주고 싶으면 Show만 살짝 확장
    public override void Show()
    {
        base.Show();
        PauseManager.Pause();   // 인벤토리 열면 정지
        hpPlus?.Focus();        // 필요 시 포커스
    }

    public override void Hide()
    {
        base.Hide();
        PauseManager.Resume();  // 인벤토리 닫으면 정지 해제(중첩이면 유지됨)
    }


    public void RefreshUI()
    {
        // PlayerExpManager.PlayerData가 초기화 전일 수 있으니 방어
        if (PlayerExpManager.PlayerData != null && LevelLabel != null)
            LevelLabel.text = PlayerExpManager.PlayerData.playerLevel.ToString();

        if (hpLabel != null) hpLabel.text = Ark_stat.health.ToString();
        if (strLabel != null) strLabel.text = Ark_stat.strength.ToString();
        if (dexLabel != null) dexLabel.text = Ark_stat.dexterity.ToString();
        if (intLabel != null) intLabel.text = Ark_stat.intelligence.ToString();
        if (remainingPointLabel != null) remainingPointLabel.text = Ark_stat.remainingStatPoints.ToString();
    }

    void Update()
    {
        // i 키로 토글 ? 게임 멈추지 않음
        if (Input.GetKeyDown(KeyCode.I))
        {
            if (IsOpen) Hide();
            else Show();
        }

        // 테스트용 리셋
        if (Input.GetKeyDown(KeyCode.R))
        {
            Ark_stat.ResetStats();
            SaveManager.Instance.SaveNow();
            RefreshUI();
            Debug.Log("테스트 : 스탯이 초기화되었습니다.");
        }

        // 레벨 변화 감지 시 UI 갱신
        if (PlayerExpManager.PlayerData != null)
        {
            int currentLevel = PlayerExpManager.PlayerData.playerLevel;
            if (currentLevel != prevLevel)
            {
                prevLevel = currentLevel;
                RefreshUI();
            }
        }
    }
}
