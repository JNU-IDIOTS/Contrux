// SuspendedRunManager.cs
using System;
using System.Collections;
using UnityEngine;

public static class SuspendedRunManager
{
    public static void SaveCurrentRun()
    {
        if (PlayerExpManager.PlayerData == null) return; // NRE 방지

        var data = new SuspendedData
        {
            playerLevel = PlayerExpManager.PlayerData.playerLevel,
            playerExp = PlayerExpManager.PlayerData.playerExp,
            totalExp = PlayerExpManager.PlayerData.playerTotalExp,
            expToNextLevel = PlayerExpManager.PlayerData.expToNextLevel,
            currentHP = PlayerHealth.currentHP,
            sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,
            savedTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss")
        };

        SuspendedData.Save(data);
    }


    public static void ResumeIfExists()
    {
        // 1) 메타 로드 보장 (소울/버프/해금 버튼)
        PlayerExpSave.LoadExp();                       // PlayerExpManager.PlayerData 채워짐
        if (PlayerExpManager.PlayerData == null)
            PlayerExpManager.InitPlayerData();         // 혹시 파일이 없었을 때 대비

        // 2) 서스펜드(런) 데이터 적용
        if (SuspendedData.Exists())
        {
            var data = SuspendedData.Load();
            if (data != null)
            {
                var pd = PlayerExpManager.PlayerData;
                pd.playerLevel = data.playerLevel;
                pd.playerExp = data.playerExp;
                pd.playerTotalExp = data.totalExp;
                pd.expToNextLevel = data.expToNextLevel;

                PlayerHealth.currentHP = Mathf.Max(1, data.currentHP);
                SuspendedData.Delete();                // 일회성 세이브 삭제
            }
        }

        // 3) 버프 재적용 (컴포넌트들이 깨어난 다음에)
        BootstrapRunner.RunAfterOneFrame(ReapplySoulBuffsOnce);
    }

    private static void ReapplySoulBuffsOnce()
    {
        // 저장된 해금 버튼 기준으로 버프 전체 재적용
        SoulBuffManager.ApplyAllUnlockedBuffs();
    }
}



