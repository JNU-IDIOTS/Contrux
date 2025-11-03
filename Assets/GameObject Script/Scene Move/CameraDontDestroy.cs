using UnityEngine;

public class CameraDontDestroy : MonoBehaviour
{
    public static CameraDontDestroy Instance { get; private set; }
    public Transform CameraTransform => transform;
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // 중복 방지
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // 씬 이동해도 유지
    }
}
