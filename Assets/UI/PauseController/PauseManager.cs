using UnityEngine;

public static class PauseManager
{
    private static int _pauseCount = 0; // 중첩 카운트
    public static bool IsPaused => _pauseCount > 0;

    public static System.Action<bool> OnToggleGameplayInput; // true=입력활성, false=비활성

    public static void Pause()
    {
        _pauseCount++;
        if (_pauseCount == 1)
        {
            Time.timeScale = 0f;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            OnToggleGameplayInput?.Invoke(false);
        }
    }

    public static void Resume()
    {
        if (_pauseCount <= 0) { _pauseCount = 0; return; }
        _pauseCount--;
        if (_pauseCount == 0)
        {
            Time.timeScale = 1f;
            OnToggleGameplayInput?.Invoke(true);
        }
    }

    // 필요하면 현재 카운트 리셋용(디버그)
    public static void ForceResumeAll()
    {
        _pauseCount = 0;
        Time.timeScale = 1f;
        OnToggleGameplayInput?.Invoke(true);
    }
}
