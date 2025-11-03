using UnityEngine;
using System.Collections;

public class Boss1_CatchThrow : MonoBehaviour
{
    public Boss1_FSM FSM;
    public Boss1_Animation ani;
    public Rigidbody2D rb;
    public Boomerang boomerang;
    private MonoBehaviour mono;
    public Boss1_Coroutine coroutine;

    [SerializeField] public GameObject boomerangPrefab;
    [SerializeField] public Transform boomerangSpawnPoint;  // 던지는 위치 지정
    private void Init()
    {
        FSM = GetComponent<Boss1_FSM>();
        ani = GetComponent<Boss1_Animation>();
        boomerang = GetComponent<Boomerang>();
        coroutine = GetComponent<Boss1_Coroutine>();
    }

    private void Awake()
    {
        FSM = GetComponent<Boss1_FSM>();
        ani = GetComponent<Boss1_Animation>();
        boomerang = GetComponent<Boomerang>();
        coroutine = GetComponent<Boss1_Coroutine>();

    }
    public void LaunchBoomerang()
    {
        float playerX = FSM.target.position.x;
        float bossX = transform.position.x - playerX;
        if (FSM == null || FSM.target == null)
        {
            Debug.LogError("FSM또는 target이 null입니다!");
            return;
        }
        StartCoroutine(Waitforthrow());
        //GameObject boom = Instantiate(boomerangPrefab);
        //Boomerang boomerang = boom.GetComponent<Boomerang>();
    }
    public IEnumerator Waitforthrow()
    {
        int dir;
        float playerX = FSM.target.position.x;
        float bossX = transform.position.x - playerX;
        ani.ThrowBoomerang();

        if (bossX < 0)
        {
            dir = 1;
        }
        else
        {
            dir = -1;
        }
        FSM.FaceDirection(dir);

        yield return new WaitForSeconds(0.45f);

        GameObject boom = Instantiate(boomerangPrefab);
        Boomerang boomerang = boom.GetComponent<Boomerang>();
        Vector2 throwdir = (playerX > bossX) ? Vector2.right : Vector2.left;
        boomerang.Init(FSM); // 'this'는 Boss1_FSM 또는 Boss1_CatchThrow
        boomerang.Init(boomerang.origin, throwdir);
        boom.GetComponent<Boomerang>().Init(boomerangSpawnPoint.position, throwdir);
    }
}


