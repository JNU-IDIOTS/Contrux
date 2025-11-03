// Assets/Audio/SceneMusicBootstrap.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneMusicBootstrap : MonoBehaviour
{
    public SceneMusicMap map;

    void Start()
    {
        var sceneName = SceneManager.GetActiveScene().name;
        if (MusicPlayer.Instance)
        {
            MusicPlayer.Instance.PlayForScene(map, sceneName);
        }
        else
        {
            Debug.LogWarning("[SceneMusicBootstrap] MusicPlayer가 씬에 없습니다. 프리팹을 스타트씬에 두고 DDOL로 유지하세요.");
        }
    }
}
