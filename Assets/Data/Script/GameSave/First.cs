using UnityEngine;

public class First : MonoBehaviour
{
    void Awake()
    {
        SaveSystemManager.ResumeOrInitAtStartup();// 씬 시작시 중간 저장 데이터를 불러오기  이부분 수정
        PlayerExpManager.InitPlayerData();
        //Debug.Log(" 초기화 완료");
    }
}
