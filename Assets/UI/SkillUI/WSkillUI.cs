using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class WSkillUI : MonoBehaviour
{
    [SerializeField] private Image WcooldownMaskImage;
    [SerializeField] private TextMeshProUGUI cooldownText;
    public float WCooldown = 5;
    private float cooldownTimer = 0f;
    public bool isCoolingDown = false;


    void Start()
    {

        WcooldownMaskImage.gameObject.SetActive(false);
        cooldownText.gameObject.SetActive(false);
    }
           
    void Update()
    {
        if (isCoolingDown)
        {
            WcooldownMaskImage.gameObject.SetActive(true);
            cooldownTimer -= Time.deltaTime;
            float ratio = Mathf.Clamp01(cooldownTimer / WCooldown);
            WcooldownMaskImage.fillAmount = ratio;

            // 텍스트 갱신
            cooldownText.text = Mathf.CeilToInt(cooldownTimer).ToString();
            cooldownText.gameObject.SetActive(true);
            cooldownText.color = Color.white; // 혹시 투명일까봐 강제로 설정

            if (cooldownTimer <= 0f)
            {
                isCoolingDown = false;
                WcooldownMaskImage.fillAmount = 0f;
                cooldownText.gameObject.SetActive(false);  // 텍스트 안보이게
            }
        }

    }

    public void StartWCooldown()
    {
        isCoolingDown = true;
        cooldownTimer = WCooldown;
        WcooldownMaskImage.fillAmount = 1f;

    }
}
