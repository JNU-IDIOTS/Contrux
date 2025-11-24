using UnityEngine;

namespace MyProject.Core
{
    [System.Serializable]
    public class InventoryItem
    {
        public string itemName;
        public Sprite icon;
        [TextArea] public string description; // 설명 필드 추가
        public ItemData data;
    }

    [System.Serializable]
    public class ItemData
    {
        public int healthBonus;
        public int attackBonus;
    }
}