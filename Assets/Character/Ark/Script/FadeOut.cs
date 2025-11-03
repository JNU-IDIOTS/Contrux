using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class FadeOut : MonoBehaviour
{
    [Tooltip("잔상이 완전히 사라질 때까지 걸리는 시간(초)")]
    public float fadeDuration = 0.3f;

    [Tooltip("0초일 때(0)와 fadeDuration일 때(1)에 대한 알파 곡선")]
    public AnimationCurve alphaCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    private SpriteRenderer sr;
    private Color startColor;
    private float timer;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        startColor = sr.color;
    }

    void Update()
    {
        timer += Time.deltaTime;
        float t = Mathf.Clamp01(timer / fadeDuration);
        float curveA = alphaCurve.Evaluate(t);
        sr.color = new Color(startColor.r, startColor.g, startColor.b, startColor.a * curveA);

        if (timer >= fadeDuration)
            Destroy(gameObject);
    }
}
