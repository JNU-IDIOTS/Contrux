using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// -100보다 작은 값일수록 더 먼저 실행됩니다.
[DefaultExecutionOrder(15)]
public class ChooseOne : MonoBehaviour
{
    [Header("UI 패널 & 버튼 참조")]
    public GameObject selectionPanel;    // 선택창 전체 패널 (Canvas 아래에 두고 비활성화)
    public Button steampunkButton;       // 스팀펑크 선택 버튼
    public Button runeButton;            // 룬마력 선택 버튼

    [Header("스팀펑크/룬마력 플레이어")]
    [SerializeField] private GameObject SteamPunkPlayer;
    [SerializeField] private GameObject MagicPlayer;

    // UIcontoller 연결
    public GameObject UI;
    [SerializeField] private InventoryUIController inventory;

    public bool select = false;
    public bool SystemSteamPunk = false;
    public bool SystemMagic = false;

    // 일시정지 전 물리 업데이트 주기를 저장할 변수
    private float _savedFixedDeltaTime;


    private SystemSwitch systemswitch;
    [SerializeField] private DashStackUI dashstackUI;

    // 참조
    [SerializeField] private CameraController cameracontroller;
    [SerializeField] private PlayerGoldManager playergoldmanager;
    //[SerializeField] private EnemyAI enemy;

    private void Awake()
    {
        systemswitch = GetComponent<SystemSwitch>();
        // 게임 완전 정지
        PauseGame();

        // 선택 UI 띄우기
        if (selectionPanel != null)
            selectionPanel.SetActive(true);

        // 버튼 콜백 연결
        steampunkButton.onClick.AddListener(OnSteampunkSelected);
        runeButton.onClick.AddListener(OnRuneSelected);
    }

    private void OnSteampunkSelected()
    {
        GameSettings.Instance.CurrentMode = GameMode.Steampunk;
        StartGame();
        SteamPunkPlayer.SetActive(true);
        SystemSteamPunk = true;
        select = true;
        systemswitch.SteamPunk();
        dashstackUI.SteamPunkDash();
        cameracontroller.SetSteamPunktype();
        UI.SetActive(true);
        inventory.OpenInventory();
        //playergoldmanager.Init();
    }

    private void OnRuneSelected()
    {
        GameSettings.Instance.CurrentMode = GameMode.Rune;
        StartGame();
        MagicPlayer.SetActive(true);
        SystemMagic = true;
        select = true;
        systemswitch.Magic();
        dashstackUI.MagicDash();
        cameracontroller.SetMagictype();
        UI.SetActive(true);
        inventory.OpenInventory();
        //playergoldmanager.Init();
    }

    private void StartGame()
    {
        // UI 숨기기
        selectionPanel.SetActive(false);

        // 게임 재개
        ResumeGame();

        // 씬 로드나 게임 매니저 시작 호출
        // SceneManager.LoadScene("MainGame");
        // GameManager.Instance.Begin();
    }

    private void PauseGame()
    {
        // 1) 물리 프레임 저장 후 0으로
        _savedFixedDeltaTime = Time.fixedDeltaTime;
        Time.fixedDeltaTime = 0f;

        // 2) 게임 시간 정지
        Time.timeScale = 0f;

        // 3) 오디오도 정지
        AudioListener.pause = true;
    }

    private void ResumeGame()
    {
        // 1) 게임 시간 복구
        Time.timeScale = 1f;

        // 2) 물리 프레임 복구
        Time.fixedDeltaTime = _savedFixedDeltaTime;

        // 3) 오디오 재생 재개
        AudioListener.pause = false;
    }
}