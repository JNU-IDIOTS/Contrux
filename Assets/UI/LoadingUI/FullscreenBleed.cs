using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class FullscreenBleed : MonoBehaviour
{
    [SerializeField] Canvas targetCanvas;   // FaderCanvas 넣어두면 좋음
    [SerializeField] float bleedPixels = 1f;

    RectTransform rt;

    void Awake() { rt = (RectTransform)transform; Apply(); }
    void OnEnable() { Apply(); }
    void Update()
    {
        // 창 크기 바뀌면 다시 적용
        if (Screen.width != _w || Screen.height != _h) Apply();
    }

    int _w, _h;
    void Apply()
    {
        _w = Screen.width; _h = Screen.height;

        if (!targetCanvas) targetCanvas = GetComponentInParent<Canvas>();
        float scale = targetCanvas ? Mathf.Max(0.0001f, targetCanvas.scaleFactor) : 1f;
        float bleed = bleedPixels / scale; // 캔버스 스케일 보정

        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(-bleed, -bleed); // 좌하로 1px 확장
        rt.offsetMax = new Vector2(bleed, bleed); // 우상으로 1px 확장
    }
}
