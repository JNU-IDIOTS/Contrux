//using System;
//using UnityEngine;

//public class PlayerGoldManager : MonoBehaviour
//{
//    public static PlayerGoldManager Instance;

//    public int Gold { get; private set; } = 0;
//    private const string GoldKey = "PlayerGold";

//    public event Action<int> OnGoldChanged;

//    private void Awake()
//    {
//        if (Instance == null)
//            Instance = this;
//        else
//        {
//            Destroy(gameObject);
//            return;
//        }

//        Gold = PlayerPrefs.GetInt(GoldKey, 0);
//    }
//    public void SaveGold()
//    {
//        PlayerPrefs.SetInt(GoldKey, Gold);
//        PlayerPrefs.Save();
//    }

//    public void AddGold(int amount)
//    {
//        Gold += amount;
//        Debug.Log($"°ρµε Αυ°΅µΚ: {Gold}");
//        OnGoldChanged?.Invoke(Gold);
//    }

//    public bool SpendGold(int amount)
//    {
//        if (Gold >= amount)
//        {
//            Gold -= amount;
//            OnGoldChanged?.Invoke(Gold);
//            return true;
//        }
//        return false;
//    }

//    private void OnApplicationQuit()
//    {
//        SaveGold();
//    }
//}
