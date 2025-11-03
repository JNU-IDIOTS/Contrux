using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class BossUI : MonoBehaviour 
{
    [SerializeField] BossUI bossUI;
    [SerializeField] private GameObject BossUIController;
    [SerializeField] private GameObject BossObjects;
    private float BossMaxHP;
    public Image BossHPfillImage;
    public TextMeshProUGUI BossNameText;
    public TextMeshProUGUI BossHPText;

    void Start()
    {
        if(BossUIController != null)
        {
            BossUIController.SetActive(false);
        }
        BossMaxHP = hp.GetMaxHPByTag(gameObject.tag);
        string objectTag = BossObjects.tag;
        BossNameText.text = $"{objectTag}";
    }

    void Update()
    {
        if (BossObjects != null && BossObjects.activeInHierarchy)
        {
            BossUIController.SetActive(true);
            UpdateBossUI();
            
        }
        else
        {
            BossUIController.SetActive(false);
        }

    }

    public void UpdateBossUI()
    {
        if(BossUIController.activeInHierarchy)
        {
            Debug.Log($"[UI] 체력 표시: {SandWorm_FSM.currentHP} / {BossMaxHP}");
            BossHPfillImage.fillAmount = SandWorm_FSM.currentHP / BossMaxHP;
            Debug.Log($"[fillImage] fillAmount 설정됨: {BossHPfillImage.fillAmount}");
        }
    }

}
