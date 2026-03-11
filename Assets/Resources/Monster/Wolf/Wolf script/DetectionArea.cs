using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider2D))]
public class DetectionArea : MonoBehaviour
{
    private EnemyAI enemyAI;

    // 플레이어의 여러 콜라이더(발/몸/무기 등)를 모두 추적
    private readonly HashSet<Collider2D> _playerOverlaps = new HashSet<Collider2D>();

    [SerializeField] private string playerTag = "Player"; // 필요시 변경
    [SerializeField] private bool verboseLog = false;      // 스팸 로그 방지용 스위치

    private void Awake()
    {
        enemyAI = GetComponentInParent<EnemyAI>();
        if (enemyAI == null)
            Debug.LogError($"[DetectionArea] 부모에 EnemyAI가 없습니다. ({name})");

        // 트리거 보증
        var col = GetComponent<Collider2D>();
        if (col != null && !col.isTrigger)
        {
            col.isTrigger = true;
            if (verboseLog) Debug.LogWarning("[DetectionArea] isTrigger가 꺼져 있어 자동으로 켰습니다.");
        }
    }

    private void OnEnable()
    {
        // 안전 초기화
        _playerOverlaps.Clear();
        SetInRange(false);
    }

    private void OnDisable()
    {
        // 끄는 순간 확실히 false
        _playerOverlaps.Clear();
        SetInRange(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        if (_playerOverlaps.Add(other)) // 새로 들어온 콜라이더만 반영
        {
            if (_playerOverlaps.Count == 1)
                SetInRange(true);
            if (verboseLog) Debug.Log("→ 플레이어 감지: Enter");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        if (_playerOverlaps.Remove(other))
        {
            if (_playerOverlaps.Count == 0)
                SetInRange(false);
            if (verboseLog) Debug.Log("→ 플레이어 범위 이탈: Exit");
        }
    }

    // OnTriggerStay2D는 스팸/부하 원인이라 보통 불필요. 필요하면 최소화해서 사용.
    // private void OnTriggerStay2D(Collider2D other) { ... }

    private void SetInRange(bool value)
    {
        if (enemyAI == null) return;
        enemyAI.playerInRange = value;
    }
}
