using UnityEngine;

public class PlayerRef : MonoBehaviour
{
    // 🚀 외부에서 가져다 쓸 컴포넌트들 (읽기 전용)
    [field: Header("Core")]
    public Rigidbody2D _Rb { get; private set; }
    public Collider2D _Col { get; private set; }

    [field: Header("Controllers")]
    public PlayerMoveController _Move { get; private set; }
    public SteamPunk_Hook _Hook { get; private set; }
    public PlayerAnimationSync _AnimSync { get; private set; }
    public Ground _Ground { get; private set; }
    public DashSkill _Dash { get; private set; }
    public NewAttackSkill _Attack { get; private set; }
    public SteamPressureSystem _SteamPressureSystem { get; private set; }

    private void Awake()
    {
        // 1. 자기 자신의 컴포넌트 할당
        _Rb = GetComponent<Rigidbody2D>();
        _Col = GetComponent<Collider2D>();
        _Move = GetComponent<PlayerMoveController>();
        _AnimSync = GetComponent<PlayerAnimationSync>();
        _Ground = GetComponent<Ground>();
        _Dash = GetComponent<DashSkill>();
        _Attack = GetComponent<NewAttackSkill>();
        _SteamPressureSystem = GetComponent<SteamPressureSystem>();
        _Hook = GetComponent<SteamPunk_Hook>();


        // 2. 자식에 있는 컴포넌트 할당

    }
}