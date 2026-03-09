using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class AttackIndicator : MonoBehaviour
{
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Collider2D targetCollider; 
    private float viewAngle = 360f; // 공격 각도

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        
        // 재질 설정 (기본 스프라이트 셰이더)
        meshRenderer.material = new Material(Shader.Find("Sprites/Default"));
        
        if (transform.parent != null)
            targetCollider = transform.parent.GetComponent<Collider2D>();
        
        gameObject.SetActive(false);
    }

    public void Setup(Sprite sprite, Color color, float angle)
    {
        // 색상 설정
        meshRenderer.material.color = color;
        this.viewAngle = angle;
    }

    public void Show()
    {
        if (targetCollider == null) return;
        gameObject.SetActive(true);
        GenerateMesh();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void GenerateMesh()
    {
        Mesh mesh = new Mesh();

        if (targetCollider is CircleCollider2D circle)
        {
            int segments = 50; 
            float radius = circle.radius; 
            
            
            float currentAngle = viewAngle;
            float startAngle = -currentAngle * 0.5f; 
            
            Vector3[] vertices = new Vector3[segments + 2];
            int[] triangles = new int[segments * 3];

            vertices[0] = circle.offset; 

            float angleStep = currentAngle / segments;
            
            for (int i = 0; i <= segments; i++)
            {
                float rad = (startAngle + (i * angleStep)) * Mathf.Deg2Rad;
                vertices[i + 1] = new Vector3(Mathf.Cos(rad) * radius, Mathf.Sin(rad) * radius, 0) + (Vector3)circle.offset;

                if (i < segments)
                {
                    triangles[i * 3] = 0;
                    triangles[i * 3 + 1] = i + 1;
                    triangles[i * 3 + 2] = i + 2;
                }
            }
            mesh.vertices = vertices;
            mesh.triangles = triangles;
        }
        else if (targetCollider is BoxCollider2D box)
        {
            // 박스 코드 (기존 동일)
            Vector2 size = box.size;
            Vector2 offset = box.offset;
            Vector3[] vertices = new Vector3[]
            {
                new Vector3(-size.x/2 + offset.x, -size.y/2 + offset.y, 0),
                new Vector3(size.x/2 + offset.x, -size.y/2 + offset.y, 0),
                new Vector3(-size.x/2 + offset.x, size.y/2 + offset.y, 0),
                new Vector3(size.x/2 + offset.x, size.y/2 + offset.y, 0)
            };
            mesh.vertices = vertices;
            mesh.triangles = new int[] { 0, 2, 1, 2, 3, 1 };
        }

        meshFilter.mesh = mesh;
    }
}