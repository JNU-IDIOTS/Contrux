using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "SynergyData", menuName = "Inventory/SynergyData")]
public class SynergyData : ScriptableObject
{
    [Header("기본 정보")]
    public string synergyId;
    public string displayName;
    public Sprite icon;
    [TextArea(2, 4)]
    public string description;
    
    [Header("활성화 조건")]
    public string requiredTag; // 예: "fire", "magic", "warrior"
    
    [Space]
    [Tooltip("시너지 레벨별 필요한 아이템 개수. 예: [2, 4, 6] 또는 [1, 4] 또는 [3, 5, 7, 9]")]
    public int[] thresholds = {2, 4, 6};
    
    [Header("레벨별 효과")]
    [Tooltip("thresholds 배열과 동일한 길이여야 함")]
    public SynergyEffect[] effects;
    
    [Header("시너지 타입")]
    public SynergyType synergyType = SynergyType.Additive;
    
    [Header("상호 배타 시너지")]
    [Tooltip("이 시너지가 활성화되면 비활성화될 시너지 ID들")]
    public List<string> incompatibleSynergyIds = new List<string>();
    
    /// <summary>
    /// 주어진 아이템 개수로 활성화 가능한 최고 레벨을 반환
    /// </summary>
    public int GetMaxLevel(int itemCount)
    {
        for (int i = thresholds.Length - 1; i >= 0; i--)
        {
            if (itemCount >= thresholds[i])
                return i + 1; // 레벨은 1부터 시작
        }
        return 0; // 활성화 안됨
    }
    
    /// <summary>
    /// 특정 레벨의 효과를 반환
    /// </summary>
    public SynergyEffect GetEffect(int level)
    {
        if (level <= 0 || level > effects.Length)
            return null;
            
        return effects[level - 1];
    }
    
    /// <summary>
    /// 다음 레벨까지 필요한 아이템 개수를 반환
    /// </summary>
    public int GetItemsNeededForNextLevel(int currentItemCount)
    {
        int currentLevel = GetMaxLevel(currentItemCount);
        
        if (currentLevel >= thresholds.Length)
            return -1; // 이미 최고 레벨
            
        return thresholds[currentLevel] - currentItemCount;
    }
    
    /// <summary>
    /// 현재 활성화된 모든 레벨의 임계값을 반환
    /// </summary>
    public List<int> GetActiveThresholds(int itemCount)
    {
        List<int> activeThresholds = new List<int>();
        
        for (int i = 0; i < thresholds.Length; i++)
        {
            if (itemCount >= thresholds[i])
            {
                activeThresholds.Add(thresholds[i]);
            }
        }
        
        return activeThresholds;
    }
    
    void OnValidate()
    {
        // Inspector에서 편집 시 검증
        if (thresholds.Length != effects.Length)
        {
            Debug.LogWarning($"[{name}] thresholds와 effects 배열의 길이가 다릅니다! " +
                           $"thresholds: {thresholds.Length}, effects: {effects.Length}");
        }
        
        // 임계값이 오름차순인지 확인
        for (int i = 1; i < thresholds.Length; i++)
        {
            if (thresholds[i] <= thresholds[i - 1])
            {
                Debug.LogWarning($"[{name}] thresholds는 오름차순이어야 합니다! " +
                               $"인덱스 {i}: {thresholds[i]} <= 인덱스 {i-1}: {thresholds[i-1]}");
            }
        }
    }
}

[System.Serializable]
public class SynergyEffect
{
    [Header("레벨 정보")]
    public int level; // 이 효과의 레벨 (1, 2, 3...)
    
    [Header("UI 색상")]
    [Tooltip("이 레벨에서 시너지 카드의 테두리 색상")]
    public Color borderColor = Color.white; // 기본값은 흰색
    
    [Header("스탯 보너스")]
    public int hpBonus;
    public int physicalAttackBonus;
    public int magicalAttackBonus;
    public int defenseBonus;
    
    [Header("특수 효과")]
    public float criticalChanceBonus; // 치명타 확률 증가 (%)
    public float attackSpeedBonus;    // 공격속도 증가 (%)
    public float moveSpeedBonus;      // 이동속도 증가 (%)
    
    // [Header("고급 효과")]
    // public bool enablesSpecialAbility; // 특수 능력 활성화
    // public string specialAbilityId;    // 특수 능력 ID
    
    // [TextArea(2, 3)]
    public string effectDescription; // 이 레벨 효과의 설명
    
    /// <summary>
    /// 이 효과가 빈 효과인지 확인
    /// </summary>
    public bool IsEmpty()
    {
        return hpBonus == 0 && 
               physicalAttackBonus == 0 && 
               magicalAttackBonus == 0 && 
               defenseBonus == 0 &&
               criticalChanceBonus == 0f &&
               attackSpeedBonus == 0f &&
               moveSpeedBonus == 0f;
    }
}

public enum SynergyType
{
    Additive,    // 누적형 (2레벨 = 1레벨 + 2레벨 효과)
    Override,    // 덮어쓰기형 (2레벨 = 2레벨 효과만)
    Multiplicative // 곱셈형 (기존 스탯에 곱하기)
}