using UnityEngine;

public class SteamPressureSystem : MonoBehaviour
{
    [Header("설정")]
    public float maxPressure = 100f;
    public float currentPressure = 0f;
    
    [Header("냉각")]
    public float baseDecreaseAmount = 5f; // 초당 감소량
    public float decreaseInterval = 1f;
    private float decreaseTimer = 0f;

    [Header("비용")]
    public float baseSkillCost = 15f; // 스킬 사용 시 증가량

    [Header("오버히트")]
    public bool isOverheated = false;
    public float overheatDuration = 5f;
    private float overheatTimer = 0f;

    private PlayerRef _ref;

    private void Awake()
    {
        _ref = GetComponent<PlayerRef>();
    }

    private void Update()
    {
        if (isOverheated)
        {
            HandleOverheat();
        }
        else
        {
            HandleCooling();
        }
    }

    private void HandleCooling()
    {
        if (currentPressure > 0)
        {
            decreaseTimer += Time.deltaTime;
            
            // 🚀 [연동] 냉각 속도 적용 (1.0 = 100%, 1.5 = 150% 속도)
            float speedMult = (_ref._Status != null) ? _ref._Status.SteamCoolSpeedMult : 1.0f;
            float actualInterval = decreaseInterval / speedMult; 

            if (decreaseTimer >= actualInterval)
            {
                currentPressure -= baseDecreaseAmount;
                if (currentPressure < 0) currentPressure = 0;
                decreaseTimer = 0f;
            }
        }
    }

    private void HandleOverheat()
    {
        overheatTimer += Time.deltaTime;
        if (overheatTimer >= overheatDuration)
        {
            isOverheated = false;
            currentPressure = 0f;
            overheatTimer = 0f;
            Debug.Log("오버히트 종료");
        }
    }

    public void AddPressure(float amount)
    {
        if (isOverheated) return;

        // 🚀 [연동] 비용 감소 적용 (0.9 = 10% 할인)
        float costMult = (_ref._Status != null) ? _ref._Status.SteamCostMult : 1.0f;
        float finalCost = amount * costMult;

        currentPressure += finalCost;
        
        if (currentPressure >= maxPressure)
        {
            StartOverheat();
        }
    }

    // 스킬에서 호출하는 함수 (기존 코드 유지하며 내부 연결)
    public void ApplyCommandSkill(float amount)
    {
        AddPressure(amount);
    }

    private void StartOverheat()
    {
        isOverheated = true;
        currentPressure = maxPressure;
        Debug.Log("🔥 오버히트 발동! 🔥");
        
        // 🚀 [연동] 오버히트 강화 여부 체크
        if (_ref._Status != null && _ref._Status.IsOverheatEnhanced)
        {
            Debug.Log(">> 오버히트 강화 보너스 적용됨 (치명타 증가)");
        }
    }
}