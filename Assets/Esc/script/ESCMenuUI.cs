using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class ESCMenuUI : MonoBehaviour
{
    private UIDocument _uiDocument;
    private VisualElement _root;
    
    private VisualElement _overlay, _pausePanel, _settingsPanel, _controlsPanel;
    private VisualElement _resolutionList; 
    private Button _btnResDropdown;
    private Label _playTimeLabel;
    
    private List<VisualElement> _currentFocusList = new List<VisualElement>();
    private int _focusIndex = -1;
    private VisualElement _lastFocusedElement; 

    private Slider _sliderMaster, _sliderBGM, _sliderSFX;
    private Label _valMaster, _valBGM, _valSFX;
    private Toggle _toggleFullscreen, _toggleVSync, _toggleDmgTxt;
    private Button _btnLang;

    private Dictionary<string, Button> _keyButtons = new Dictionary<string, Button>();
    private string _rebindingKey = null;

    private bool _isOpen = false;
    private bool _isSubMenuOpen = false; 

    // 🎨 색상 정의
    private readonly Color _colActiveBg = new Color(0.25f, 0.23f, 0.3f); 
    private readonly Color _colActiveText = new Color(1f, 0.85f, 0.42f); 
    private readonly Color _colBorderActive = new Color(1f, 0.85f, 0.42f); 

    private void OnEnable()
    {
        _uiDocument = GetComponent<UIDocument>();
        if (_uiDocument == null) return;
        _root = _uiDocument.rootVisualElement;

        _overlay = _root.Q<VisualElement>("Overlay");
        _pausePanel = _root.Q<VisualElement>("PausePanel");
        _settingsPanel = _root.Q<VisualElement>("SettingsPanel");
        _controlsPanel = _root.Q<VisualElement>("ControlsPanel");
        _playTimeLabel = _root.Q<Label>("PlayTimeLabel");

        SetupButton("BtnResume", CloseMenu);
        SetupButton("BtnRestart", RestartGame);
        SetupButton("BtnControls", OpenControls);
        SetupButton("BtnSettings", OpenSettings);
        SetupButton("BtnExit", QuitGame);

        SetupButton("BtnBack", CloseSubMenu);
        SetupButton("BtnBackBottom", CloseSubMenu);
        SetupButton("BtnBackControls", CloseSubMenu);
        SetupButton("BtnCloseControls", CloseSubMenu);
        SetupButton("BtnCloseSettings", CloseSubMenu);

        SetupKeyButton("Key_Jump", "Jump");
        SetupKeyButton("Key_Attack", "Attack");
        SetupKeyButton("Key_Dash", "Dash");
        SetupKeyButton("Key_Hook", "Hook");
        SetupKeyButton("Key_Interact", "Interact");
        SetupKeyButton("Key_Skill1", "Skill1");
        SetupKeyButton("Key_Skill2", "Skill2");

        _sliderMaster = SetupSlider("SliderMaster", "ValMaster", v => AudioListener.volume = v);
        _sliderBGM = SetupSlider("SliderBGM", "ValBGM", v => { });
        _sliderSFX = SetupSlider("SliderSFX", "ValSFX", v => { });

        _resolutionList = _root.Q<VisualElement>("ResolutionList");
        _btnResDropdown = _root.Q<Button>("BtnResolutionDropdown");
        if (_btnResDropdown != null)
        {
            _btnResDropdown.RegisterCallback<ClickEvent>(e => 
            {
                bool isVisible = _resolutionList.style.display == DisplayStyle.Flex;
                _resolutionList.style.display = isVisible ? DisplayStyle.None : DisplayStyle.Flex;
            });
            _btnResDropdown.text = $"{Screen.width} x {Screen.height} ▼";
        }

        SetupResolutionItem("ResItem_1920", 1920, 1080);
        SetupResolutionItem("ResItem_1600", 1600, 900);
        SetupResolutionItem("ResItem_1280", 1280, 720);

        _toggleFullscreen = _root.Q<Toggle>("ToggleFullscreen");
        if (_toggleFullscreen != null)
        {
            _toggleFullscreen.value = Screen.fullScreen;
            _toggleFullscreen.RegisterValueChangedCallback(e => Screen.fullScreen = e.newValue);
        }

        _toggleVSync = _root.Q<Toggle>("ToggleVSync");
        if (_toggleVSync != null)
        {
            _toggleVSync.value = (QualitySettings.vSyncCount > 0);
            _toggleVSync.RegisterValueChangedCallback(e => QualitySettings.vSyncCount = e.newValue ? 1 : 0);
        }

        _btnLang = _root.Q<Button>("BtnLang");
        if (_btnLang != null) SetupButton("BtnLang", () => _btnLang.text = _btnLang.text == "한국어" ? "English" : "한국어");

        CloseMenu();
    }

    private void Update()
    {
        if (_playTimeLabel != null)
        {
            System.TimeSpan ts = System.TimeSpan.FromSeconds(Time.realtimeSinceStartup);
            _playTimeLabel.text = $"Play Time: {ts.Hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";
        }

        if (_rebindingKey != null) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (SoulStoreUI.Instance != null && SoulStoreUI.Instance.IsOpen) return;

            if (_isOpen)
            {
                if (_isSubMenuOpen) 
                    CloseSubMenu();
                else 
                    CloseMenu();
            }
            else OpenMenu();
        }

        if (!_isOpen) return;

        if (Input.GetKeyDown(KeyCode.DownArrow)) MoveFocus(1);
        if (Input.GetKeyDown(KeyCode.UpArrow)) MoveFocus(-1);
        
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (_focusIndex >= 0 && _focusIndex < _currentFocusList.Count)
            {
                if (_currentFocusList[_focusIndex] is Button btn)
                {
                    using (var e = new ClickEvent()) { e.target = btn; btn.SendEvent(e); }
                    PlayClickAnimation(btn);
                }
                else if (_currentFocusList[_focusIndex] is Toggle tog)
                {
                    tog.value = !tog.value;
                }
            }
        }
    }

    private void MoveFocus(int direction)
    {
        if (_currentFocusList.Count == 0) return;

        if (_focusIndex >= 0 && _focusIndex < _currentFocusList.Count)
            HighlightElement(_currentFocusList[_focusIndex], false);

        _focusIndex += direction;
        if (_focusIndex >= _currentFocusList.Count) _focusIndex = 0;
        if (_focusIndex < 0) _focusIndex = _currentFocusList.Count - 1;

        HighlightElement(_currentFocusList[_focusIndex], true);
    }

    // 🚀 [색상 복구 로직]
    private void HighlightElement(VisualElement elem, bool isOn)
    {
        if (elem == null) return;
        _lastFocusedElement = elem;

        elem.style.transitionDuration = new List<TimeValue> { new TimeValue(0.1f) };

        if (isOn)
        {
            elem.style.backgroundColor = _colActiveBg;
            elem.style.color = _colActiveText; 
            elem.style.borderTopColor = _colBorderActive;
            elem.style.borderBottomColor = _colBorderActive;
            elem.style.borderLeftColor = _colBorderActive;
            elem.style.borderRightColor = _colBorderActive;
            elem.style.scale = new Scale(new Vector3(1.02f, 1.02f, 1f));
        }
        else
        {
            // 원래 색상으로 복구 (StyleKeyword.Null 사용)
            elem.style.backgroundColor = StyleKeyword.Null;
            // 글자 색은 UXML 기본값(흰색/회색 등)을 따르도록 놔두거나 직접 지정
            elem.style.color = StyleKeyword.Null; 
            
            Color normalBorder = new Color(0,0,0,0); 
            elem.style.borderTopColor = normalBorder;
            elem.style.borderBottomColor = normalBorder;
            elem.style.borderLeftColor = normalBorder;
            elem.style.borderRightColor = normalBorder;
            elem.style.scale = new Scale(Vector3.one);
        }
    }

    private void PlayClickAnimation(VisualElement target)
    {
        target.style.scale = new Scale(new Vector3(0.95f, 0.95f, 1f));
        target.schedule.Execute(() => target.style.scale = new Scale(new Vector3(1.02f, 1.02f, 1f))).StartingIn(50);
    }

    private void SetupButton(string name, System.Action onClick)
    {
        var btn = _root.Q<Button>(name);
        if (btn != null)
        {
            btn.RegisterCallback<ClickEvent>(e => 
            {
                SetFocusToElement(btn);
                onClick?.Invoke();
                PlayClickAnimation(btn);
            });
            
            btn.RegisterCallback<MouseEnterEvent>(e => HighlightElement(btn, true));
            btn.RegisterCallback<MouseLeaveEvent>(e => HighlightElement(btn, false));
        }
    }

    private void SetupKeyButton(string uiName, string keyName)
    {
        var btn = _root.Q<Button>(uiName);
        if (btn != null)
        {
            _keyButtons[keyName] = btn;
            KeyCode current = (KeyCode)PlayerPrefs.GetInt("Key_" + keyName, (int)KeyCode.None);
            if(current != KeyCode.None) btn.text = $"{GetLabel(keyName)} : [ {current} ]";

            btn.RegisterCallback<ClickEvent>(e => 
            {
                SetFocusToElement(btn);
                _rebindingKey = keyName;
                btn.text = $"{GetLabel(keyName)} : [ 입력... ]";
                btn.style.backgroundColor = new Color(0.8f, 0.2f, 0.2f);
            });
            
            btn.RegisterCallback<MouseEnterEvent>(e => HighlightElement(btn, true));
            btn.RegisterCallback<MouseLeaveEvent>(e => HighlightElement(btn, false));
        }
    }

    private void SetupResolutionItem(string btnName, int w, int h)
    {
        var btn = _root.Q<Button>(btnName);
        if (btn != null)
        {
            btn.RegisterCallback<MouseEnterEvent>(e => btn.style.backgroundColor = new Color(1f, 1f, 1f, 0.1f));
            btn.RegisterCallback<MouseLeaveEvent>(e => btn.style.backgroundColor = StyleKeyword.Null);
            
            btn.RegisterCallback<ClickEvent>(e => 
            {
                SetResolution(w, h);
                if (_btnResDropdown != null) _btnResDropdown.text = $"{w} x {h} ▼";
                if (_resolutionList != null) _resolutionList.style.display = DisplayStyle.None;
            });
        }
    }

    private Slider SetupSlider(string name, string labelName, System.Action<float> onValueChange)
    {
        var slider = _root.Q<Slider>(name);
        var label = _root.Q<Label>(labelName);
        if (slider != null)
        {
            slider.RegisterValueChangedCallback(e => 
            {
                if (label != null) label.text = $"{(int)(e.newValue * 100)}%";
                onValueChange?.Invoke(e.newValue);
            });
        }
        return slider;
    }

    private void SetFocusToElement(VisualElement target)
    {
        UpdateFocusListForCurrentPanel();
        if (_focusIndex >= 0 && _focusIndex < _currentFocusList.Count)
            HighlightElement(_currentFocusList[_focusIndex], false);

        int idx = _currentFocusList.IndexOf(target);
        if (idx != -1)
        {
            _focusIndex = idx;
            HighlightElement(target, true);
        }
    }

    private void UpdateFocusListForCurrentPanel()
    {
        _currentFocusList.Clear();
        VisualElement activePanel = null;

        if (_pausePanel.style.display == DisplayStyle.Flex) activePanel = _pausePanel;
        else if (_settingsPanel.style.display == DisplayStyle.Flex) activePanel = _settingsPanel;
        else if (_controlsPanel.style.display == DisplayStyle.Flex) activePanel = _controlsPanel;

        if (activePanel != null)
        {
            activePanel.Query<VisualElement>().Where(e => e is Button || e is Toggle || e is Slider).ForEach(e => _currentFocusList.Add(e));
        }
    }

    private void OpenMenu() { _isOpen = true; _isSubMenuOpen = false; Time.timeScale = 0f; _root.style.display = DisplayStyle.Flex; _pausePanel.style.display = DisplayStyle.Flex; _settingsPanel.style.display = DisplayStyle.None; _controlsPanel.style.display = DisplayStyle.None; UpdateFocusListForCurrentPanel(); _focusIndex = 0; if(_currentFocusList.Count > 0) HighlightElement(_currentFocusList[0], true); UnityEngine.Cursor.visible = true; UnityEngine.Cursor.lockState = CursorLockMode.None; }
    private void CloseMenu() { _isOpen = false; Time.timeScale = 1f; _root.style.display = DisplayStyle.None; }
    private void OpenSettings() { _isSubMenuOpen = true; _pausePanel.style.display = DisplayStyle.None; _settingsPanel.style.display = DisplayStyle.Flex; UpdateFocusListForCurrentPanel(); _focusIndex = 0; if(_currentFocusList.Count > 0) HighlightElement(_currentFocusList[0], true); }
    private void OpenControls() { _isSubMenuOpen = true; _pausePanel.style.display = DisplayStyle.None; _controlsPanel.style.display = DisplayStyle.Flex; UpdateFocusListForCurrentPanel(); _focusIndex = 0; if(_currentFocusList.Count > 0) HighlightElement(_currentFocusList[0], true); }
    private void CloseSubMenu() { _isSubMenuOpen = false; _settingsPanel.style.display = DisplayStyle.None; _controlsPanel.style.display = DisplayStyle.None; _pausePanel.style.display = DisplayStyle.Flex; UpdateFocusListForCurrentPanel(); _focusIndex = 0; if(_currentFocusList.Count > 0) HighlightElement(_currentFocusList[0], true); }
    private void RestartGame() { Time.timeScale = 1f; if (ItemDataManager.Instance != null) ItemDataManager.Instance.ResetData(); SceneManager.LoadScene(SceneManager.GetActiveScene().name); }
    private void QuitGame() { 
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    private void SetResolution(int w, int h) { Screen.SetResolution(w, h, Screen.fullScreen); }

    private void OnGUI() { if (_rebindingKey != null) { Event e = Event.current; if (e.isKey && e.keyCode != KeyCode.None) ApplyKeyBinding(_rebindingKey, e.keyCode); } }
    private void ApplyKeyBinding(string keyName, KeyCode newKey) 
    {
        if (_keyButtons.ContainsKey(keyName)) {
            _keyButtons[keyName].text = $"{GetLabel(keyName)} : [ {newKey} ]";
            KeyManager.Instance.SetKey(keyName, newKey);
            HighlightElement(_keyButtons[keyName], true);
        }
        _rebindingKey = null;
    }
    private string GetLabel(string key) { switch(key) { case "Jump": return "점프"; case "Attack": return "공격"; case "Dash": return "대쉬"; case "Hook": return "갈고리"; case "Interact": return "상호작용"; case "Skill1": return "스킬 1"; case "Skill2": return "스킬 2"; default: return key; } }
}