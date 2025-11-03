
public interface IBackPanel
{
    bool IsOpen { get; }
    int BackPriority { get; }   // 숫자 클수록 먼저 닫힘 (인벤토리 > 설정 > ... > 일시정지)
    void Show();
    void Hide();
}

