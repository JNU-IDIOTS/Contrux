using UnityEngine;

public class MonsterDropGold : MonoBehaviour
{
    [SerializeField] private int goldAmount = 10;

    public void Drop()
    {
        PlayerGoldManager.Instance.AddGold(goldAmount);
    }
}
