using System.Collections;
using UnityEngine;

public class DamageObject : MonoBehaviour
{
    public Boss1_FSM FSM;
    [SerializeField] public GameObject Phase1_Ground;
    public Boss1_Pillar targetPillar;
    public GameObject Circle;
    public void Awake()
    {
     FSM = GetComponent<Boss1_FSM>();   
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject == Phase1_Ground)
        {
            // Phase1_Ground와 닿으면 phase1Ground 오브젝트 제거
            Destroy(Circle);
            Debug.Log("Phase1_Ground와 충돌: 제거됨");
        }
        else if (collision.CompareTag("Player"))
        {
            // 플레이어 태그와 닿으면 TakeDamage 호출
            PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(5,transform, 0f,0f);
                StartCoroutine(punk());
                Debug.Log("플레이어가 데미지를 입음");
                Destroy(Circle);
            }
        }
    }
    private IEnumerator punk()
    {
        yield return new WaitForSeconds(2f);
        Destroy(Circle);
    }








}
    