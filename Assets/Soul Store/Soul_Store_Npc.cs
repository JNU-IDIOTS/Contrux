using UnityEngine;

public class Soul_Store_Npc : MonoBehaviour
{
    [SerializeField] private SoulStoreManager soulStoreManager;
    [SerializeField] private Transform steamPlayer;
    [SerializeField] private Transform magicPlayer;
    [SerializeField] private ChooseOne chooseOne;

    [SerializeField] private float interactDistance = 4f;

    void Update()
    {
        // 1) 현재 컨트롤 중인 플레이어 위치
        Vector2 playerPos;
        if (chooseOne.SystemSteamPunk) playerPos = steamPlayer.position;
        else if (chooseOne.SystemMagic) playerPos = magicPlayer.position;
        else return;

        // 2) 범위 체크
        bool inRange = Vector2.Distance(playerPos, (Vector2)transform.position) < interactDistance;

        // 3) Z 키로 UI 토글
        if (inRange && Input.GetKeyDown(KeyCode.F) && soulStoreManager != null)
        {
            if (!soulStoreManager.IsOpen) soulStoreManager.Show();
            else soulStoreManager.Hide();
        }
    }
}
