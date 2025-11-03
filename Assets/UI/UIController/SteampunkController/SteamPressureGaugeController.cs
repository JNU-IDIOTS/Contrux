using UnityEngine;
using UnityEngine.UIElements;

public class SteamPressureGaugeController : MonoBehaviour
{
    public UIDocument uiDocument;
    public SteamPressureSystem steamSystem;

    private VisualElement needleWrapper;

    void OnEnable()
    {
        var root = uiDocument.rootVisualElement;
        needleWrapper = root.Q<VisualElement>("NeedleWrapper");
    }

    void Update()
    {
        if (needleWrapper == null || steamSystem == null) return;

        float ratio = steamSystem.CurrentPressure / steamSystem.MaxPressure;

        // 🔁 회전 각도 계산 (예: 180도 → 0도, 시계 반대방향)
        float angle = Mathf.Lerp(0f, 180f, ratio);
        needleWrapper.transform.rotation = Quaternion.Euler(0, 0, -angle);
    }
}
