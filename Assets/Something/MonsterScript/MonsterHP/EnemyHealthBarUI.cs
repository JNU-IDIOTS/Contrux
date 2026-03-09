using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// IHealth 인터페이스를 사용해 현재 체력과 최대 체력을 제공하는 대상 객체를 바인딩합니다.
/// </summary>
public class EnemyHealthBarUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image enemyHealthBar;
    [SerializeField] private TextMeshProUGUI enemyHealthBarText;

    private IHealth healthSource;
    private Transform target;

    [Header("Offset and Target")]
    [SerializeField] private Vector3 offset = new Vector3(0, 1.5f, 0);

    private RectTransform rectTransform;
    private Canvas canvas;
    private float maxHP;

    private void Awake()
    {
        // RectTransform과 Canvas를 미리 캐싱
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
    }

    /// <summary>
    /// IHealth 구현체, 타겟 Transform, 오프셋을 설정하고 초기 UI를 갱신합니다.
    /// </summary>
    public void SetTarget(IHealth hpSource, Transform targetTransform, Vector3 offset)
    {
        healthSource = hpSource;
        target = targetTransform;
        this.offset = offset;
        maxHP = hpSource.maxHP;

        // 초기 UI와 위치 갱신
        UpdateHealthUI();
        UpdatePositionAndRotation();
    }

    private void Update()
    {
        if (healthSource == null)
            return;

        float current = healthSource.currentHP;
        if (current <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        UpdateHealthUI();
    }

    private void LateUpdate()
    {
        // Null 체크: SetTarget 전에 호출되지 않도록 방어
        if (healthSource == null || target == null || canvas == null)
            return;

        if (canvas.renderMode == RenderMode.WorldSpace)
            UpdatePositionAndRotation();
    }

    private void UpdatePositionAndRotation()
    {
        rectTransform.position = target.position + offset;

        var cam = Camera.main;
        if (cam == null)
            return;

        Vector3 forward = cam.transform.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude > 0.001f)
            rectTransform.rotation = Quaternion.LookRotation(forward);
    }

    /// <summary>
    /// 체력바와 텍스트를 최신 값으로 갱신합니다.
    /// </summary>
    public void UpdateHealthUI()
    {
        if (healthSource == null || enemyHealthBar == null || enemyHealthBarText == null)
            return;

        float current = healthSource.currentHP;
        enemyHealthBar.fillAmount = current / maxHP;
        enemyHealthBarText.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(maxHP)}";
    }
}
