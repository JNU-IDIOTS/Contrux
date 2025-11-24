using UnityEngine;

public class NPC : MonoBehaviour
{
    [Header("NPC 데이터 설정")]
    public NPCData npcData; 

    [Header("감지 설정")]
    public float detectRange = 1.5f; 
    public GameObject interactionHint; 

    private Transform _playerTransform;
    private bool _isPlayerInRange = false;

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) 
        {
            _playerTransform = player.transform;
        }
        else
        {
            // 🚨 범인 1: 태그 문제
            Debug.LogError("❌ [NPC 에러] 'Player' 태그를 가진 오브젝트를 못 찾았습니다! 플레이어 Tag를 확인하세요.");
        }

        if (interactionHint != null) interactionHint.SetActive(false);
    }

    private void Update()
    {
        if (_playerTransform == null) return;

        float distance = Vector2.Distance(transform.position, _playerTransform.position);
        _isPlayerInRange = distance <= detectRange;

        if (interactionHint != null)
            interactionHint.SetActive(_isPlayerInRange);

        if (Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log($"🔍 [진단] F키 눌림 / 범위안: {_isPlayerInRange}");

            if (!_isPlayerInRange) return;

            if (SoulStoreUI.Instance == null)
            {
                Debug.LogError("❌ [NPC 에러] SoulStoreUI가 없습니다! UI 오브젝트가 켜져 있는지 확인하세요.");
                return;
            }

            if (SoulStoreUI.Instance.IsOpen)
            {
                Debug.Log("⚠️ UI가 이미 열려있어서 무시됨.");
                return;
            }

            if (npcData == null)
            {
                Debug.LogError("❌ [NPC 에러] NPC Data가 비어있습니다! 인스펙터에 데이터를 넣으세요.");
                return;
            }

            // 정상 실행
            Debug.Log("✅ 대화 시작!");
            SoulStoreUI.Instance.StartInteraction(npcData); 
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);
    }
}