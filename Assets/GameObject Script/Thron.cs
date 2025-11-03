using UnityEngine;

public class Thorn:MonoBehaviour
{
    [SerializeField] private LayerMask enemyLayer; // Inspector 에서 Enemy 레이어 지정

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth player = other.GetComponent<PlayerHealth>();
            if (player != null)
            {
                Debug.Log("rktlrktl");
                player.TakeDamage(5, transform, 0f,0f);
            }
        }
        else if ((enemyLayer.value & (1 << other.gameObject.layer)) != 0)
        {
            // 트리거 콜라이더가 자식에 붙어있을 수 있으니 부모에서 컴포넌트 검색
            var enemy = other.GetComponentInParent<MonsterHP>();
            if (enemy != null)
            {
                // 예: 10 대미지, 넉백 0, 0
                enemy.Getdamage(100);
            }
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth player = other.GetComponent<PlayerHealth>();
            if (player != null)
            {
                Debug.Log("rktlrktl");
                player.TakeDamage(5, transform, 0f, 0f);
            }
        }
        else if ((enemyLayer.value & (1 << other.gameObject.layer)) != 0)
        {
            // 트리거 콜라이더가 자식에 붙어있을 수 있으니 부모에서 컴포넌트 검색
            var enemy = other.GetComponentInParent<MonsterHP>();
            if (enemy != null)
            {
                // 예: 10 대미지, 넉백 0, 0
                enemy.Getdamage(10);
            }
        }
    }


}
