using UnityEngine;

public class InventoryHotkey : MonoBehaviour
{
    [SerializeField] private InventoryBackPanelAdapter inventoryAdapter;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            if (inventoryAdapter.IsOpen) inventoryAdapter.Hide();
            else inventoryAdapter.Show(); // 내부에서 inv.OpenInventory() 호출됨
        }
    }
}