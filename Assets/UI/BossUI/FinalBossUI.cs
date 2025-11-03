using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class FinalBossUI : MonoBehaviour
{
    [SerializeField] FinalBossUI FinalbossUI;
    [SerializeField] private GameObject BossUIController;
    [SerializeField] private GameObject BossObjects;
    [SerializeField] private Boss1_FSM Boss1_fsm;
    private float BossMaxHP;
    public Image BossHPfillImage;
    public TextMeshProUGUI BossNameText;
    public TextMeshProUGUI BossHPText;
    public Image GrogyBarFillImage;
    public float grogyGaugeMax;
    private bool prevGroggyState = false;

    void Start()
    {
        if (BossUIController != null)
        {
            BossUIController.SetActive(false);
        }
        BossMaxHP = hp.GetMaxHPByTag(gameObject.tag);
        string objectTag = BossObjects.tag;
        BossNameText.text = $"바르곤 골드 비어드";
        grogyGaugeMax = Boss1_fsm.groggyGauge;

    }

    void Update()
    {
        if (BossObjects != null && BossObjects.activeInHierarchy)
        {
            BossUIController.SetActive(true);
            UpdateBossUI();

            if (prevGroggyState && !Boss1_fsm.isGroggy)
            {
                GrogyBarFillImage.fillAmount = 0f;
            }

            prevGroggyState = Boss1_fsm.isGroggy;
        }
        else
        {
            BossUIController.SetActive(false);
        }
        if (!Boss1_fsm.isGroggy)
        {
            GrogyBarFillImage.fillAmount = Mathf.Clamp01(Boss1_fsm.damageAccumulator / grogyGaugeMax);
        }
        else
        {
            // 그로기 상태일 때는 항상 바를 꽉 채움
            GrogyBarFillImage.fillAmount = 1f;
        }
    }

    public void UpdateBossUI()
    {
        if (BossUIController.activeInHierarchy)
        {
            Debug.Log($"[UI] 체력 표시: {Boss1_FSM.currentHP} / {BossMaxHP}");
            BossHPfillImage.fillAmount = Boss1_FSM.currentHP / BossMaxHP;
        }
    }

}
