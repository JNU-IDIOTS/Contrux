using UnityEngine;
using UnityEngine.UIElements;

public class HealthBarUIController : MonoBehaviour
{
    private VisualElement fill;
    private Label hpLabel;
    private VisualElement rootElement;

    private MagicSystemType lastSystemType;

    private float currentHP;
    private float maxHP;

    void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        rootElement = root.Q<VisualElement>("HealthBarRoot");
        fill = root.Q<VisualElement>("HealthBarFill");
        hpLabel = root.Q<Label>("HealthBarLabel");

        // 임시 초기값
        maxHP = PlayerHealth.maxHP;
        currentHP = maxHP;

        SetHealthBarPosition(); // 선택된 시스템에 따라 위치 조정
        UpdateUI();
    }

    void Update()
    {
        if (GameChoice.SelectedSystem != lastSystemType)
        {
            SetHealthBarPosition();
            lastSystemType = GameChoice.SelectedSystem;
        }
    }
    public void SetShield(int current, int max)
    {
        PlayerHealth.RemainShield = current;
        PlayerHealth.PlayerShield = max;
        UpdateUI();
    }


    private void SetHealthBarPosition()
    {
        switch (GameChoice.SelectedSystem)
        {
            case MagicSystemType.None:
                rootElement.style.left = 190;
                rootElement.style.top = 910;
                break;

            case MagicSystemType.SteamPunk:
                rootElement.style.left = 280;
                rootElement.style.top = 910;
                break;

            case MagicSystemType.Mana:
                rootElement.style.left = 280;
                rootElement.style.top = 910;
                break;
        }
    }


    public void SetHP(float current, float max)
    {
        currentHP = current;
        maxHP = max;
        UpdateUI();
    }
    public void RefreshFromPlayerHealth()
    {
        currentHP = PlayerHealth.currentHP;
        maxHP = PlayerHealth.maxHP;
        UpdateUI();
    }
    private void UpdateUI()
    {
        float ratio = maxHP > 0 ? (float)currentHP / maxHP : 0f;
        fill.style.width = new Length(ratio * 100f, LengthUnit.Percent);
        hpLabel.text = $"{currentHP} / {maxHP}";
    }
}
