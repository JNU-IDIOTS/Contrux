using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

public class Grenade : MonoBehaviour
{
    [Header("Explosion Settings")]
    public GameObject explosionEffectPrefab;  // 터짐 이펙트 프리팹
    public float fuseTime = 3f;        // 발사 후 자동 폭발까지 시간 (인스펙터에서 조정 가능)

    void Start()
    {
        // 던진 즉시 타이머 시작
        GameObject player = GameObject.FindWithTag("Player");
        Vector2 direction = ((Vector2)player.transform.position - (Vector2)transform.position).normalized;
        if (Mathf.Abs(direction.x) > 0.01f)
            FaceDirection(direction.x > 0 ? 1 : -1);
        StartCoroutine(ExplosionCountdown());
    }

    IEnumerator ExplosionCountdown()
    {
        yield return new WaitForSeconds(fuseTime);
        Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("부딪힘");

        // 플레이어와 부딪히면 즉시 폭발
        if (other.CompareTag("Player"))
        {
            Debug.Log("플레이어 부딪힘");
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
        // 바닥(Ground)나 벽과 부딪혀도 여기는 비워둡니다.
    }
    public void FaceDirection(int dir) // 그림 전환 메소드
    {
        if (dir == 0) return;  // 정지 상태에서는 방향 유지

        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * -dir;  // dir이 -1이면 왼쪽, 1이면 오른쪽
        transform.localScale = scale;
    }
}
