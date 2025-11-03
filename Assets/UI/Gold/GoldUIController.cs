//using UnityEngine;
//using UnityEngine.UIElements;

//public class GoldUIController : MonoBehaviour
//{
//    private Label goldLabel;

//    private void Start()
//    {
//        Debug.Log("[UI] GoldUIController 활성화됨");
//        var root = GetComponent<UIDocument>().rootVisualElement;

//        goldLabel = root.Q<Label>("GoldText");

//        if (goldLabel == null)
//        {
//            Debug.LogError("GoldText 라벨을 찾을 수 없습니다.");
//            return;
//        }

//        // 골드 변경 시 UI 갱신
//        PlayerGoldManager.Instance.OnGoldChanged += UpdateGoldText;
//        UpdateGoldText(PlayerGoldManager.Instance.Gold);
//    }

//    private void OnDisable()
//    {
//        PlayerGoldManager.Instance.OnGoldChanged -= UpdateGoldText;
//    }

//    private void UpdateGoldText(int gold)
//    {
//        Debug.Log($"[UI] 골드 UI 업데이트: {gold}");

//        if (goldLabel != null)
//            goldLabel.text = $"{gold}G";
//        else
//            Debug.LogError("GoldText 라벨을 찾을 수 없습니다");
//    }

//}
