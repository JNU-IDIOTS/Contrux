// SceneFlowMapEditor.cs (커스텀 에디터)
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

// SceneFlowMap을 위한 커스텀 인스펙터
[CustomEditor(typeof(SceneFlowMap))]
public class SceneFlowMapEditor : Editor
{
    SerializedProperty flowEntries;

    private void OnEnable()
    {
        flowEntries = serializedObject.FindProperty("flowEntries");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("\uD83C\uDF1F 씬 흐름 테이블", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        for (int i = 0; i < flowEntries.arraySize; i++)
        {
            var entry = flowEntries.GetArrayElementAtIndex(i);
            var current = entry.FindPropertyRelative("currentSceneName");
            var useRandom = entry.FindPropertyRelative("useRandomNext");
            var next = entry.FindPropertyRelative("nextSceneName");
            var randomList = entry.FindPropertyRelative("randomNextScenes");

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.PropertyField(current, new GUIContent("현재 씬 이름"));
            EditorGUILayout.PropertyField(useRandom, new GUIContent("랜덤 다음 씬 사용"));

            if (useRandom.boolValue)
            {
                EditorGUILayout.PropertyField(randomList, new GUIContent("랜덤 다음 씬들"), true);
            }
            else
            {
                EditorGUILayout.PropertyField(next, new GUIContent("다음 씬 이름"));
            }

            if (GUILayout.Button("삭제"))
            {
                flowEntries.DeleteArrayElementAtIndex(i);
                break;
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        if (GUILayout.Button("새 흐름 추가"))
        {
            flowEntries.InsertArrayElementAtIndex(flowEntries.arraySize);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
