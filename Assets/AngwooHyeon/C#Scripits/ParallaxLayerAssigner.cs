using UnityEngine;
using UnityEngine.Tilemaps;

[ExecuteAlways]
public class ParallaxLayerAssigner : MonoBehaviour
{
    public LayerType layerType;
    public bool isSpriteRenderer = false;

    // 내부 추적용: 현재 설정된 레이어
    [SerializeField, HideInInspector]
    private LayerType _appliedLayerType = (LayerType)(-1); // 초기값 무효값

    void OnValidate()
    {
        // layerType이 바뀌었을 때만 설정 적용
        if (_appliedLayerType != layerType)
        {
            ApplyLayerSettings();
            _appliedLayerType = layerType;
        }
    }

    void ApplyLayerSettings()
    {
        // 기본값 정의
        float z = 0f;
        int sortingOrder = 0;
        float parallaxFactor = 0f;

        switch (layerType)
        {
            case LayerType.FarFarBack: z = 10f; sortingOrder = -100; parallaxFactor = 0.01f; break;
            case LayerType.FarBack: z = 8f; sortingOrder = -50; parallaxFactor = 0.03f; break;
            case LayerType.MidBack: z = 6f; sortingOrder = -30; parallaxFactor = 0.05f; break;
            case LayerType.Mid: z = 4f; sortingOrder = -10; parallaxFactor = 0.1f; break;
            case LayerType.Foreground: z = -2f; sortingOrder = 10; parallaxFactor = 0.2f; break;
            case LayerType.ForegroundClose: z = -4f; sortingOrder = 30; parallaxFactor = 0.3f; break;
            case LayerType.UI: z = -10f; sortingOrder = 100; parallaxFactor = 0f; break;
        }

        // Z값은 현재 값과 다를 때만 변경
        Vector3 pos = transform.position;
        if (Mathf.Abs(pos.z - z) > 0.001f)
            transform.position = new Vector3(pos.x, pos.y, z);

        if (isSpriteRenderer)
        {
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null && sr.sortingOrder != sortingOrder)
                sr.sortingOrder = sortingOrder;
        }
        else
        {
            var tr = GetComponent<TilemapRenderer>();
            if (tr != null)
            {
                if (tr.sortingOrder != sortingOrder)
                    tr.sortingOrder = sortingOrder;

                tr.mode = TilemapRenderer.Mode.Individual;
            }
        }

        var pLayer = GetComponent<ParallaxLayer>();
        if (pLayer == null)
            pLayer = gameObject.AddComponent<ParallaxLayer>();

        if (Mathf.Abs(pLayer.parallaxFactor - parallaxFactor) > 0.001f)
            pLayer.parallaxFactor = parallaxFactor;
    }
}
