// 파일 상단: 여전히 유지
using System.Collections.Generic;
using UnityEngine;

public enum PanelGroup { None, Pause, Overlay }

public class UINavigator : MonoBehaviour
{
    public static UINavigator Instance { get; private set; }

    private readonly List<IBackPanel> openStack = new();
    private IBackPanel pauseMenu;

    private readonly Dictionary<IBackPanel, PanelGroup> _panelGroup = new();
    private PanelGroup _currentGroup = PanelGroup.None;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Register(IBackPanel panel, bool isPauseMenu = false, PanelGroup group = PanelGroup.Overlay)
    {
        if (isPauseMenu) { pauseMenu = panel; group = PanelGroup.Pause; }
        _panelGroup[panel] = group;
    }

    // ✅ 추가: 같은 오버레이 그룹 내 다른 패널은 모두 닫기
    public void CloseAllOverlaysExcept(IBackPanel except)
    {
        for (int i = openStack.Count - 1; i >= 0; --i)
        {
            var p = openStack[i];
            if (p == except) continue;
            if (_panelGroup.TryGetValue(p, out var g) && g == PanelGroup.Overlay)
                p.Hide(); // Hide() 안에서 NotifyClosed 호출된다고 가정
        }
    }

    public void NotifyOpened(IBackPanel panel)
    {
        var group = _panelGroup.TryGetValue(panel, out var g) ? g : PanelGroup.Overlay;

        // 🔒 그룹 배타: Pause ↔ Overlay는 서로 동시에 금지
        if (_currentGroup != PanelGroup.None && _currentGroup != group)
        {
            for (int i = openStack.Count - 1; i >= 0; --i)
            {
                var p = openStack[i];
                if (_panelGroup.TryGetValue(p, out var pg) && pg != group)
                    p.Hide();
            }
        }

        // 🔒 오버레이끼리도 1개만 허용 (여기서도 보수적으로 한 번 더)
        if (group == PanelGroup.Overlay)
            CloseAllOverlaysExcept(panel);

        // 스택 갱신
        openStack.Remove(panel);
        openStack.Add(panel);
        _currentGroup = group;

        // 정책: 오버레이도 게임 멈춤 유지
        PauseManager.Pause();
    }

    public void NotifyClosed(IBackPanel panel)
    {
        openStack.Remove(panel);

        if (openStack.Count == 0)
        {
            _currentGroup = PanelGroup.None;
            PauseManager.Resume();
        }
        else
        {
            var top = openStack[openStack.Count - 1];
            _currentGroup = _panelGroup.TryGetValue(top, out var g) ? g : PanelGroup.Overlay;
        }
    }

    void Update()
    {
#if ENABLE_INPUT_SYSTEM
        var kb = UnityEngine.InputSystem.Keyboard.current;
        if (kb != null && kb.escapeKey.wasPressedThisFrame) OnEsc();
#else
        if (Input.GetKeyDown(KeyCode.Escape)) OnEsc();
#endif
    }

    void OnEsc()
    {
        if (openStack.Count > 0)
        {
            var top = openStack[^1];
            if (pauseMenu != null && top == pauseMenu)
            {
                pauseMenu.Hide();
                PauseManager.Resume();
                return;
            }
            top.Hide();
            return;
        }

        if (pauseMenu != null)
        {
            pauseMenu.Show(); // Pause는 NotifyOpened에서 처리
        }
    }
}
