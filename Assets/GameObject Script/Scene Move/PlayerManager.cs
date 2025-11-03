using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }

    [Header("플레이어 트랜스폼들")]
    [SerializeField] private Transform magicPlayer;
    [SerializeField] private Transform steamPunkPlayer;

    // 어떤 타입을 쓰고 있는지 ChooseOne 스크립트 대신 여기서 판단해도 좋습니다
    public enum PlayerType { Magic, SteamPunk }
    public PlayerType CurrentType { get; private set; }

    public Transform ActivePlayerTransform
        => CurrentType == PlayerType.SteamPunk
            ? steamPunkPlayer
            : magicPlayer;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // 외부에서 타입 변경할 때
    public void SetPlayerType(PlayerType type)
    {
        CurrentType = type;
        // 필요하면 이벤트로 알릴 수도 있음
    }
}