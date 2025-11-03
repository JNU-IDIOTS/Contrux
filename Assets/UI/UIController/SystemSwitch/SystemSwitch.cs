using UnityEngine;

public class SystemSwitch : MonoBehaviour
{
    [SerializeField] private GameObject steamPressureUI;
    [SerializeField] private GameObject manaUI;
    [SerializeField] private GameObject QSkill;
    [SerializeField] private GameObject WSkill;
    [SerializeField] private GameObject HookUI;
    [SerializeField] private GameObject OverHeatUI;

    public bool Steampunk = false;
    public bool ManaSystem = false;
    public HookUI hookUI;

    private void Awake()
    {
        hookUI = GetComponent<HookUI>();
    }

    void Start()
    {
        // ▶ 게임 시작 시 UI 모두 숨기기
        steamPressureUI.SetActive(false);
        manaUI.SetActive(false);
        HookUI.SetActive(false);
        OverHeatUI.SetActive(false);
        QSkill.SetActive(false);
        WSkill.SetActive(false);

        // ▶ 선택된 시스템 있으면 해당 UI만 보이게
        switch (GameChoice.SelectedSystem)
        {
            case MagicSystemType.SteamPunk:
                steamPressureUI.SetActive(true);
                HookUI.SetActive(true);
                OverHeatUI.SetActive(true);
                break;
            case MagicSystemType.Mana:
                manaUI.SetActive(true);
                QSkill.SetActive(true);
                WSkill.SetActive(true);
                break;
        }
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            GameChoice.SelectedSystem = MagicSystemType.None;
            UpdateUI();
            Debug.Log("🔄 시스템 초기화 (선택 안 함)");
        }
    }

    private void UpdateUI()
    {
        steamPressureUI.SetActive(GameChoice.SelectedSystem == MagicSystemType.SteamPunk);
        HookUI.SetActive(GameChoice.SelectedSystem == MagicSystemType.SteamPunk);
        OverHeatUI.SetActive(GameChoice.SelectedSystem == MagicSystemType.SteamPunk);
        manaUI.SetActive(GameChoice.SelectedSystem == MagicSystemType.Mana);
        QSkill.SetActive(GameChoice.SelectedSystem == MagicSystemType.Mana);
        WSkill.SetActive(GameChoice.SelectedSystem == MagicSystemType.Mana);
    }

    public void SteamPunk()
    {
        GameChoice.SelectedSystem = MagicSystemType.SteamPunk;
        UpdateUI();
        Steampunk = true;
        ManaSystem = false;
        //hookUI.SteampunkCheck();
        //Debug.Log("스팀펑크 시스템 선택");
    }
    public void Magic()
    {
        GameChoice.SelectedSystem = MagicSystemType.Mana;
        UpdateUI();
        Steampunk = false;
        ManaSystem = true;
        //Debug.Log(" 마나 시스템 선택");
    }
}

public enum MagicSystemType { None, SteamPunk, Mana }

public static class GameChoice
{
    public static MagicSystemType SelectedSystem = MagicSystemType.None;
}



