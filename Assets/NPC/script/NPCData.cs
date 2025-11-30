using UnityEngine;

// 상점 종류를 구분하기 위한 열거형
public enum NPCType
{
    SoulShop,   // 소울 상점 (능력치 강화)
    TalkOnly,   // 그냥 대화만 하는 NPC (나중에 퀘스트용)
    // WeaponShop, // (나중에 무기 상점 등이 생기면 여기에 추가)
}

[CreateAssetMenu(fileName = "New NPC Data", menuName = "Scriptable Object/NPC Data")]
public class NPCData : ScriptableObject
{
    [Header("NPC 기본 정보")]
    public string npcName = "상점 주인"; // NPC 이름 (예: 신비한 상인)
    
    [TextArea(3, 5)]
    public string greetingDialogue = "어서오게, 무엇이 필요한가?"; // 만났을 때 첫 대사
    
    [Header("기능 설정")]
    public NPCType npcType = NPCType.SoulShop; // 이 NPC의 역할
}