/// <summary>
/// 플레이어가 선택할 모드를 정의하는 enum
/// </summary>
public enum GameMode
{
    Steampunk,  // 스팀펑크 테마
    Rune        // 룬 마력 테마
}
public class GameType
{
    public bool type(GameMode type)
    {
        if(type == GameMode.Steampunk)
            return true;
        else
            return false;
    }
}

/*public static class CritChanceCal
{
    public static bool CritChanceOK = false;
    public static void CritChanceCalculate(GameMode type)
    {
        if (type == GameMode.Steampunk)
        {
            if (SteamPunk_Attack.Critrand <= Attack_Damage.SoulBuffCritChance)
            {
                //Debug.Log($"크리티컬 확률 랜덤 수는 {Attack.Critrand}, 크리티컬 데미지가 적용됩니다.");
                CritChanceOK = true;
            }
            else
            {
                //Debug.Log($"크리티컬 확률 랜덤 수는 {Attack.Critrand}, 크리티컬 데미지가 적용 안됩니다.");
            }
        }
        else
        {
            if (Magic_Attack.Critrand <= Attack_Damage.SoulBuffCritChance)
            {
                //Debug.Log($"크리티컬 확률 랜덤 수는 {Attack.Critrand}, 크리티컬 데미지가 적용됩니다.");
                CritChanceOK = true;
            }
            else
            {
                //Debug.Log($"크리티컬 확률 랜덤 수는 {Attack.Critrand}, 크리티컬 데미지가 적용 안됩니다.");
            }
        }
    }
}*/