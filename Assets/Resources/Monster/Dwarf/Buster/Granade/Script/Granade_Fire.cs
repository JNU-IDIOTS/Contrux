using UnityEngine;
using System.Collections;

public class DwarfBusterBullet : MonoBehaviour
{
    public GameObject bulletPrefab;          // 총알 프리팹
    public Transform firePoint;              // 발사 위치 (오브젝트 앞쪽)
    [SerializeField] private float bulletspeed = 0f;

    public void FireBullet() //enemyAI의 movetoplayer 메소드를 사용해서 수류탄이 되게 만들것.
    {
        if (bulletPrefab != null && firePoint != null)
        {
            GameObject bullet = GameObject.Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

            Vector2 direction = transform.localScale.x < 0 ? Vector2.right : Vector2.left;

            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.gravityScale = 1f; // 중력 영향 받게 설정 (필요 시 조절)

                // 포물선 방향: x축 속도는 바라보는 방향, y축은 위로 발사
                Vector2 force = new Vector2(direction.x * bulletspeed, bulletspeed * 0.5f);
                rb.AddForce(force, ForceMode2D.Impulse);
            }
        }
    }


}

