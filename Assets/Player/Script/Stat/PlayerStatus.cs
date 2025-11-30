using UnityEngine;

public class PlayerStatus : MonoBehaviour
{
    // [최종 적용 스탯] (StatDataManager가 계산해서 여기에 값을 넣어줌)
    [Header("--- [최종 적용 스탯 (자동 갱신됨)] ---")]
    public float AtkMultiplier = 1.0f;      
    public float FinalMaxHP = 100f;         
    public float FinalDefense = 0f;         
    public float CritChanceBonus = 0f;      
    public float CritDamageBonus = 0f;
    
    public float ItemCooldownMult = 1.0f;
    public float SkillCooldownMult = 1.0f;
    public float DamageReduceMult = 1.0f;
    
    public bool CanRevive = false;
    public float ReviveHpPercent = 0f;
    
    public float SteamCostMult = 1.0f;
    public float SteamCoolSpeedMult = 1.0f;
    public int BonusDashCount = 0;
    public bool IsOverheatEnhanced = false;

    private void Start()
    {
        // 시작하자마자 매니저에게 "내 스탯 채워줘!" 요청
        CalculateStats();
    }

    // 🚀 [에러 해결] 외부에서 이 함수를 호출하면 매니저에게 재계산을 요청함
    public void CalculateStats()
    {
        if (StatDataManager.Instance != null)
            StatDataManager.Instance.CalculateFinalStats();
    }

    // UI에서 미리보기 할 때 쓰는 함수
    public string GetStatPreview(string key, int lv)
    {
        // (이 함수는 StatDataManager로 기능을 옮겼지만, 혹시 모를 호환성을 위해 남겨둠)
        if (StatDataManager.Instance != null)
            return StatDataManager.Instance.GetStatPreview(key, lv);
            
        return "-";
    }
}