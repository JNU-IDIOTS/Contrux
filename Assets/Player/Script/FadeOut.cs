using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class FadeOut : MonoBehaviour
{
    [Tooltip("잔상이 사라지는 데 걸리는 총 시간(초)")]
    public float fadeDuration = 0.3f;

    [Tooltip("시간(0)이 지날수록 알파값(1)이 0이 되도록 설정된 애니메이션 커브")]
    public AnimationCurve alphaCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    private SpriteRenderer _sr; // 🚨 컴포넌트 참조 변수 규칙 적용
    private Color startColor;   // 🚨 잔상의 최종 초기 색상 (투명도 포함)
    private float timer;        // 🚨 변수명 규칙 유지

    private void Awake()
    {
        // 1. Awake에서 SpriteRenderer 참조
        _sr = GetComponent<SpriteRenderer>();

        // 2. 혹시라도 Awake가 실행되기 전에 SetInitialAlpha가 호출될 경우를 대비해 
        //    Awake에서 _sr 할당을 제거하고 SetInitialAlpha에서 할당하는 것도 고려해볼 수 있습니다. 
        //    하지만 RequireComponent가 있으므로 Awake에서 하는 것이 일반적입니다.
    }

    /// <summary>
    /// 외부(DashSkill)에서 잔상의 초기 투명도를 설정합니다.
    /// </summary>
    /// <param name="alphaMultiplier">0.0(완전 투명)에서 1.0(완전 불투명) 사이의 초기 투명도 값</param>
    public void SetInitialAlpha(float alphaMultiplier)
    {
        // ⚠️ 안전성 추가: 혹시 Awake에서 초기화에 실패했거나, 
        //    프리팹 생성/활성화 순서 문제로 _sr이 null일 수 있다면 여기서 다시 가져옵니다.
        if (_sr == null)
        {
            _sr = GetComponent<SpriteRenderer>();
            if (_sr == null)
            {
                Debug.LogError("SpriteRenderer를 찾을 수 없어 페이드 아웃을 시작할 수 없습니다.");
                Destroy(gameObject);
                return;
            }
        }

        // SpriteRenderer의 현재 색상(잔상 생성 시점의 색상)을 가져와 투명도만 alphaMultiplier로 설정
        // 💡 중요: 잔상 생성 시점에 _sr.color가 올바른 색상을 가지고 있어야 합니다.
        Color initialColor = _sr.color;
        initialColor.a *= alphaMultiplier; // 잔상이 생성된 순간의 투명도를 설정

        startColor = initialColor;
        _sr.color = initialColor;   // 잔상의 투명도를 즉시 반영
    }

    private void Update()
    {
        if (_sr == null) return; // _sr이 유효하지 않으면 Update를 중단

        timer += Time.deltaTime;
        float timeNormalized = Mathf.Clamp01(timer / fadeDuration);
        float curveAlpha = alphaCurve.Evaluate(timeNormalized);

        // startColor.a(초기 투명도)에 curveAlpha를 곱하여 시간이 지남에 따라 0으로 페이드 아웃
        _sr.color = new Color(startColor.r, startColor.g, startColor.b, startColor.a * curveAlpha);

        if (timer >= fadeDuration)
            Destroy(gameObject);
    }
}