using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DDOLSceneLoader : MonoBehaviour
{
    public static DDOLSceneLoader I { get; private set; }

    [Header("Refs")]
    [SerializeField] private LoadingUI loadingUI;   // LoadingUIRoot의 컴포넌트
    [SerializeField] private CanvasGroup fader;     // FaderPanel의 CanvasGroup

    [Header("Timing")]
    [SerializeField] private float minDisplaySeconds = 2f;
    [SerializeField] private float fadeDuration = 0.25f;

    [Header("Debug")]
    [SerializeField] private bool autoTestOnStartup = true;
    [SerializeField] private string testSceneName = ""; // F10 로드용

    [SerializeField] LoadingTipRotator tipRotator;
    public bool IsBusy { get; private set; }

    void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);

        if (!loadingUI) loadingUI = FindObjectOfType<LoadingUI>(true);
        if (!fader) fader = GameObject.Find("FaderPanel")?.GetComponent<CanvasGroup>();

        if (!loadingUI || !fader)
        {
            Debug.LogError("[Loader] Missing refs. loadingUI or fader not found.");
        }

        if (fader)
        {
            fader.alpha = 0f;
            fader.blocksRaycasts = false; // 투명일 땐 클릭 통과
        }
        if (loadingUI) loadingUI.Hide();
    }

    void Start()
    {
        //if (autoTestOnStartup) StartCoroutine(DebugShow2s());
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F9)) StartCoroutine(DebugShow2s());
        if (Input.GetKeyDown(KeyCode.F10) && !string.IsNullOrEmpty(testSceneName))
            LoadLevel(testSceneName);
    }

    public void LoadLevel(string nextScene)
    {
        if (IsBusy) { Debug.Log("[Loader] Busy, ignored."); return; }
        Debug.Log($"[Loader] LoadLevel({nextScene})");
        StartCoroutine(LoadRoutine(nextScene));
    }

    IEnumerator LoadRoutine(string sceneName)
    {
        IsBusy = true;
        LockInput(true);

        // 1) 로딩 UI 먼저 렌더
        loadingUI.Show();
        loadingUI.SetProgress(0f);
        loadingUI.SetSubtext("Preparing...");
        tipRotator?.Begin(sceneName);
        Canvas.ForceUpdateCanvases();
        yield return null; // 화면에 실제로 그려지게

        // 2) 페이드아웃(검정막만)
        yield return Fade(1f);

        // 3) 표시 시작 시각 고정
        float showStart = Time.realtimeSinceStartup;

        // 4) 비동기 로드
        var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        op.allowSceneActivation = false;

        while (true)
        {
            float progress01 = Mathf.Clamp01(op.progress / 0.9f); // 0~0.9 ⇒ 0~1
            float visual = Mathf.Lerp(loadingUI.Progress, progress01 * 0.9f, 0.25f);
            loadingUI.SetProgress(visual);

            if (op.progress < 0.9f) loadingUI.SetSubtext("Loading...");
            else loadingUI.SetSubtext("Almost there...");

            bool loadReady = op.progress >= 0.9f;
            bool minTime = (Time.realtimeSinceStartup - showStart) >= minDisplaySeconds;

            if (loadReady && minTime) break;
            yield return null;
        }

        // 5) 활성화
        loadingUI.SetSubtext("Activating...");
        loadingUI.SetProgress(0.98f);
        op.allowSceneActivation = true;
        while (!op.isDone) yield return null;

        // 6) 후처리
        yield return DoPostActivateSetup();

        // 7) 마무리
        loadingUI.SetProgress(1f);
        yield return Fade(0f);
        loadingUI.Hide();
        tipRotator?.End();
        LockInput(false);
        IsBusy = false;
        Debug.Log("[Loader] Done");
    }

    IEnumerator DoPostActivateSetup()
    {
        yield return null; // 씬 오브젝트 살아난 뒤 1프레임
        // PlayerSpawner.SpawnAtEntrance();
        // RunSaveSystem.InjectToPlayer();
        // CameraRoomBinder.BindCurrentScene();
    }

    IEnumerator Fade(float target)
    {
        if (!fader) yield break;
        fader.gameObject.SetActive(true);
        fader.blocksRaycasts = target > 0f; // 까매질 땐 클릭 차단, 투명해지면 허용

        float start = fader.alpha;
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            fader.alpha = Mathf.Lerp(start, target, t / fadeDuration);
            yield return null;
        }
        fader.alpha = target;

        if (target <= 0f) fader.blocksRaycasts = false;
    }

    IEnumerator DebugShow2s()
    {
        Debug.Log("[Loader] DebugShow2s start");
        loadingUI.Show();
        Canvas.ForceUpdateCanvases();
        yield return null;
        yield return Fade(1f);
        yield return new WaitForSecondsRealtime(2f);
        yield return Fade(0f);
        loadingUI.Hide();
        Debug.Log("[Loader] DebugShow2s end");
    }

    void LockInput(bool on)
    {
        // PlayerInput.enabled = !on; 같은 프로젝트 로직 추가
    }
}
