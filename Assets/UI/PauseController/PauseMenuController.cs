using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(UIDocument))]
public class PauseMenuController : BackPanelBase
{
    public override int BackPriority => 10; // 의미상 유지

    Button btnResume, btnNew, btnControls, btnSettings, btnQuit;

    protected override void Awake()
    {
        base.Awake();
        var r = doc.rootVisualElement;

        btnResume = r.Q<Button>("BtnResume");
        btnNew = r.Q<Button>("BtnNew");
        btnControls = r.Q<Button>("BtnControls");
        btnSettings = r.Q<Button>("BtnSettings");
        btnQuit = r.Q<Button>("BtnQuit");

        btnResume.clicked += () => { Hide(); PauseManager.Resume(); };
        btnNew.clicked += () => { PauseManager.Resume(); SceneManager.LoadScene(SceneManager.GetActiveScene().name); };
        btnControls.clicked += () => Debug.Log("TODO: 컨트롤 패널 열기");
        btnSettings.clicked += () => Debug.Log("TODO: 설정 패널 열기");
        btnQuit.clicked += OnQuit;

        // 시작은 숨김 + 일시정지 패널로 등록
        Hide();
        UINavigator.Instance.Register(this, isPauseMenu: true);
    }

    public override void Show()
    {
        base.Show();
        // 포커스 세팅
        btnResume?.Focus();
    }

    void OnQuit()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
