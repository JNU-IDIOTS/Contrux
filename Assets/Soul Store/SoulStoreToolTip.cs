using UnityEngine;
using UnityEngine.UIElements;

public class SoulStoreToolTip : MonoBehaviour
{
    public VisualTreeAsset tooltipUXML;

    private VisualElement tooltipRoot;
    private Label tooltipLabel;

    public void Init(VisualElement parentRoot)
    {
        tooltipRoot = tooltipUXML.CloneTree();
        tooltipLabel = tooltipRoot.Q<Label>("SoulToolTipLabel");

        tooltipRoot.style.position = Position.Absolute;
        tooltipRoot.style.display = DisplayStyle.None;

        parentRoot.Add(tooltipRoot);
    }

    public void Show(string message, Vector2 screenPosition)
    {
        tooltipLabel.text = message;
        tooltipRoot.style.left = screenPosition.x + 10;
        tooltipRoot.style.top = screenPosition.y + 10;
        tooltipRoot.style.display = DisplayStyle.Flex;
        
    }

    public void Hide()
    {
        tooltipRoot.style.display = DisplayStyle.None;
    }
}
