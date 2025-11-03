using UnityEngine;
using System.Collections;
public class Boss1_Phasechange: MonoBehaviour
{
    public static bool Is2phase = false;
    [Header("Wiring (선택 연결)")]
    public Renderer Renderer;
    public GameObject boss;
    [SerializeField] public Transform Phase1_Ground;

    [Header("Auto-Refs")]
    public Boss1_FSM fsm;
    public BoxColliderGizmo gizmo;

    void Awake()
    {
        // 1) FSM 찾기: boss 오브젝트 쪽 먼저 → 전체 씬(비활성 포함)
        if (fsm == null && boss != null)
            fsm = boss.GetComponentInChildren<Boss1_FSM>(true);
        if (fsm == null)
            fsm = Object.FindAnyObjectByType<Boss1_FSM>(FindObjectsInactive.Include);

        if (fsm == null)
            Debug.LogWarning("[Boss1_Phasechange] Boss1_FSM을 찾지 못했습니다. Boss1이 씬에 없거나 스크립트가 비활성일 수 있습니다.");

        // 2) Gizmo
        if (gizmo == null)
            gizmo = Object.FindAnyObjectByType<BoxColliderGizmo>(FindObjectsInactive.Include);
        if (gizmo == null)
            Debug.LogWarning("[Boss1_Phasechange] BoxColliderGizmo를 찾지 못했습니다.");

        // 3) Renderer: 미지정이면 자기 자식/부모 쪽에서 자동 탐색
        if (Renderer == null)
            Renderer = GetComponentInChildren<Renderer>(true) ?? GetComponent<Renderer>();
        if (Renderer == null)
            Debug.LogWarning("[Boss1_Phasechange] Renderer가 없습니다. 정렬 변경이 적용되지 않습니다.");

        // 4) Phase1_Ground 자동 보정(옵션): 이름 기반(이미지 트리상 이름 같으면 시도)
        if (Phase1_Ground == null)
        {
            var t = transform.root.Find("BossMap/Phase1_Ground");
            if (t == null) t = GameObject.Find("Phase1_Ground")?.transform;
            Phase1_Ground = t;
            if (Phase1_Ground == null)
                Debug.LogWarning("[Boss1_Phasechange] Phase1_Ground를 찾지 못했습니다. 파괴가 생략됩니다.");
        }
    }
    void Start()
    {
        fsm = Object.FindAnyObjectByType<Boss1_FSM>();
        gizmo = Object.FindAnyObjectByType<BoxColliderGizmo>();
    }

    // Update is called once per frame
    void Update()
    {
        if (fsm.hit == true)
        {
            StartCoroutine(Phasechange());
            Debug.Log("때리기");
            fsm.hit = false;
            
        }
    }



    public IEnumerator Phasechange()
    {
    
        Is2phase = true;
        gizmo.DdalGGak = false;
        Renderer.sortingOrder = 20;
        Destroy(Phase1_Ground.gameObject);
        yield return new WaitForSeconds(2f);
        Renderer.sortingOrder = -5;
        

    }
}
