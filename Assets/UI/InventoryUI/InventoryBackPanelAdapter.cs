using UnityEngine;

[RequireComponent(typeof(InventoryUIController))]
public class InventoryBackPanelAdapter : BackPanelBase
{
    InventoryUIController inv;

    public override int BackPriority => 90;

    // 1) Awake는 최소화 (부모 Awake + 컴포넌트 캐싱만)
    protected override void Awake()
    {
        base.Awake();
        inv = GetComponent<InventoryUIController>();
        // 여기서 Register/Hide 호출하지 않음
    }

    // 2) Register/Hide는 Start에서 (Awake들 끝난 뒤)
    private void Start()
    {
        if (UINavigator.Instance == null)
        {
            Debug.LogError("[InventoryBackPanelAdapter] UINavigator.Instance가 아직 없습니다.");
            return;
        }

        UINavigator.Instance.Register(this, isPauseMenu: false, group: PanelGroup.Overlay);
        if (!IsOpen) Hide();// 시작은 닫힘 (이때 inv.CloseInventory() 호출됨)
    }

    public override void Show()
    {
        UINavigator.Instance?.CloseAllOverlaysExcept(this);
        base.Show();
        inv.OpenInventory();
        PauseManager.Pause();
    }

    public override void Hide()
    {
        inv.CloseInventory();
        base.Hide();
        if (PauseManager.IsPaused) PauseManager.Resume();
    }
}