using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class DashCooldownUI : MonoBehaviour
{
    public float cooldownTime = 5f;  // 쿨타임 (초 단위)

    private float cooldownTimer = 0f;
    private bool isCoolingDown = false;
    [SerializeField] private Image cooldownMaskImage;
    [SerializeField] private TextMeshProUGUI cooldownText;
    void Start()
    {
        if (cooldownMaskImage == null)
            Debug.LogError("Cooldown mask image is not assigned.");
        cooldownMaskImage.gameObject.SetActive(false);
        cooldownText.gameObject.SetActive(false);  // 시작 시 안보이게
    }
    void Update()
    {
        if (isCoolingDown)
        {
            cooldownMaskImage.gameObject.SetActive(true);
            cooldownTimer -= Time.deltaTime;
            float ratio = Mathf.Clamp01(cooldownTimer / cooldownTime);
            cooldownMaskImage.fillAmount = ratio;

            // 텍스트 갱신
            cooldownText.text = Mathf.CeilToInt(cooldownTimer).ToString();
            cooldownText.gameObject.SetActive(true);
            cooldownText.color = Color.white; // 혹시 투명일까봐 강제로 설정

            if (cooldownTimer <= 0f)
            {
                isCoolingDown = false;
                cooldownMaskImage.fillAmount = 0f;
                cooldownText.gameObject.SetActive(false);  // 텍스트 안보이게
            }
        }

    }

    public void StartCooldown()
    { 
        isCoolingDown = true;
        cooldownTimer = cooldownTime;
        cooldownMaskImage.fillAmount = 1f;
       
    }
}
