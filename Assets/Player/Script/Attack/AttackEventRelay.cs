using UnityEngine;

// 이 스크립트를 Animator 컴포넌트가 있는 각 하위 오브젝트에 추가해야 합니다.
public class AttackEventRelay : MonoBehaviour
{
    // 🚨 부모의 공격 스크립트를 캐싱할 변수
    private NewAttackSkill _attackScript; 

    private void Awake()
    {
        // 부모 GameObject에서 NewAttackSkill 컴포넌트를 찾습니다.
        _attackScript = GetComponentInParent<NewAttackSkill>();
        
        if (_attackScript == null)
        {
            Debug.LogError("AttackEventRelay could not find NewAttackSkill in parent hierarchy. Check if it's attached to the Player Root GameObject.");
        }
    }

    // 🚨 [핵심]: 애니메이션 이벤트가 호출할 메서드
    // Unity Animator 클립의 이벤트 함수 이름으로 'CallOnAttackEnd'를 지정해야 합니다.
    public void CallOnAttackEnd()
    {
        if (_attackScript != null)
        {
            _attackScript.OnAttackEnd(); // 부모 스크립트의 OnAttackEnd()를 호출합니다.
        }
    }
    
    // 필요하다면 다른 이벤트도 추가 (예: 히트박스 활성화/비활성화)
    // public void CallOnHitFrame() { ... }
}