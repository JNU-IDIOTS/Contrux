using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    private GameObject interactTarget;


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F) && interactTarget != null)
        {
            string tag = interactTarget.tag;

            switch (tag)
            {
                case "Weapon":
                    GroundItem groundItem = interactTarget.GetComponent<GroundItem>();
                    if (groundItem != null)
                    {
                        bool success = InventoryUIController.Instance.TryAddItem(groundItem.data);
                        if (success)
                        {
                            Destroy(interactTarget);
                            Debug.Log("아이템 획득 완료!");
                        }
                        else
                        {
                            Debug.Log("인벤토리가 가득 찼습니다.");
                        }
                    }
                    break;

                case "NPC":
                    Debug.Log("NPC 대화 시작!");
                    break;

                case "Store":
                    StoreUIController.Instance.OpenStoreUI();
                    break;

                case "Object":
                    Debug.Log("상호작용 가능한 오브젝트!");
                    break;
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Weapon") || other.CompareTag("NPC") || other.CompareTag("Object") || other.CompareTag("Store"))
        {
            interactTarget = other.gameObject;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject == interactTarget)
        {
            interactTarget = null;
        }
    }
}
