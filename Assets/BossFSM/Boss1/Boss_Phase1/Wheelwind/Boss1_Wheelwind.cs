using System.Collections.Generic;
using UnityEngine;

public class Boss1_WheelWind : MonoBehaviour
{
    [SerializeField] public LayerMask playerLayer;
    [SerializeField] public float wheelDamageInterval = 1f; // 딜 간격
    [SerializeField] public float wheelDuration = 3f;         // 휠윈드 지속 시간
    [SerializeField] public int wheelDamage = 20;
    [SerializeField] public float wheelRange = 1.5f;
    [SerializeField] public BoxCollider2D wheelAttackArea;

    private Vector3 lockedDir;
    private float lockTimer = 0f;
    private const float lockDuration = 1f;

    public HashSet<PlayerHealth> recentlyDamaged = new HashSet<PlayerHealth>();
    public Boss1_Phase1 Phase1;
    public Boss1_Animation ani;
    public Boss1_Coroutine coroutine;
    public Boss1_FSM fsm;
    
    public float moveSpeed = 15f;
    public float searchRadius = 30f;

    private UnityEngine.Transform player;

    public void Init()
    {
        ani = GetComponent<Boss1_Animation>();
        coroutine = GetComponent<Boss1_Coroutine>();
        fsm = GetComponent<Boss1_FSM>();
    }

    public void FindPlayerByLayer(LayerMask playerLayer)
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, 50f, playerLayer);
        if (hits.Length > 0)
        {
            player = hits[0].transform;
            Debug.Log(hits[0].transform);
        }
    }

    private void Awake()
    {
        Init();
    }


    private Vector3 GetDirectionToPlayer()
    {
        if (player == null)
        {
            // 모든 "Player" 태그 오브젝트 탐색
            GameObject[] playerObjs = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject obj in playerObjs)
            {
                player = obj.transform;

            }

            if (player == null)
            {
                Debug.LogWarning("Player 태그이고 이름이 'Ark'인 오브젝트를 찾을 수 없습니다.");
                return Vector3.zero;
            }
        }

        //Debug.Log(player.position);
        //Debug.Log(transform.position);
        // 방향 계산
        return (player.position - transform.position).normalized;
    }


    public void Enter()
    {
    }

    public void Update()
    {
    }
    public void Exit() 
    {
    
    }

    public void WheelMove()
    {
        /*int xdir;
        Vector3 dir = GetDirectionToPlayer();
        dir.y = 0;
        transform.position += dir * moveSpeed * Time.deltaTime;
        if (dir.x <= 0)
        {
            xdir = -1;
        }
        else
        {
            xdir = 1;
        }
        if (fsm == null)
        {
            Debug.LogError("FSM가 null입니다! 초기화되지 않았습니다.");
            return;
        }
        fsm.FaceDirection(xdir);
        //Debug.Log(transform.position);*/

        if (lockTimer <= 0f)
        {
            // 플레이어 위치 기반으로 방향 계산
            Vector3 dir = GetDirectionToPlayer();
            dir.y = 0;
            lockedDir = dir.normalized;        // 고정할 방향 저장
            lockTimer = lockDuration;          // 타이머 리셋
        }
        if (player == null)
        {
            Debug.LogWarning("플레이어를 찾지 못했으므로 이동하지 않습니다.");
        }
        // 2) 고정된 방향으로 이동
        transform.position += lockedDir * moveSpeed * Time.deltaTime;
        //Debug.Log(transform.position);
        // 3) 타이머 차감
        lockTimer -= Time.deltaTime;

        // 4) 바라보는 방향 맞추기
        int xdir = lockedDir.x < 0 ? -1 : 1;
        fsm.FaceDirection(xdir);
    }
    
}


