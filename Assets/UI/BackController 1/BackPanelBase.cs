using UnityEngine;
using UnityEngine.UIElements;

public abstract class BackPanelBase : MonoBehaviour, IBackPanel
{
    public bool IsOpen { get; private set; }
    public abstract int BackPriority { get; } // 스택 방식에서도 쓸 일 있을 수 있으니 유지

    protected UIDocument doc;
    protected VisualElement viewRoot; // name="Root"가 있으면 그걸, 없으면 전체

    protected virtual void Awake()
    {
        doc = GetComponent<UIDocument>();
        var r = doc.rootVisualElement;
        viewRoot = r.Q<VisualElement>("Root") ?? r;
    }

    public virtual void Show()
    {
        viewRoot.style.display = DisplayStyle.Flex;
        if (!IsOpen)
        {
            IsOpen = true;
            UINavigator.Instance?.NotifyOpened(this);
        }
    }

    public virtual void Hide()
    {
        viewRoot.style.display = DisplayStyle.None;
        if (IsOpen)
        {
            IsOpen = false;
            UINavigator.Instance?.NotifyClosed(this);
        }
    }
}
