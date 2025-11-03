using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class GenericPanelAdapter : BackPanelBase
{
    [Header("Back(ESC) 우선순위 (높을수록 먼저 닫힘)")]
    [SerializeField] private int priority = 50;
    public override int BackPriority => priority;

    [Header("열릴 때 게임 멈출지 (상점/소울창은 보통 false)")]
    [SerializeField] private bool pauseGameOnOpen = false;

    [Header("PauseMenu로 등록할 패널인지")]
    [SerializeField] private bool registerAsPauseMenu = false;

    [Header("옵션: 열릴 때/닫힐 때 이벤트")]
    public UnityEvent onShown;
    public UnityEvent onHidden;

    protected override void Awake()
    {
        base.Awake();
        // 시작은 숨김
        Hide();
        // 네비게이터 등록
        //UINavigator.Instance.Register(this, isPauseMenu: registerAsPauseMenu);
    }

    public override void Show()
    {
        base.Show();
        onShown?.Invoke();

        if (pauseGameOnOpen)
            PauseManager.Pause();     // 필요 UI는 열릴 때 게임 멈춤
        // 포커스가 필요하면 UXML 안의 첫 버튼 name으로 가져와서 Focus()
        // var firstBtn = viewRoot.Q<Button>("FirstButtonName");
        // firstBtn?.Focus();
    }

    public override void Hide()
    {
        base.Hide();
        onHidden?.Invoke();

        if (pauseGameOnOpen && PauseManager.IsPaused)
            PauseManager.Resume();    // 자신 때문에 멈췄다면 닫을 때 복귀
    }

    // 외부에서 열고 닫을 수 있게 편의 메서드
    public void Open() => Show();
    public void Close() => Hide();
}

