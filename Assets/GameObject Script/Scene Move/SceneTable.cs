using UnityEngine;

[CreateAssetMenu(fileName = "SceneTable", menuName = "SceneSystem/SceneTable", order = 1)]
public class SceneTable : ScriptableObject
{
    public SceneData[] scenes;

    public SceneData GetSceneByName(string name)
    {
        foreach (var scene in scenes)
        {
            if (scene.sceneName == name)
                return scene;
        }
        return null;
    }

    public SceneData[] GetRandomCandidates()
    {
        return System.Array.FindAll(scenes, s => s.isRandomCandidate);
    }
}
