using UnityEngine;

[CreateAssetMenu(fileName = "SceneData", menuName = "SceneSystem/SceneData", order = 0)]
public class SceneData : ScriptableObject
{
    public string sceneName;        // Build Settings에 등록된 씬 이름
    public string displayName;      // UI 등에 표시할 이름
    public int buildIndex;          // 빌드 인덱스 (선택 사항)
    public Sprite previewImage;     // 씬 썸네일 (선택 사항)
    public bool isRandomCandidate;  // 랜덤 이동 후보인지 여부
}