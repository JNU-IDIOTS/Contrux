using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;

public class ManaUIController : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private ManaSystem manaSystem;

    private VisualElement manaWave;
    private Label manaLabel;

    private Sprite[] manaFrames;
    private int currentFrame = 0;
    private float frameDelay = 0.1f;
    private float timer = 0f;

    [SerializeField] private float lerpSpeed = 5f;
    private float currentRatio = 1f;  // 현재 마나 비율 (0~1)

    private void OnEnable()
    {
        var root = uiDocument.rootVisualElement;
        manaWave = root.Q<VisualElement>("ManaWave");
        manaLabel = root.Q<Label>("ManaLabel");

        manaFrames = Resources.LoadAll<Sprite>("ManaUI")
            .Where(s => s.name.StartsWith("ManaWave"))
            .OrderBy(s => s.name)
            .ToArray();

        if (manaFrames == null || manaFrames.Length == 0)
        {
            Debug.LogError("❌ ManaWave 프레임 로드 실패");
            return;
        }

        SetFrame(0);
        UpdateManaUI();  // 초기화 시 즉시 반영
    }

    private void Update()
    {
        if (manaWave == null || manaFrames.Length == 0 || manaSystem == null) return;

        // 프레임 애니메이션
        timer += Time.deltaTime;
        if (timer >= frameDelay)
        {
            timer = 0f;
            currentFrame = (currentFrame + 1) % manaFrames.Length;
            SetFrame(currentFrame);
        }

        // 마나 UI 갱신
        UpdateManaUI();
    }

    private void UpdateManaUI()
    {
        float targetRatio = Mathf.Clamp01(manaSystem.CurrentMana / manaSystem.MaxMana);

        // 디버그 로그로 현재 상태 확인
        //Debug.Log($"[ManaUIController] 마나 갱신 요청: {manaSystem.CurrentMana} / {manaSystem.MaxMana} (비율: {targetRatio}, 보간 전: {currentRatio})");

        // scale.y만 보간 적용
        currentRatio = Mathf.Lerp(currentRatio, targetRatio, Time.deltaTime * lerpSpeed);

        // 디버그 로그로 보간 후 값 확인
        //Debug.Log($"[ManaUIController] 보간 적용 후 비율: {currentRatio}");

        manaWave.style.scale = new Scale(new Vector2(1f, currentRatio)); // ✅ 세로만 줄이기

        // 숫자 텍스트 갱신
        if (manaLabel != null)
            manaLabel.text = $"{(int)manaSystem.CurrentMana} / {(int)manaSystem.MaxMana}";
    }


    private void SetFrame(int index)
    {
        var sprite = manaFrames[index];
        if (!sprite.texture.isReadable) return;

        var tex = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
        tex.SetPixels(sprite.texture.GetPixels(
            (int)sprite.textureRect.x,
            (int)sprite.textureRect.y,
            (int)sprite.textureRect.width,
            (int)sprite.textureRect.height));
        tex.Apply();

        manaWave.style.backgroundImage = new StyleBackground(tex);
    }
}
