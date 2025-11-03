// SceneFlowLoader.cs
using UnityEngine;
using UnityEngine.SceneManagement;

// 씬 흐름에 따라 자동으로 다음 씬을 로딩하는 컴포넌트
public class SceneFlowLoader : MonoBehaviour
{
    [SerializeField] private SceneFlowMap flowMap; // 연결된 흐름 테이블

    // 현재 씬 기준으로 다음 씬 로드
    public void LoadNextScene()
    {
        string current = SceneManager.GetActiveScene().name;
        string next = flowMap != null ? flowMap.GetNextScene(current) : null;

        if (string.IsNullOrEmpty(next))
        {
            Debug.LogWarning($"흐름이 정의되지 않은 씬: {current}");
            return;
        }

        // DDOLSceneLoader가 살아 있으면 로딩창 경유(최소 2초, 페이드, 후처리 다 처리됨)
        if (DDOLSceneLoader.I != null)
        {
            DDOLSceneLoader.I.LoadLevel(next);
            return; // 기존 동기 로드는 실행하지 않음
        }

        // 로더가 없으면 기존 방식 유지(로딩창 없이 즉시 전환)
        SceneManager.LoadScene(next);
    }
}
