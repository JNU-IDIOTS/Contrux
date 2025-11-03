using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Collider111 : MonoBehaviour
{
    [Tooltip("감지할 콜라이더에 붙은 태그, 비워두면 모든 콜라이더 감지")]
    public string targetTag;

    // 현재 들어가 있는 콜라이더 목록
    private readonly HashSet<Collider2D> _current = new HashSet<Collider2D>();

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!string.IsNullOrEmpty(targetTag) && !other.CompareTag(targetTag)) return;
        if (_current.Add(other))
            Debug.Log($"[Enter]  ▶  {other.name}");
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!string.IsNullOrEmpty(targetTag) && !other.CompareTag(targetTag)) return;
        // Stay 이벤트는 너무 자주 찍히면 로그가 많으니 원한다면 주석 처리하세요.
        Debug.Log($"[Stay]   ▶  {other.name}");
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!string.IsNullOrEmpty(targetTag) && !other.CompareTag(targetTag)) return;
        if (_current.Remove(other))
            Debug.Log($"[Exit]   ▶  {other.name}");
    }

    private void Update()
    {
        if (_current.Count > 0)
        {
            var names = _current.Select(c => c.name);
            Debug.Log($"[Current] ▶  {string.Join(", ", names)}");
        }
        else
        {
            Debug.Log("[Current] ▶  (없음)");
        }
    }
}
