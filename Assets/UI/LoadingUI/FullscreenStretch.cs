using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
public class FullscreenStretch : MonoBehaviour
{
    RectTransform rt;
    int sw, sh;

    void Awake()
    {
        rt = (RectTransform)transform;
        Apply();
        sw = Screen.width; sh = Screen.height;
    }

    void OnEnable() => Apply();   // 활성화될 때 한 번

    void Update()                 // 해상도/창 크기 바뀌면만 다시 적용
    {
        if (sw != Screen.width || sh != Screen.height)
        {
            sw = Screen.width; sh = Screen.height;
            Apply();
        }
    }

    void Apply()
    {
        rt.anchorMin = Vector2.zero; // (0,0)
        rt.anchorMax = Vector2.one;  // (1,1)
        rt.offsetMin = Vector2.zero; // L,B = 0
        rt.offsetMax = Vector2.zero; // R,T = 0
    }
}