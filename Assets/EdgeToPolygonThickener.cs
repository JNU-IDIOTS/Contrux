// Assets/Scripts/EdgeToPolygonThickener.cs
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 통합 버전: 에디터 변환 + 런타임 자동 변환
/// - 에디터: 메뉴 창(Edge → Polygon)에서 선택 오브젝트 변환
/// - 런타임/빌드: 컴포넌트를 붙여두면 Awake에서 자동 변환
/// 최신 Unity(2023+)에서는 usedByComposite 대신 compositeOperation 사용
/// </summary>
[DefaultExecutionOrder(-1000)]
[DisallowMultipleComponent]
[RequireComponent(typeof(EdgeCollider2D))]
public class EdgeToPolygonThickener : MonoBehaviour
{
    [Header("Thickness (world units)")]
    [Min(0.001f)] public float thickness = 0.1f;

    [Header("When to convert (Runtime)")]
    public bool convertOnAwake = true;   // 런타임/빌드에서 Awake 때 자동 변환

    [Header("Edge handling")]
    public bool disableOriginalEdge = true;
    public bool destroyOriginalEdge = false;

    [Header("Polygon Collider options")]
    public bool copyIsTrigger = true;
    public PhysicsMaterial2D physicsMaterial;  // 필요시 지정 (null 가능)
    public bool useComposite = false;          // CompositeCollider2D로 합칠지

    void Awake()
    {
        if (convertOnAwake)
        {
            ConvertNow();
        }
    }

    [ContextMenu("Convert Now (Runtime)")]
    public void ConvertNow()
    {
        var edge = GetComponent<EdgeCollider2D>();
        if (edge == null)
        {
            Debug.LogWarning("[EdgeToPolygonThickener] EdgeCollider2D가 없습니다.", this);
            return;
        }

        bool ok = ConvertEdgeToPolygon(
            gameObject, edge, thickness,
            disableOriginalEdge, destroyOriginalEdge,
            copyIsTrigger, physicsMaterial, useComposite,
            useUndo: false // 런타임은 Undo 불가
        );

        if (ok)
            Debug.Log($"[EdgeToPolygonThickener] 런타임 변환 완료: {name}, thickness={thickness}", this);
    }

    /// <summary>
    /// 실제 변환 로직(에디터/런타임 공용)
    /// </summary>
    public static bool ConvertEdgeToPolygon(
        GameObject go,
        EdgeCollider2D edge,
        float thickness,
        bool disableEdge,
        bool destroyEdge,
        bool copyIsTrigger,
        PhysicsMaterial2D material,
        bool useComposite,
        bool useUndo // 에디터에서는 true로, 런타임은 false로
    )
    {
        if (edge == null) return false;
        var points = edge.points;
        if (points == null || points.Length < 2)
        {
            Debug.LogWarning($"[Edge → Poly] {go.name}: 포인트가 2개 미만입니다.", go);
            return false;
        }

        float half = Mathf.Max(0.0005f, thickness * 0.5f);

        // 평균 노멀 계산 (로컬)
        Vector2[] normals = new Vector2[points.Length];
        for (int i = 0; i < points.Length; i++)
        {
            Vector2 n = Vector2.zero;

            if (i > 0)
            {
                var dirPrev = (points[i] - points[i - 1]).normalized;
                n += new Vector2(-dirPrev.y, dirPrev.x);
            }
            if (i < points.Length - 1)
            {
                var dirNext = (points[i + 1] - points[i]).normalized;
                n += new Vector2(-dirNext.y, dirNext.x);
            }

            if (n.sqrMagnitude < 1e-6f)
            {
                // 끝점/직선 보정
                Vector2 dir;
                if (i == 0) dir = (points[1] - points[0]).normalized;
                else if (i == points.Length - 1) dir = (points[i] - points[i - 1]).normalized;
                else dir = (points[i + 1] - points[i - 1]).normalized;

                n = new Vector2(-dir.y, dir.x);
            }
            normals[i] = n.normalized;
        }

        Vector2 edgeOffset = edge.offset;
        var right = new List<Vector2>(points.Length);
        var left = new List<Vector2>(points.Length);

        for (int i = 0; i < points.Length; i++)
        {
            var p = points[i] + edgeOffset; // 로컬 좌표
            var n = normals[i];

            right.Add(p + n * half);
            left.Add(p - n * half);
        }

        // 폴리곤 경로(반시계 권장): 오른쪽 진행 → 왼쪽 역순
        var polyPath = new List<Vector2>(right.Count + left.Count);
        polyPath.AddRange(right);
        for (int i = left.Count - 1; i >= 0; i--)
            polyPath.Add(left[i]);

        // PolygonCollider2D 생성/획득
        PolygonCollider2D poly = go.GetComponent<PolygonCollider2D>();
        if (!poly)
        {
#if UNITY_EDITOR
            if (useUndo) poly = (PolygonCollider2D)UnityEditor.Undo.AddComponent<PolygonCollider2D>(go);
            else poly = go.AddComponent<PolygonCollider2D>();
#else
            poly = go.AddComponent<PolygonCollider2D>();
#endif
        }

#if UNITY_EDITOR
        if (useUndo) UnityEditor.Undo.RecordObject(poly, "Set Polygon Path");
#endif
        poly.pathCount = 1;
        poly.SetPath(0, polyPath.ToArray());
        poly.sharedMaterial = material;
        if (copyIsTrigger) poly.isTrigger = edge.isTrigger;

        // Composite 사용 시 세팅 (버전 분기)
        if (useComposite)
        {
#if UNITY_2023_1_OR_NEWER
            poly.compositeOperation = Collider2D.CompositeOperation.Merge;
#else
            poly.usedByComposite = true;
#endif

            var rb = go.GetComponent<Rigidbody2D>();
            if (!rb)
            {
#if UNITY_EDITOR
                if (useUndo) rb = (Rigidbody2D)UnityEditor.Undo.AddComponent<Rigidbody2D>(go);
                else rb = go.AddComponent<Rigidbody2D>();
#else
                rb = go.AddComponent<Rigidbody2D>();
#endif
            }
            rb.bodyType = RigidbodyType2D.Static;

            var comp = go.GetComponent<CompositeCollider2D>();
            if (!comp)
            {
#if UNITY_EDITOR
                if (useUndo) comp = (CompositeCollider2D)UnityEditor.Undo.AddComponent<CompositeCollider2D>(go);
                else comp = go.AddComponent<CompositeCollider2D>();
#else
                comp = go.AddComponent<CompositeCollider2D>();
#endif
            }
            comp.geometryType = CompositeCollider2D.GeometryType.Polygons;
        }

        // 원본 Edge 처리
        if (disableEdge) edge.enabled = false;
        if (destroyEdge)
        {
#if UNITY_EDITOR
            if (useUndo) UnityEditor.Undo.DestroyObjectImmediate(edge);
            else Object.DestroyImmediate(edge);
#else
            Object.Destroy(edge);
#endif
        }

        return true;
    }

#if UNITY_EDITOR
    // ─────────────────────────────────────────────────────────────────────────────
    // 에디터 전용: 인스펙터 버튼 & 메뉴 창
    // ─────────────────────────────────────────────────────────────────────────────
    [UnityEditor.CustomEditor(typeof(EdgeToPolygonThickener))]
    public class EdgeToPolygonThickenerInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Convert Now (Editor)"))
            {
                var comp = (EdgeToPolygonThickener)target;
                var edge = comp.GetComponent<EdgeCollider2D>();
                if (edge)
                {
                    ConvertEdgeToPolygon(
                        comp.gameObject, edge, comp.thickness,
                        comp.disableOriginalEdge, comp.destroyOriginalEdge,
                        comp.copyIsTrigger, comp.physicsMaterial, comp.useComposite,
                        useUndo: true
                    );
                    UnityEditor.EditorUtility.SetDirty(comp.gameObject);
                }
                else
                {
                    Debug.LogWarning("EdgeCollider2D가 없습니다.", comp);
                }
            }
        }
    }

    public class EdgeToPolygonThickenerWindow : UnityEditor.EditorWindow
    {
        float thickness = 0.1f;
        bool disableEdge = true;
        bool destroyEdge = false;
        bool copyIsTrigger = true;
        bool useComposite = false;
        PhysicsMaterial2D sharedMaterial;

        [UnityEditor.MenuItem("Tools/Physics2D/Edge → Polygon (Thicken)...")]
        public static void ShowWindow()
        {
            var win = GetWindow<EdgeToPolygonThickenerWindow>("Edge → Polygon");
            win.minSize = new Vector2(360, 230);
            win.Show();
        }

        void OnGUI()
        {
            GUILayout.Label("Edge → Polygon (Thickener)", UnityEditor.EditorStyles.boldLabel);
            thickness = UnityEditor.EditorGUILayout.Slider("Thickness", thickness, 0.01f, 1f);
            disableEdge = UnityEditor.EditorGUILayout.Toggle("Disable Original Edge", disableEdge);
            destroyEdge = UnityEditor.EditorGUILayout.Toggle("Destroy Original Edge", destroyEdge);
            copyIsTrigger = UnityEditor.EditorGUILayout.Toggle("Copy isTrigger", copyIsTrigger);
            useComposite = UnityEditor.EditorGUILayout.Toggle("Use Composite (Static RB)", useComposite);
            sharedMaterial = (PhysicsMaterial2D)UnityEditor.EditorGUILayout.ObjectField("Physics Material 2D", sharedMaterial, typeof(PhysicsMaterial2D), false);

            UnityEditor.EditorGUILayout.Space();
            if (GUILayout.Button("Convert Selected"))
            {
                var targets = UnityEditor.Selection.gameObjects;
                if (targets == null || targets.Length == 0)
                {
                    UnityEditor.EditorUtility.DisplayDialog("Edge → Polygon", "오브젝트를 선택하세요.", "OK");
                    return;
                }

                foreach (var go in targets)
                {
                    var edge = go.GetComponent<EdgeCollider2D>();
                    if (!edge) continue;

                    bool ok = EdgeToPolygonThickener.ConvertEdgeToPolygon(
                        go, edge, thickness,
                        disableEdge, destroyEdge,
                        copyIsTrigger, sharedMaterial, useComposite,
                        useUndo: true
                    );

                    if (ok)
                    {
                        UnityEditor.EditorUtility.SetDirty(go);
                        Debug.Log($"[Edge → Poly] '{go.name}' 변환 완료. thickness={thickness}", go);
                    }
                }
            }
        }
    }
#endif
}
