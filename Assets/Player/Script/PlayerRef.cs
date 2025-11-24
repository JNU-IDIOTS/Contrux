using UnityEngine;

public class PlayerRef : MonoBehaviour
{
    // [기존 코드 유지를 위한 변수들]
    [Header("Core Components")]
    public Rigidbody2D _Rb;
    public PlayerMoveController _Move;
    public Collider2D _Col;                 
    public PlayerAnimatorController _Anim;  
    public PlayerAnimationSync _AnimSync;   

    [Header("Systems & Skills")]
    public SteamPressureSystem _SteamPressureSystem; 
    public DashSkill _Dash;                          
    public NewAttackSkill _Attack;                   
    public Ground _Ground;                           

    // 🚀 [상점 연동용]
    [Header("New Stats Integration")]
    public PlayerStatus _Status;
    // public StatDataManager _Statu; // 🚀 삭제 권장: 싱글톤으로 접근하므로 불필요
    public PlayerHealth _Health; 

    private void Awake()
    {
        // 기존 컴포넌트 연결
        _Rb = GetComponentInChildren<Rigidbody2D>();
        _Move = GetComponentInChildren<PlayerMoveController>();
        _Anim = GetComponentInChildren<PlayerAnimatorController>();
        _AnimSync = GetComponentInChildren<PlayerAnimationSync>();
        _Col = GetComponentInChildren<Collider2D>();

        _SteamPressureSystem = GetComponentInChildren<SteamPressureSystem>();
        _Dash = GetComponentInChildren<DashSkill>();
        _Attack = GetComponentInChildren<NewAttackSkill>();
        _Ground = GetComponentInChildren<Ground>(); 

        // 🚀 [수정] _Statu 초기화 제거
        _Status = GetComponentInChildren<PlayerStatus>();
        // _Statu = GetComponentInChildren<StatDataManager>(); // 🚀 삭제
        _Health = GetComponentInChildren<PlayerHealth>();
        
        if (_Status == null) Debug.LogError("❌ PlayerStatus 스크립트를 못 찾겠습니다! 플레이어 오브젝트나 자식 어딘가에 붙여주세요.");
        if (_Health == null) Debug.LogError("❌ PlayerHealth 스크립트를 못 찾겠습니다! 플레이어 오브젝트나 자식 어딘가에 붙여주세요.");
    }
}