using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Boss1_Animation))]
[RequireComponent(typeof(Boss1_Coroutine))]
public class Boss1_BackDash : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] public Transform player;  // 인스펙터에서 넣어도 되고, 비면 자동 탐색
    public Rigidbody2D rb { get; private set; }
    public Boss1_Coroutine coroutine { get; private set; }
    public Boss1_Animation ani { get; private set; }

    // MonoBehaviour가 아닌 가능성 있으니 GetComponent 금지
    // public Boss1_Phase1 phase1;

    [Header("Params")]
    public float dashSpeed = 0.1f;

    // 외부에서 읽기 전, 반드시 보장되도록 프로퍼티로 노출
    public Transform Player
    {
        get
        {
            if (!player) TryResolvePlayer();
            return player;
        }
    }

    void Reset()
    {
        // 에디터에서 컴포넌트 추가할 때 자동 결선
        rb = GetComponent<Rigidbody2D>();
        ani = GetComponent<Boss1_Animation>();
        coroutine = GetComponent<Boss1_Coroutine>();
        if (!player) TryResolvePlayer();
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        ani = GetComponent<Boss1_Animation>();
        coroutine = GetComponent<Boss1_Coroutine>();

        // GetComponent<Boss1_Phase1>()는 금지(컴포넌트가 아닐 수 있음)
        // phase1은 FSM 등 매니저에서 참조 주입받아야 함.

        if (!player) TryResolvePlayer();
    }

    void OnEnable()
    {
        // 비활성→활성 전환 시에도 보강
        if (!player) TryResolvePlayer();
    }

    void Start()
    {
        // 다른 컴포넌트가 Awake에서 코루틴을 시작해도 Start 시점에는 대부분 준비됨
        if (!player)
        {
            Debug.LogError("[Boss1_BackDash] Player 참조를 찾지 못했습니다. Tag=Player 또는 직접 할당을 확인하세요.", this);
        }
    }

    void TryResolvePlayer()
    {
        // 1순위: Tag
        var go = GameObject.FindWithTag("Player");
        if (go) { player = go.transform; return; }

        // 2순위: 씬에서 플레이어 컨트롤러/헬스 같은 대표 컴포넌트로 탐색 (있다면 타입 바꿔주세요)
        var pc = FindObjectOfType<PlayerHealth>(includeInactive: true);
        if (pc) { player = pc.transform; return; }
    }

    // 외부(코루틴 포함)에서 안전하게 호출할 수 있는 유틸
    public bool TryGetPlayer(out Transform t)
    {
        t = Player;
        return t != null;
    }
}
