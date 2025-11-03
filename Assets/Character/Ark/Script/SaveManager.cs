using UnityEngine;
using System.IO; // 파일을 저장하고 불러오기 위한 기능을 사용하기 위해 포함

/// <summary>
/// statSaveData를 json으로 저장하거나 불러오는 매니저 클래스
/// 이 클래스는 어디서는 사용될 수 있도록 static 클래스로 정의함.
/// </summary>
public class SaveManager : MonoBehaviour
{
    /* 
    저장할 파일의 경로를 자동으로 지정해주는 변수 
    Application.persistentDataPath는 플렛폼에 따라 자동으로 적절한 저장 폴더를 알려줌
    결과적으로 ex) C:/Users/user/AppData/LocalLow/회사명/게임명/Ark_stat.json 으로 저장
    */
    private static string SavePath => Path.Combine(Application.persistentDataPath, "Ark_stat.json");

    public static SaveManager Instance { get; private set; }

    public static statSaveData StatData { get; private set; }

    private void Awake()
    {
        if (Instance != null) Destroy(gameObject);
        else Instance = this;

        DontDestroyOnLoad(gameObject);

        LoadOnce();
    }

    public void LoadOnce()
    {
        if (File.Exists(SavePath))
        {
            string json = File.ReadAllText(SavePath);
            StatData = JsonUtility.FromJson<statSaveData>(json);
        }
        else
        {
            StatData = new statSaveData();
        }

        Ark_stat.LoadFrom(StatData);
    }

    public void SaveNow()
    {
        Ark_stat.ApplyTo(StatData);
        string json = JsonUtility.ToJson(StatData, true);
        File.WriteAllText(SavePath, json);

        //Debug.Log($"[SAVE] 저장됨 : {SavePath}");
    }

    

}
