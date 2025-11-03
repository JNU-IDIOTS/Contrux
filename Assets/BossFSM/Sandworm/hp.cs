using UnityEngine;

public class hp : MonoBehaviour
{
    public static float boss1_health = 1000;
    public static float boss2_health = 1000;
    public static float SandWorm = 1000;

    // 태그에 따라 MaxHP를 반환
    public static float GetMaxHPByTag(string tag)
    {
        switch (tag)
        {
            case "Boss1":
                return boss1_health;
            case "boss2":
                return boss2_health;
            case "SandWorm":
                return SandWorm;
            default:
                Debug.LogWarning($"정의되지 않은 태그: {tag}");
                return 0;
        }
    }
}