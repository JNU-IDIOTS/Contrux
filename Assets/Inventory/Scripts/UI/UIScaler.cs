// Assets/Scripts/UI/UIScaler.cs

using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// UIDocument의 해상도를 1080p (1920x1080) 기준으로 스케일링합니다.
/// 이 스크립트를 UIDocument가 있는 오브젝트에 붙여주세요.
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class UIScaler : MonoBehaviour
{
    // 기준이 되는 해상도
    [Header("기준 해상도 설정")]
    public float referenceWidth = 1920f;
    public float referenceHeight = 1080f;
    
    [Header("캔버스 크기 강제 설정")]
    [Tooltip("체크하면 캔버스 크기를 기준 해상도로 강제합니다")]
    public bool forceCanvasSize = true;
    
    [Header("스케일링 옵션")]
    [Tooltip("체크하면 화면 크기에 맞춰 UI를 스케일링합니다")]
    public bool enableScaling = true;

    private UIDocument _uiDocument;
    private VisualElement _root;
    private float _currentScale = 1f;
    private int _currentScreenWidth = 0;
    private int _currentScreenHeight = 0;

    void Start()
    {
        _uiDocument = GetComponent<UIDocument>();
        if (_uiDocument != null)
        {
            _root = _uiDocument.rootVisualElement;
            _currentScreenWidth = Screen.width;
            _currentScreenHeight = Screen.height;
            ApplyCanvasSettings();
        }
    }

    void Update()
    {
        // 화면 해상도 변경 감지
        if (_currentScreenWidth != Screen.width || _currentScreenHeight != Screen.height)
        {
            _currentScreenWidth = Screen.width;
            _currentScreenHeight = Screen.height;
            ApplyCanvasSettings();
        }
    }

    /// <summary>
    /// 캔버스 크기 강제 설정 및 스케일링을 적용합니다.
    /// </summary>
    private void ApplyCanvasSettings()
    {
        if (_root == null) return;

        // 1. 캔버스 크기 강제 설정
        if (forceCanvasSize)
        {
            ForceCanvasSize();
        }

        // 2. 스케일링 적용
        if (enableScaling)
        {
            ApplyScale();
        }
    }

    /// <summary>
    /// 캔버스 크기를 기준 해상도(1920x1080)로 강제합니다.
    /// </summary>
    private void ForceCanvasSize()
    {
        // UI Document의 Panel Settings를 통해 고정 해상도 설정
        var panelSettings = _uiDocument.panelSettings;
        if (panelSettings != null)
        {
            panelSettings.scaleMode = PanelScaleMode.ConstantPixelSize;
            panelSettings.referenceResolution = new Vector2Int((int)referenceWidth, (int)referenceHeight);
            panelSettings.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
            panelSettings.match = 0.5f; // 가로세로 비율 균형
        }

        // Root Element의 크기를 직접 설정
        _root.style.width = referenceWidth;
        _root.style.height = referenceHeight;
        
        Debug.Log($"[UIScaler] 캔버스 크기 강제 설정: {referenceWidth}x{referenceHeight}");
    }

    /// <summary>
    /// 기준 해상도 대비 현재 화면 크기를 기준으로 UI 전체 스케일을 적용합니다.
    /// </summary>
    private void ApplyScale()
    {
        if (_root == null) return;

        // 가로세로 비율을 고려한 스케일 계산
        float widthScale = (float)Screen.width / referenceWidth;
        float heightScale = (float)Screen.height / referenceHeight;
        
        // 더 작은 스케일을 사용해서 UI가 화면을 벗어나지 않도록 함
        _currentScale = Mathf.Min(widthScale, heightScale);
        
        // UI Root의 scale 속성을 변경
        _root.style.scale = new StyleScale(new Vector3(_currentScale, _currentScale, 1f));
        
        // 스케일 중심을 중앙으로 설정
        _root.style.transformOrigin = new StyleTransformOrigin(
            new TransformOrigin(Length.Percent(50), Length.Percent(50))
        );

        Debug.Log($"[UIScaler] UI 스케일 적용: {_currentScale} (화면: {Screen.width}x{Screen.height}, 기준: {referenceWidth}x{referenceHeight})");
    }

    /// <summary>
    /// 런타임에서 기준 해상도를 변경합니다.
    /// </summary>
    public void SetReferenceResolution(float width, float height)
    {
        referenceWidth = width;
        referenceHeight = height;
        ApplyCanvasSettings();
    }

    /// <summary>
    /// 캔버스 크기 강제 설정을 토글합니다.
    /// </summary>
    public void ToggleForceCanvasSize(bool enable)
    {
        forceCanvasSize = enable;
        ApplyCanvasSettings();
    }
}