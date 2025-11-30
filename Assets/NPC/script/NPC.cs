using UnityEngine;

public class NPC : MonoBehaviour
{
    [Header("NPC 데이터 설정")]
    public NPCData npcData; 

    [Header("감지 설정")]
    // 🚀 [수정됨] 감지 범위를 3f -> 1.5f로 줄였습니다.
    public float detectRange = 1.5f; 
    
    public GameObject interactionHint; 

    private Transform _playerTransform;
    private bool _isPlayerInRange = false;

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) _playerTransform = player.transform;
        if (interactionHint != null) interactionHint.SetActive(false);
    }

    private void Update()
    {
        if (_playerTransform == null) return;

        float distance = Vector2.Distance(transform.position, _playerTransform.position);
        _isPlayerInRange = distance <= detectRange;

        if (interactionHint != null)
            interactionHint.SetActive(_isPlayerInRange);

        // 🚀 [수정(우현)] KeyManager 사용
        if (_isPlayerInRange && Input.GetKeyDown(KeyManager.Instance.KeyInteract))
        {
            if (SoulStoreUI.Instance != null && !SoulStoreUI.Instance.IsOpen)
            {
                SoulStoreUI.Instance.StartInteraction(npcData); 
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);
    }
}