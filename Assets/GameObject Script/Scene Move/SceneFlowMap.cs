// SceneFlowMap.cs
using UnityEngine;

// 전체 씬 흐름을 관리하는 ScriptableObject
[CreateAssetMenu(fileName = "SceneFlowMap", menuName = "SceneSystem/SceneFlowMap")]
public class SceneFlowMap : ScriptableObject
{
    public SceneFlowEntry[] flowEntries; // 씬 흐름 리스트

    // 현재 씬 이름에 따라 다음 씬 이름 반환 (랜덤 포함)
    public string GetNextScene(string current)
    {
        foreach (var entry in flowEntries)
        {
            if (entry.currentSceneName == current)
            {
                if (entry.useRandomNext && entry.randomNextScenes.Length > 0)
                {
                    int index = Random.Range(0, entry.randomNextScenes.Length);
                    return entry.randomNextScenes[index];
                }
                else
                {
                    return entry.nextSceneName;
                }
            }
        }
        return null; // 매칭 안 될 경우
    }
}