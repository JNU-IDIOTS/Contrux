using UnityEngine;

public class AnimEventRelay : MonoBehaviour
{
    [Tooltip("이 오브젝트를 이벤트 소스로 쓸지 (Body만 체크)")]
    [SerializeField] private bool isEventSource = true;

    private IAttackAnimEvents receiver;

    void Awake()
    {
        // 부모 트리에 있는 수신자(부모 스크립트) 찾기
        receiver = GetComponentInParent<IAttackAnimEvents>();
        if (receiver == null)
            Debug.LogWarning("[AnimEventRelay] 상위에서 IAttackAnimEvents 찾지 못함.", this);
    }

    // === 애니메이션 이벤트에서 직접 호출할 메서드들 ===
    // Attack 1/2 클립의 타격 프레임에서 호출 (int 인자: 1 or 2)
    public void AE_Step(int index)
    {
        if (!isEventSource) return;
        receiver?.OnStep(index);
    }

    // 공격/스킬 끝 프레임에서 호출
    public void AE_AttackEnd()
    {
        if (!isEventSource) return;
        receiver?.OnAttackEnd();
    }

    // 스킬 시작 프레임에서 호출(필요할 때만)
    public void AE_SkillStart()
    {
        if (!isEventSource) return;
        receiver?.OnSkillStart();
    }

    // 스킬 종료 프레임에서 호출
    public void AE_SkillEnd()
    {
        if (!isEventSource) return;
        receiver?.OnSkillEnd();
    }
}
