using UnityEngine;

/// <summary>
/// 게임 전체 설정을 관리하는 싱글톤
/// 씬 전환 시에도 파괴되지 않고 CurrentMode를 유지합니다.
/// </summary>
[DefaultExecutionOrder(20)]  // Awake가 다른 스크립트보다 먼저 호출되도록 조정
public class GameSettings : MonoBehaviour
{
    public static GameSettings Instance { get; private set; }

    /// <summary>
    /// 플레이어가 선택한 모드
    /// </summary>
    public GameMode CurrentMode { get; set; } = GameMode.Steampunk;

    private void Awake()
    {
        // 싱글톤 인스턴스 초기화
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
