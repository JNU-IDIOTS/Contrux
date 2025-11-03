using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ExpUIController : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    [SerializeField] private TextMeshProUGUI xpText;
    [SerializeField] private TextMeshProUGUI playerLevelText;

    public float currentXP;
    public int maxXP;
    private int playerLevel;
    public int lastPlayerLevel = -1;
    void OnEnable()
    {
        PlayerExpManager.OnExpChanged += RefreshUI;
    }

    void OnDisable()
    {
        PlayerExpManager.OnExpChanged -= RefreshUI;
    }

    void Start()

    {
        LinkEXP();
        RefreshUI();
        if (currentXP == 0)
        {
            fillImage.gameObject.SetActive(false);
        }
        playerLevelText.gameObject.layer = 15;
    }


    public void RefreshUI()
    {
        Debug.Log("[UI] RefreshUI 호출됨"); // 이거 넣어봐!
        SetXP(
            PlayerExpManager.PlayerData.playerExp,
            PlayerExpManager.PlayerData.expToNextLevel,
            PlayerExpManager.PlayerData.playerLevel
        );
    }

    public void LinkEXP()
    {
        currentXP = PlayerExpManager.PlayerData.playerExp;
        maxXP = PlayerExpManager.PlayerData.expToNextLevel;
        playerLevel = PlayerExpManager.PlayerData.playerLevel;
    }

    public void SetXP(float current, int max, int level)
    {
        bool leveledUp = (level > lastPlayerLevel);
        lastPlayerLevel = level;

        xpText.text = $"EXP {current} / {max} ({(current / max * 100f):0}%)";
        playerLevelText.text = $" Lv. {level}";

        StopAllCoroutines(); // 이전 코루틴 중복 방지

        if (leveledUp)
            StartCoroutine(PlayLevelUpBarAnimation(current, max));
        else
            StartCoroutine(FillBarSmooth(current / max));
        fillImage.gameObject.SetActive(current > 0);
    }

    IEnumerator PlayLevelUpBarAnimation(float current, float max)
    {
        Debug.Log("▶ 레벨업 애니메이션 시작");

        // 1. 강제 1 채우기
        fillImage.fillAmount = 1f;

        // 2. 줄이기
        yield return StartCoroutine(FillBarSmooth(0f));

        // 3. 남은 경험치만큼 채우기
        float targetFill = current / max;
        Debug.Log($"▶ 남은 경험치 채우기: {targetFill}");
        yield return StartCoroutine(FillBarSmooth(targetFill));
    }


    IEnumerator FillBarSmooth(float targetFill)
    {
        float speed = 2.5f;
        while (Mathf.Abs(fillImage.fillAmount - targetFill) > 0.01f)
        {
            fillImage.fillAmount = Mathf.Lerp(fillImage.fillAmount, targetFill, Time.deltaTime * speed);
            yield return null;
        }
        fillImage.fillAmount = targetFill;
    }
}
