using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    public PlayerHealth player;
    public Image hpFillImage;
    public Image guardFillImage;

    void Update()
    {
        if (player == null) return;

        // 체력 fillAmount 설정
        hpFillImage.fillAmount = Mathf.Clamp01((float)player.CurrentHP / player.MaxHP);

        // 가드 fillAmount 설정
        guardFillImage.fillAmount = Mathf.Clamp01(player.GuardRatio);
    }
}
