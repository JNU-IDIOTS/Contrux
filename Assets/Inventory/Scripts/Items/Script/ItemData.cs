using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ItemData", menuName = "Inventory/ItemData")]
public class ItemData : ScriptableObject
{
    [Header("기본 정보")]
    public int id;
    public string itemName;

    public int itemType; // 시너지 혹은 장비 종류 구분용
    public Sprite icon;
    public int count = 1;

    // 아이템 스탯 들어갈 자리
    public int InhanceLevel;
    public int HP;
    public int MagicDeal;
    public int PhysicalDeal;
    public int Defense;
    [TextArea]
    public string description;
    [TextArea]
    public string performance;

    [Header("시너지 시스템")]
    [Tooltip("이 아이템이 가진 시너지 태그들. Resources/Synergies 폴더의 SynergyData에서 선택")]
    public List<string> synergyTags = new List<string>();
    
    [Tooltip("시너지 티어 (1성, 2성, 3성 등). 높을수록 더 강력한 시너지 효과")]
    [Range(1, 5)]
    public int synergyTier = 1;
    
    [Tooltip("이 아이템이 특정 시너지에 기여하는 가중치 (기본 1)")]
    public int synergyWeight = 1;

    /// <summary>
    /// 특정 시너지 태그를 가지고 있는지 확인
    /// </summary>
    public bool HasSynergyTag(string tag)
    {
        return synergyTags.Contains(tag);
    }
    
    /// <summary>
    /// 시너지 태그가 있는 아이템인지 확인
    /// </summary>
    public bool IsSynergyItem()
    {
        return synergyTags.Count > 0;
    }
    
    /// <summary>
    /// 이 아이템이 특정 시너지에 기여하는 실제 값 (티어 * 가중치)
    /// </summary>
    public int GetSynergyContribution()
    {
        return synergyTier * synergyWeight;
    }
}
