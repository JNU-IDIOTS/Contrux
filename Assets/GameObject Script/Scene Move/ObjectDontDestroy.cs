using UnityEngine;

public class ObjectDontDestroy : MonoBehaviour
{
    public static ObjectDontDestroy Instance;
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
