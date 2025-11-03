using UnityEngine;
using System.Collections.Generic;

public class ChildPositionRestorer : MonoBehaviour
{
    private Dictionary<Transform, Vector3> originalLocalPositions = new();

    void Awake()
    {
        // 자식들의 초기 localPosition 저장
        foreach (Transform child in transform)
        {
            originalLocalPositions[child] = child.localPosition;
        }
    }

    public void RestoreLocalPositions()
    {
        // 저장된 localPosition으로 복원
        foreach (var kvp in originalLocalPositions)
        {
            if (kvp.Key != null)
                kvp.Key.localPosition = kvp.Value;
        }
    }
}