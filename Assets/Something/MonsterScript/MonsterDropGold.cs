using UnityEngine;

public class MonsterDropGold : MonoBehaviour
{
    [SerializeField] private int goldAmount = 10;

    public void Drop()
    {
        // TODO: 골드 시스템 추가 시 활성화
        // PlayerGoldManager.Instance.AddGold(goldAmount);
    }
}

