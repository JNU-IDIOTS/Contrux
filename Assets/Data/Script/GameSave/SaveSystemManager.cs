using UnityEngine;

public static class SaveSystemManager
{

    // 씬 이동/일시중지 등: 강제 종료 대비해서 모두 저장
    public static void SaveOnSceneTransition()
    {
        // 런(일회성) 저장
        SuspendedRunManager.SaveCurrentRun();

        // 메타(소울/버프/해금 등) 저장
        PlayerExpManager.SavePlayerData();

        // 그 외(스탯/설정 등 프로젝트 종합 저장)
        SaveManager.Instance.SaveNow();

        // 인벤토리 등 별도 시스템
        InventoryUIController.Instance.SaveInventory();
    }

    // 사용자가 옵션창에서 나가기/메인메뉴로 돌아가기/Alt+F4 대비
    public static void SaveOnExitLike()
    {
        SuspendedRunManager.SaveCurrentRun();
        PlayerExpManager.SavePlayerData();
        if (SaveManager.Instance != null) SaveManager.Instance.SaveNow();
        if (InventoryUIController.Instance != null) InventoryUIController.Instance.SaveInventory();
    }

    // 사망 시: 런 폐기(중간세이브 저장 금지), 메타만 저장
    public static void SaveOnDeath()
    {
        // 절대 SaveCurrentRun() 호출하지 않기
        PlayerExpManager.SavePlayerData();
        if (SaveManager.Instance != null) SaveManager.Instance.SaveNow();
        if (InventoryUIController.Instance != null) InventoryUIController.Instance.SaveInventory();
    }

    // 게임 시작 시: Resume 있으면 반영, 없으면 Init
    // 게임 시작: 메타 → 서스펜드 → 다음 프레임 버프 재적용
    public static void ResumeOrInitAtStartup()
    {
        // 소울/버프(메타) 먼저 로드
        PlayerExpSave.LoadExp();
        if (PlayerExpManager.PlayerData == null) PlayerExpManager.InitPlayerData();

        // 런(서스펜드) 오버레이
        SuspendedRunManager.ResumeIfExists(); // 내부에서 파일 있으면 반영/삭제

        // 다음 프레임에 소울 버프 일괄 재적용
        BootstrapRunner.RunAfterOneFrame(SoulBuffManager.ApplyAllUnlockedBuffs);
    }
}
