using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class OverHeatUI : MonoBehaviour 
{

    [SerializeField] private Image OverHeatcooldownMaskImage;
    [SerializeField] private TextMeshProUGUI cooldownText;
    public float OverHeatCooldown = 7;
    private float cooldownTimer = 0f;
    public bool isCoolingDown = false;

    void Start()
    {

        OverHeatcooldownMaskImage.gameObject.SetActive(false);
        cooldownText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (isCoolingDown)
        {
            OverHeatcooldownMaskImage.gameObject.SetActive(true);
            cooldownTimer -= Time.deltaTime;
            float ratio = Mathf.Clamp01(cooldownTimer / OverHeatCooldown);
            OverHeatcooldownMaskImage.fillAmount = ratio;

            // 텍스트 갱신
            cooldownText.text = Mathf.CeilToInt(cooldownTimer).ToString();
            cooldownText.gameObject.SetActive(true);
            cooldownText.color = Color.white; // 혹시 투명일까봐 강제로 설정

            if (cooldownTimer <= 0f)
            {
                isCoolingDown = false;
                OverHeatcooldownMaskImage.fillAmount = 0f;
                cooldownText.gameObject.SetActive(false);  // 텍스트 안보이게
            }
        }
    }

    public void StartOverHeatCooldown()
    {
        isCoolingDown = true;
        cooldownTimer = OverHeatCooldown;
        OverHeatcooldownMaskImage.fillAmount = 1f;

    }
}
