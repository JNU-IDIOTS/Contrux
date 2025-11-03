// SceneFlowGraphWindow.cs
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;

// 씬 흐름을 시각적으로 보여주는 그래프 윈도우
public class SceneFlowGraphWindow : EditorWindow
{
    private SceneFlowMap flowMap;
    private Vector2 scrollPos;
    private Dictionary<string, Vector2> nodePositions = new();

    [MenuItem("Window/Scene System/Scene Flow Visualizer")]
    public static void ShowWindow()
    {
        GetWindow<SceneFlowGraphWindow>("Scene Flow Visualizer");
    }

    private void OnGUI()
    {
        flowMap = (SceneFlowMap)EditorGUILayout.ObjectField("Flow Map", flowMap, typeof(SceneFlowMap), false);

        if (flowMap == null) return;

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        Handles.BeginGUI();

        Dictionary<string, Rect> nodeRects = new();

        // 노드 렌더링
        for (int i = 0; i < flowMap.flowEntries.Length; i++)
        {
            var entry = flowMap.flowEntries[i];
            Vector2 pos = nodePositions.ContainsKey(entry.currentSceneName)
                ? nodePositions[entry.currentSceneName]
                : new Vector2(150, i * 120 + 50);

            Rect rect = new Rect(pos.x, pos.y, 140, 60);
            nodeRects[entry.currentSceneName] = rect;

            GUI.Box(rect, entry.currentSceneName);

            // 드래그 가능
            EditorGUIUtility.AddCursorRect(rect, MouseCursor.MoveArrow);
            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            {
                if (Event.current.clickCount == 2)
                {
                    EditorGUIUtility.PingObject(flowMap);
                }
                GUI.FocusControl(null);
            }

            nodePositions[entry.currentSceneName] = rect.position;
        }

        // 연결선 렌더링
        foreach (var entry in flowMap.flowEntries)
        {
            if (!nodeRects.ContainsKey(entry.currentSceneName)) continue;
            Rect fromRect = nodeRects[entry.currentSceneName];
            Vector3 from = new Vector3(fromRect.xMax, fromRect.center.y);

            if (entry.useRandomNext)
            {
                foreach (var next in entry.randomNextScenes)
                {
                    if (!nodeRects.ContainsKey(next)) continue;
                    Rect toRect = nodeRects[next];
                    Vector3 to = new Vector3(toRect.xMin, toRect.center.y);
                    Handles.DrawBezier(from, to, from + Vector3.right * 50, to + Vector3.left * 50, Color.cyan, null, 2);
                }
            }
            else
            {
                if (!nodeRects.ContainsKey(entry.nextSceneName)) continue;
                Rect toRect = nodeRects[entry.nextSceneName];
                Vector3 to = new Vector3(toRect.xMin, toRect.center.y);
                Handles.DrawBezier(from, to, from + Vector3.right * 50, to + Vector3.left * 50, Color.white, null, 2);
            }
        }

        Handles.EndGUI();
        EditorGUILayout.EndScrollView();
    }
}
