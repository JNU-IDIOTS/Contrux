using UnityEngine;

public class StatUIController : MonoBehaviour
{
    [SerializeField] private GameObject statPanel;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            statPanel.SetActive(!statPanel.activeSelf);
        }
    }
}
