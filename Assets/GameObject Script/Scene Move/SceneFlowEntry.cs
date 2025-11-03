// SceneFlowEntry.cs
using UnityEngine;

// 각 씬의 흐름을 정의하는 데이터 클래스
[System.Serializable]
public class SceneFlowEntry
{
    public string currentSceneName; // 현재 씬 이름

    public bool useRandomNext = false; // 랜덤 분기 여부
    public string nextSceneName;       // 단일 다음 씬 이름 (useRandomNext == false)
    public string[] randomNextScenes;  // 랜덤 분기 대상 씬 리스트 (useRandomNext == true)
}
