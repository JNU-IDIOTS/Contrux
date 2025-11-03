using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class BoxColliderGizmo : MonoBehaviour
{
    private BoxCollider2D boxCollider;
    public Boss1_Phasechange phasechange;
    public bool DdalGGak = true;

    public void Awake()
    {
        phasechange = GetComponent<Boss1_Phasechange>();
        boxCollider = GetComponent<BoxCollider2D>();
    }
    private void Update()
    {
        if (DdalGGak)
        {
            boxCollider.isTrigger = true;
        }
        else
        {
            boxCollider.isTrigger = false;
        }

    }
    private void OnDrawGizmos()
    {
        if (boxCollider == null)
            boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider != null)
        {
            // Gizmo 색상 설정 (원하는 색으로 변경 가능)
            Gizmos.color = new Color(0, 1, 0, 0.3f); // 반투명 초록색

            // 박스 콜라이더 위치와 크기 계산 (로컬 -> 월드 좌표 변환)
            Vector3 colliderCenter = transform.position + (Vector3)boxCollider.offset;
            Vector3 colliderSize = new Vector3(boxCollider.size.x * transform.lossyScale.x, boxCollider.size.y * transform.lossyScale.y, 0f);

            // Gizmo로 박스 콜라이더 크기만큼 박스 그리기
            Gizmos.DrawCube(colliderCenter, colliderSize);
        }
    }
}
