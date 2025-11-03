using UnityEngine;
using System.Collections;

public class GuardSystem : MonoBehaviour
{
    [Header("가드 설정")]
    [SerializeField] public float maxGuardPower = 20f;    // 최대 가드 파워
    [SerializeField] public float guardDisableTime = 3f;  // 가드 파워 0 이후 재사용까지 걸리는 시간
    public float currentGuardPower;                       // 현재 가드 파워
    public bool isGuardDisabled = false;                  // 현재 가드 불능 상태 여부
    public float guardDisabledTimer = 0f;                 // 가드 불능 타이머
    public bool isGuarding;   // PlayerHealth 쪽에서 참조하는 가드 상태

    //private PlayerAnimatorController animController;
    private PlayerAnimationSync sync;
    void Awake()
    {
        //animController = GetComponentInParent<PlayerAnimatorController>();
        sync = GetComponentInParent<PlayerAnimationSync>();
    }

    public void Update() 
    {
        bool guardingNow = !isGuardDisabled && Input.GetKey(KeyCode.S);

        //가드 입력 감지 (S 키를 누르면 가드 활성화)
        //Debug.Log(guardingNow);
        if (Input.GetKeyDown(KeyCode.S))
        {
            isGuarding = true;
            //animController.Guard();
            sync.Guard();
            StartCoroutine(Guarding());
        }
        else if (Input.GetKeyUp(KeyCode.S))
        {
            isGuarding = false;
            //animController.NotGuard();
            sync.NotGuard();
        }

        // [2] 가드 비활성화 상태일 경우 타이머로 재충전
        if (isGuardDisabled)
        {
            guardDisabledTimer += Time.deltaTime;
            if (guardDisabledTimer >= guardDisableTime)
            {
                currentGuardPower = maxGuardPower;     // 가드 파워 완전 회복
                isGuardDisabled = false;               // 가드 가능 상태로 전환
                guardDisabledTimer = 0f;
            }
        }
    }
    private IEnumerator Guarding()
    {
        //animController.Guarding();
        sync.Guarding();
        yield return new WaitForSeconds(1f);
    }
}
