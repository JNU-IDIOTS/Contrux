using UnityEngine;

public class Spit : MonoBehaviour
{
    public Rigidbody2D rb;

    [Header("던질까 말까")]
    public GameObject groundEffect;
    public LayerMask groundLayer;
    public void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Start()
    {
    
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        // other가 groundLayer에 속하는지 확인
        if (((1 << other.gameObject.layer) & groundLayer) != 0)
        {
            Debug.Log("닿음");
            SpawnEffectAndDestroy();
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            //Debug.Log("닿음");
            SpawnEffectAndDestroy();
        }
    }
    private void SpawnEffectAndDestroy()
    {
        // 이펙트 생성 (위치·회전은 필요에 맞게 조절)
        Instantiate(
            groundEffect,
            transform.position,    // 충돌한 오브젝트 위치
            Quaternion.identity
        );
        // 원본 오브젝트 파괴
        Destroy(gameObject);
    }
}
