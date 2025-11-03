using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoadingUI : MonoBehaviour
{
    [Header("Wiring")]
    [SerializeField] private Canvas rootCanvas;         // DDOL_LoadingCanvas
    [SerializeField] private Image barFill;             // type=Filled, Horizontal
    [SerializeField] private TextMeshProUGUI subLabel;  // optional
    [SerializeField] TextMeshProUGUI tipLabel;    // ← 팁 출력용

    [Header("Character")]
    [SerializeField] private GameObject runCharacter; // RunCharacter 루트
    [SerializeField] private Animator runAnimator;    // RunCharacter의 Animator

    [Header("Music")]
    [SerializeField] private AudioClip loadingMusic;      // 로딩 전용 BGM
    [Range(0f, 1f)][SerializeField] private float loadingMusicVolume = 0.8f;
    [SerializeField] private bool loadingMusicLoop = true;
    [SerializeField] private bool playMusicOnShow = true;
    public float Progress { get; private set; }
    const string MusicVolKey = "Music_Volume_01";

    void Reset()
    {
        rootCanvas = GetComponentInParent<Canvas>();
        if (!subLabel) subLabel = GetComponentInChildren<TextMeshProUGUI>(true);
        if (!barFill) barFill = GetComponentInChildren<Image>(true);
    }

    void Awake()
    {
        DontDestroyOnLoad(rootCanvas.gameObject);
        Hide();
        SetProgress(0f);
        SetSubtext("");
        SetTip("");
        if (runAnimator) runAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;

        if (PlayerPrefs.HasKey(MusicVolKey))
            MusicPlayer.Instance?.SetMusicLinearVolume(PlayerPrefs.GetFloat(MusicVolKey, 1f));
    }

    public void Show()
    {
        if (rootCanvas) rootCanvas.enabled = true;
        if (runCharacter) runCharacter.SetActive(true);
        if (runAnimator) { runAnimator.speed = 1f; runAnimator.Play("Run", 0, 0f); }

        if (playMusicOnShow && loadingMusic && MusicPlayer.Instance)
            MusicPlayer.Instance.Play(loadingMusic, loadingMusicLoop, loadingMusicVolume);
    }

    public void Hide()
    {
        if (runAnimator) runAnimator.speed = 0f;
        if (runCharacter) runCharacter.SetActive(false);
        if (rootCanvas) rootCanvas.enabled = false;
    }

    public void SetProgress(float v)
    {
        Progress = Mathf.Clamp01(v);
        if (barFill) barFill.fillAmount = Progress;
    }

    public void SetSubtext(string s)
    {
        if (subLabel) subLabel.text = s;
    }

    public void SetTip(string s)
    {
        if (tipLabel) tipLabel.text = s;
    }
    public void OnMusicVolumeChanged(float v01)
    {
        MusicPlayer.Instance?.SetMusicLinearVolume(v01);
        PlayerPrefs.SetFloat(MusicVolKey, Mathf.Clamp01(v01));
    }
}
