using UnityEngine;

public class EndGame : MonoBehaviour
{
    private void OnApplicationQuit()
    {
        SaveSystemManager.SaveOnExitLike();
    }

    void OnApplicationPause(bool pause)
    {
        if (pause) SaveSystemManager.SaveOnExitLike();
    }
}
