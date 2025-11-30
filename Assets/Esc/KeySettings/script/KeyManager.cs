using UnityEngine;

public class KeyManager : MonoBehaviour
{
    public static KeyManager Instance;

    public KeyCode KeyUp = KeyCode.UpArrow;
    public KeyCode KeyDown = KeyCode.DownArrow;
    public KeyCode KeyLeft = KeyCode.LeftArrow;
    public KeyCode KeyRight = KeyCode.RightArrow;
    
    public KeyCode KeyJump = KeyCode.Space;
    public KeyCode KeyDash = KeyCode.LeftShift;
    public KeyCode KeyAttack = KeyCode.A;
    public KeyCode KeyHook = KeyCode.E;
    public KeyCode KeyInteract = KeyCode.F;

    // 🚀 [추가됨] 스킬 1, 2
    public KeyCode KeySkill1 = KeyCode.Q;
    public KeyCode KeySkill2 = KeyCode.R;

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }

        LoadKeys();
    }

    public void LoadKeys()
    {
        KeyUp = (KeyCode)PlayerPrefs.GetInt("Key_Up", (int)KeyCode.UpArrow);
        KeyDown = (KeyCode)PlayerPrefs.GetInt("Key_Down", (int)KeyCode.DownArrow);
        KeyLeft = (KeyCode)PlayerPrefs.GetInt("Key_Left", (int)KeyCode.LeftArrow);
        KeyRight = (KeyCode)PlayerPrefs.GetInt("Key_Right", (int)KeyCode.RightArrow);

        KeyJump = (KeyCode)PlayerPrefs.GetInt("Key_Jump", (int)KeyCode.Space);
        KeyDash = (KeyCode)PlayerPrefs.GetInt("Key_Dash", (int)KeyCode.LeftShift);
        KeyAttack = (KeyCode)PlayerPrefs.GetInt("Key_Attack", (int)KeyCode.A);
        KeyHook = (KeyCode)PlayerPrefs.GetInt("Key_Hook", (int)KeyCode.E);
        KeyInteract = (KeyCode)PlayerPrefs.GetInt("Key_Interact", (int)KeyCode.F);
        
        // 🚀 추가된 스킬 로드
        KeySkill1 = (KeyCode)PlayerPrefs.GetInt("Key_Skill1", (int)KeyCode.Q);
        KeySkill2 = (KeyCode)PlayerPrefs.GetInt("Key_Skill2", (int)KeyCode.R);
    }

    public void SetKey(string keyName, KeyCode newKey)
    {
        switch (keyName)
        {
            case "Jump": KeyJump = newKey; break;
            case "Dash": KeyDash = newKey; break;
            case "Attack": KeyAttack = newKey; break;
            case "Hook": KeyHook = newKey; break;
            case "Interact": KeyInteract = newKey; break;
            case "Skill1": KeySkill1 = newKey; break; // 추가
            case "Skill2": KeySkill2 = newKey; break; // 추가
        }

        PlayerPrefs.SetInt("Key_" + keyName, (int)newKey);
        PlayerPrefs.Save();
    }
}