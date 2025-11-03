using UnityEngine;

public class WhiteOutlineNPC : MonoBehaviour
{
    public SpriteRenderer mainRenderer;
    private SpriteRenderer outlineRenderer;

    void Start()
    {
        // 자식 오브젝트 생성
        GameObject outlineObj = new GameObject("Outline");
        outlineObj.transform.parent = transform;
        outlineObj.transform.localPosition = Vector3.zero;
        outlineObj.transform.localScale = Vector3.one * 1.05f;

        // SpriteRenderer 생성 및 설정
        outlineRenderer = outlineObj.AddComponent<SpriteRenderer>();
        outlineRenderer.sprite = mainRenderer.sprite;
        outlineRenderer.sortingLayerID = mainRenderer.sortingLayerID;
        outlineRenderer.sortingOrder = mainRenderer.sortingOrder - 1;

        // 머티리얼 적용
        Material mat = Resources.Load<Material>("WhiteSilhouetteMat");
        if (mat != null)
        {
            outlineRenderer.material = mat;
        }
        else
        {
            Debug.LogError("WhiteSilhouetteMat을 Resources 폴더에 넣었는지 확인!");
        }
    }

    void LateUpdate()
    {
        if (mainRenderer != null && outlineRenderer != null)
        {
            outlineRenderer.sprite = mainRenderer.sprite;
            outlineRenderer.flipX = mainRenderer.flipX;
        }
    }
}
