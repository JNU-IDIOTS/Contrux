using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private SceneTable sceneTable;

    public void LoadSceneByName(string name)
    {
        var data = sceneTable.GetSceneByName(name);
        if (data != null)
        {
            SceneManager.LoadScene(data.sceneName);
        }
        else
        {
            Debug.LogError($"씬 이름 '{name}'에 해당하는 데이터가 없습니다.");
        }
    }

    public void LoadSceneByIndex(int index)
    {
        if (index >= 0 && index < sceneTable.scenes.Length)
        {
            SceneManager.LoadScene(sceneTable.scenes[index].sceneName);
        }
        else
        {
            Debug.LogError("유효하지 않은 씬 인덱스");
        }
    }

    public void LoadRandomScene()
    {
        var candidates = sceneTable.GetRandomCandidates();
        if (candidates == null || candidates.Length == 0)
        {
            Debug.LogWarning("랜덤 대상 씬이 없습니다.");
            return;
        }

        int randomIndex = Random.Range(0, candidates.Length);
        SceneManager.LoadScene(candidates[randomIndex].sceneName);
    }
}
