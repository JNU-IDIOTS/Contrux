using UnityEngine;
using UnityEngine.UIElements;

public class PlayerPortraitController : MonoBehaviour
{
    [SerializeField] private string texturePath = "Portraits/PlayerPortrait";

    void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        var portraitImage = root.Q<VisualElement>("PortraitImage");

        Texture2D texture = Resources.Load<Texture2D>(texturePath);
        if (texture != null)
        {
            portraitImage.style.backgroundImage = new StyleBackground(texture);
        }
        else
        {
            Debug.LogWarning("❌ 초상화 텍스처 로드 실패!");
        }
    }
}

