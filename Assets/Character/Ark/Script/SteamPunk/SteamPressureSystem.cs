using UnityEngine;

public class SteamPressureSystem : MonoBehaviour
{
    [Header("압력 설정")]
    public float maxPressure = 100f;
    [SerializeField] public float pressureIncreasePerSkill_1 = 50f; //차징 강공 증가량
    [SerializeField] public float pressureIncreasePerSkill_2 = 25f; //윗 강공 증가량
    [SerializeField] private float pressureDecreaseInterval = 10f; // 몇 초마다 감소할지
    [SerializeField] private float pressureDecreaseAmount = 5f;    // 한 번에 줄일 압력량
    private float currentPressure = 0f;
    private float pressureTimer = 0f;   // 누적 타이머
   

    [Header("상태 플래그")]
    public bool isOverheated = false;  // 압력 최대치 도달 여부

    public float CurrentPressure => currentPressure;
    public float MaxPressure => maxPressure;

    private void Start()
    {
    }

    void Update()
    {
        //Debug.Log($"🔥 압력 상태: {currentPressure}");

        // 압력 감소 타이머 업데이트 (과열 상태든 아니든 감소는 진행됨)
        pressureTimer += Time.deltaTime;

        // 초과된 압력 비율 계산
        float excessRatio = currentPressure > maxPressure
            ? (currentPressure - maxPressure) / maxPressure
            : 0f; // ?조건이란 -> 조건이 참일 때 결과 /  조건이 거짓일 때 결과

        // 동적으로 감소 간격 조절 (초과할수록 간격 늘어남: 예, 기본 10초 → 최대 15초까지)
        float dynamicInterval = pressureDecreaseInterval * (1f + excessRatio * 1f);
        // 1f는 최대 2배까지 늘어나도록 조정 가능 (가중치)

        if (pressureTimer >= dynamicInterval)
        {
            currentPressure -= pressureDecreaseAmount;
            currentPressure = Mathf.Clamp(currentPressure, 0f, maxPressure * 2f); // 초과 허용 가능

            pressureTimer = 0f;
        }

        // 과열 진입 조건
        if (currentPressure >= maxPressure && !isOverheated)
        {
            isOverheated = true;
            Debug.Log("🔥 과열! 커맨드 봉인 + 평타 강화 시작");
        }

        // 과열 해제 조건
        else if (isOverheated && currentPressure < maxPressure * 0.5f)
        {
            isOverheated = false;
            Debug.Log("💨 압력 정상화됨. 커맨드 사용 가능");
        }
    }

    // 커맨드 기술 사용 시 압력 증가
    public void ApplyCommandSkill(float overhitgage)
    {
        if (isOverheated) return;
        currentPressure += overhitgage;
        currentPressure = Mathf.Clamp(currentPressure, 0f, maxPressure);
        Debug.Log($"⚙️ 스킬 사용 → 압력 증가: {currentPressure}/{maxPressure}");
    }
    public void DecreasePressure(float amount)
    {
        currentPressure -= amount;
        currentPressure = Mathf.Clamp(currentPressure, 0f, maxPressure);

        if (isOverheated && currentPressure < maxPressure * 0.5f)
        {
            isOverheated = false;
            Debug.Log("💨 압력 정상화됨. 커맨드 사용 가능");
        }
        Debug.Log($"🧯 압력 감소: {currentPressure}/{maxPressure}");
    }


}
