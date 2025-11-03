using UnityEngine;

public class InventoryOpener : MonoBehaviour
{
    [SerializeField] private InventoryBackPanelAdapter adapter;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            if (adapter == null) return;

            // UINavigator 정책에 맞춰 열기/닫기
            if (adapter.IsOpen)
            {
                adapter.Hide();
            }
            else
            {
                UINavigator.Instance?.CloseAllOverlaysExcept(adapter);
                adapter.Show(); // 내부에서 inv.OpenInventory() 호출
            }
        }
    }
}
