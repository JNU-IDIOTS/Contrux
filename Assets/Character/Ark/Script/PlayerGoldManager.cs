using System;
using UnityEngine;

public class PlayerGoldManager : MonoBehaviour
{
    public static PlayerGoldManager Instance;

    public int Gold { get; private set; } = 0;
    public event Action<int> OnGoldChanged;

    [SerializeField] private ChooseOne chooseone;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // 필요하다면 초기 골드 세팅이나 구독 등록 등도 여기서
        }
        else
        {
            //Destroy(gameObject);
        }
    }

    public void AddGold(int amount)
    {
        Gold += amount;
        OnGoldChanged?.Invoke(Gold);
    }

    public bool SpendGold(int amount)
    {
        if (Gold >= amount)
        {
            Gold -= amount;
            OnGoldChanged?.Invoke(Gold);
            return true;
        }
        return false;
    }
#if UNITY_EDITOR
    [ContextMenu("디버그: 골드 999로 설정")]
    private void DebugSetGold()
    {
        Gold = 999;
        OnGoldChanged?.Invoke(Gold);
    }
#endif
}
