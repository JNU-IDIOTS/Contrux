using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class QSkillUI : MonoBehaviour
{
    [SerializeField] private Image QcooldownMaskImage;
    [SerializeField] private TextMeshProUGUI cooldownText;
    public float QCooldown = 5;
    private float cooldownTimer = 0f;
    public bool isCoolingDown = false;

    void Start()
    {
        QcooldownMaskImage.gameObject.SetActive(false);
        cooldownText.gameObject.SetActive(false);
    }
           
    void Update()
    {
        if (isCoolingDown)
        {
            QcooldownMaskImage.gameObject.SetActive(true);
            cooldownTimer -= Time.deltaTime;
            float ratio = Mathf.Clamp01(cooldownTimer / QCooldown);
            QcooldownMaskImage.fillAmount = ratio;

            // 텍스트 갱신
            cooldownText.text = Mathf.CeilToInt(cooldownTimer).ToString();
            cooldownText.gameObject.SetActive(true);
            cooldownText.color = Color.white; // 혹시 투명일까봐 강제로 설정

            if (cooldownTimer <= 0f)
            {
                isCoolingDown = false;
                QcooldownMaskImage.fillAmount = 0f;
                cooldownText.gameObject.SetActive(false);  // 텍스트 안보이게
            }
        }
    }

    public void StartQCooldown()
    {
        isCoolingDown = true;
        cooldownTimer = QCooldown;
        QcooldownMaskImage.fillAmount = 1f;

    }
}
