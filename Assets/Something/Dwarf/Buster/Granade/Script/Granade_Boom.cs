using UnityEngine;
using System.Collections;

public class Grenade_Boom : MonoBehaviour
{
    [Header("Explosion Settings")]
    public float explosionRadius = 2f;   // 폭발 반경
    public float explosionForce = 10f;  // 폭발 힘
    public int Damage = 50;   // 최대 대미지
    private bool hasExploded = false;

    private PlayerHealth player;

    private void Awake()
    {
        player = GetComponent<PlayerHealth>();
    }

    // 1) 플레이어와 충돌하면 폭발
    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasExploded || !other.CompareTag("Player"))
            return;

        hasExploded = true;
        Debug.Log("수류탄 터짐");

        // 주변 플레이어에게만 물리 반발력과 대미지 적용
        if (other.CompareTag("Player"))
        {  
            // 대미지 계산
            /*float dist = Vector2.Distance(transform.position, other.transform.position);
            float ratio = Mathf.Clamp01((explosionRadius - dist) / explosionRadius);
            int damage = Mathf.RoundToInt(ratio * maxDamage);*/

            // 플레이어 체력 감소 및 넉백 플래그
            PlayerHealth player = other.GetComponent<PlayerHealth>();
            if (player != null)
            {
                player.isknockback = true;
                player.TakeDamage(Damage,transform , 3f, 5f);
            }
            // 물리 반발력
            Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 dir = rb.position - (Vector2)transform.position;
                rb.AddForce(dir.normalized * explosionForce, ForceMode2D.Impulse);
            }
        }
    }
    void SelfDestory()
    {
        // 자기 자신 제거
        Destroy(gameObject);
    }

    // 씬 뷰에서 폭발 반경 시각화
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
    private IEnumerator waitAddforce()
    {
        yield return null;
    }
}
