using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    // [1] 체력 관련 설정
    [Header("HP 설정")]
    public static float maxHP = 100f;
    public static float currentHP;
    public static bool healbuttonclicked = false;
    public static float discountDamage = 0;
    public static float finaldamage;
    public static float PlayerShield = 0;
    public static float RemainShield = 0;
    public static bool isImmortalNow = false;
    ApplyDeffenceStatBonus applyDeffenceStatBonus;
    Die PlayerDie;

    [Header("HP 계산식")]
    [SerializeField] private float baseHP = 100f;      // ★ 기본 HP
    [SerializeField] private float hpPerHealthStat = 10f; // ★ health 1당 증가량

    public float CurrentHP => currentHP;
    public float MaxHP => maxHP;
    // [2] 피격 쿨타임 설정
    [Header("피격 쿨타임")]
    [SerializeField] private float damageCooldown = 1.0f;

    // [3] 넉백 & 스턴 관련 설정
    [Header("넉백 & 스턴")]
    [SerializeField] private float stunDuration = 0.5f;
    private Coroutine knockbackRoutine;

    private Coroutine regenCoroutine;
    [SerializeField] private float baseRegenRate = 1f;
    [SerializeField] private float regenInterval = 0.1f;

    //체력 ui
    public HealthBarUIController hpUI;  // 인스펙터에서 직접 연결할 것

    public bool isknockback = false;

    [SerializeField]private bool isStunned = false;

    // [6] 참조 컴포넌트
    private Rigidbody2D rb;
    private Die die;
    private GuardSystem guard;
    private PlayerAnimatorController animController;
    private DashSkill dash;

    public float GuardRatio => guard.currentGuardPower / guard.maxGuardPower;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        guard = GetComponent<GuardSystem>();
        animController = GetComponent<PlayerAnimatorController>();
        die = GetComponent<Die>();
        dash = GetComponent<DashSkill>();

        // [시작 초기화] 체력 및 가드값 초기화
        RecalculateMaxHP(keepRatio: false); // 처음엔 현재 체력을 새 max로 맞춤
        currentHP = maxHP;
        guard.currentGuardPower = guard.maxGuardPower;
    }

    private void OnEnable()
    {
        ManaSystem.OnSkillCast += HandleSkillCast;

        // ★ 스탯 변경에 반응
        Ark_stat.StatsChanged += OnStatsChanged;
    }

    private void OnDisable()
    {
        ManaSystem.OnSkillCast -= HandleSkillCast;

        // ★ 구독 해제
        Ark_stat.StatsChanged -= OnStatsChanged;
    }

    private void OnStatsChanged()
    {
        // health 올릴 때마다 최대체력 재산출
        RecalculateMaxHP(keepRatio: true);  // 체력 비율 유지 권장
    }

    private void RecalculateMaxHP(bool keepRatio)
    {
        float oldMax = maxHP;
        float oldHP = currentHP;
        float ratio = (oldMax > 0f) ? (oldHP / oldMax) : 1f;

        maxHP = baseHP + Ark_stat.health * hpPerHealthStat;

        if (keepRatio)
        {
            // 비율 유지 모드: 현재 체력도 같은 퍼센트가 되도록
            currentHP = Mathf.Clamp(maxHP * ratio, 0f, maxHP);
        }
        else
        {
            // 초기화 모드: 첫 계산 등에서는 새 최대치로 세팅
            currentHP = Mathf.Clamp(currentHP, 0f, maxHP);
        }

        // UI 반영
        if (hpUI != null)
        {
            // 무적 표시 규칙 유지
            if (isImmortalNow) hpUI.SetHP(1, (int)maxHP);
            else hpUI.SetHP((int)currentHP, (int)maxHP);
        }
    }

    void Start()
    {
        //PlayerExpManager.PlayerDead();
    }

    void Update()
    {
        //maxHP = maxHP + (Ark_stat.health * 10);
        
        // 기존 가드 회복 처리
        if (guard.isGuardDisabled)
        {
            guard.guardDisabledTimer += Time.deltaTime;
            if (guard.guardDisabledTimer >= guard.guardDisableTime)
            {
                guard.currentGuardPower = guard.maxGuardPower;
                guard.isGuardDisabled = false;
                guard.guardDisabledTimer = 0f;
            }
        }
        Debug.Log($"시발좀{isknockback}");

        // 체력이 다 안 찼고, 자동 회복이 꺼져 있다면 다시 시작
        if (HpGenerate.isRegenerating)
        {
            if (currentHP < maxHP && regenCoroutine == null && healbuttonclicked)
            {
                StartHealthRegen();
            }
            else if (currentHP == maxHP && regenCoroutine != null && healbuttonclicked)
            {
                StopHealthRegen();
            }
        }
        //Debug.Log($"체력 : {currentHP}");
        //Debug.Log($"[Update] SoulBClicked: {DamageDiscount.SoulBClicked}, negateNextDamage: {NonDamage.negateNextDamage}");

        if (FindObjectOfType<UINavigator>() != null)
        {
            Debug.Log("씬 안에 InventoryBackPanelAdapter 있음");
        }
        else
        {
            Debug.Log("씬 안에 없음");
        }
    }
    private void HandleSkillCast(string skillName)
    {
        if (SoulBuffShield.shieldCanUse && SoulBuffShield.DefenceSClicked)
        {
            PlayerShield = MaxHP;
            RemainShield = 10;
            Debug.Log($"플레이어 쉴드: {PlayerShield}, 남은 쉴드: {RemainShield}");
            RemainShield = Mathf.Clamp(RemainShield, 0, PlayerShield); // 범위 보정
            SoulBuffShield.shieldCanUse = false;
            if (hpUI != null)
            {
                hpUI.SetShield((int)RemainShield, (int)PlayerShield);
            }
        }
    }



    // [피격 처리 함수] 외부에서 호출 (적 공격 등)
    public void TakeDamage(int damage, Transform damageSource, float knockbackForce, float knockbackUpwardForce)
    {
        {
            if (isImmortalNow)
            {
                //Debug.Log("무적 상태입니다.");
                return;
            }

            
            finaldamage = damage;

            // 사망했거나 스턴, 대쉬 무적 상태면 무시
            if (die.isDead || isStunned || dash.isInvincible) return;

            // **playerAttack이 할당되지 않았다면 guard 검사 자체 무시**
            if (guard != null && guard.isGuarding)
            {
                //Debug.Log("가드 중: 데미지 처리");
                guard.currentGuardPower -= finaldamage;

                if (guard.currentGuardPower > 0f)
                {
                    // 가드 성공 → 체력 감소 없음
                    return;
                }
                else
                {
                    // 가드 파워 소진 → 넘친 데미지를 체력에 반영
                    int overflowDamage = Mathf.CeilToInt(-guard.currentGuardPower);
                    guard.currentGuardPower = 0f;

                    // 가드 깨짐 처리
                    guard.isGuardDisabled = true;
                    guard.guardDisabledTimer = 0f;
                    guard
                        .isGuarding = false;

                    animController.GuardBreak();

                    if (overflowDamage > 0)
                        currentHP -= overflowDamage;
                    if (TryGetComponent<SoulBuffAttack>(out var buff))
                    {
                        buff.TryActivateBuffAfterHit();
                    }

                }
            }
            else
            {
                // [일반 피격 처리] 가드 중이 아닐 경우
                if (PlayerShield != 0)
                {
                    if (RemainShield < finaldamage)
                    {
                        finaldamage -= RemainShield;
                        RemainShield = 0;
                        SoulBuffShield.shieldReady = false;
                        PlayerShield = RemainShield;
                        //Debug.Log($"총 쉴드: {PlayerShield}, 남은 쉴드: {RemainShield}");
                    }
                    else if (RemainShield >= finaldamage)
                    {
                        RemainShield -= finaldamage;
                        finaldamage = 0;
                        //Debug.Log($"총 쉴드: {PlayerShield}, 남은 쉴드: {RemainShield}");
                        if (hpUI != null)
                        {
                            hpUI.SetShield((int)RemainShield, (int)PlayerShield);
                        }
                    }
                }
                if (currentHP <= finaldamage && !SoulBuffInvincibility.immortalOnceUsed && SoulBuffInvincibility.Defence6Clicked)
                {
                    StartCoroutine(ImmortalOnceCoroutine());
                }
                currentHP -= finaldamage;
                if (TryGetComponent<SoulBuffAttack>(out var buff))
                {
                    buff.TryActivateBuffAfterHit();
                }

                // 넉백 처리
                isknockback = true;
                KnockBack(damageSource, knockbackForce, knockbackUpwardForce);

                // 이전 코루틴 돌고 있으면 정리
                if (knockbackRoutine != null)
                    StopCoroutine(knockbackRoutine);

                // 0.1초 정도 유지 후 끄기 (원하면 값 조절)
                knockbackRoutine = StartCoroutine(knockback(0.1f));
            }

            finaldamage = damage;
            if (NonDamage.negateNextDamage)
            {
                finaldamage = 0;
                NonDamage.negateNextDamage = false;
                NonDamage.isNegateReserved = false; //예약 상태 초기화
                //Debug.Log("무효화 발동!");
                //Debug.Log($"[DEBUG] 무효화: {NonDamage.negateNextDamage}, 원본 데미지: {damage}, 최종 데미지: {finaldamage}");
                if (PlayerShield != 0)
                {
                    RemainShield -= finaldamage;
                    RemainShield = Mathf.Clamp(RemainShield, 0, PlayerShield); // 범위 보정
                    if (hpUI != null)
                    {
                        hpUI.SetShield((int)RemainShield, (int)PlayerShield);
                    }
                }
            }
            else if (!NonDamage.negateNextDamage)
            {
                finaldamage = damage;
                if (DamageDiscount.SoulBClicked)
                {

                    //Debug.Log("데미지가 감소됩니다.");
                    finaldamage = damage - (damage * discountDamage);
                    if (PlayerShield != 0)
                    {
                        if (RemainShield < finaldamage)
                        {
                            finaldamage -= RemainShield;
                            RemainShield = 0;
                            SoulBuffShield.shieldReady = false;
                            PlayerShield = RemainShield;
                            //Debug.Log($"총 쉴드: {PlayerShield}, 남은 쉴드: {RemainShield}");
                            if (hpUI != null)
                            {
                                hpUI.SetShield((int)RemainShield, (int)PlayerShield);
                            }
                        }
                        else if (RemainShield >= finaldamage)
                        {
                            RemainShield -= finaldamage;
                            finaldamage = 0;
                            //Debug.Log($"총 쉴드: {PlayerShield}, 남은 쉴드: {RemainShield}");
                            if (hpUI != null)
                            {
                                hpUI.SetShield((int)RemainShield, (int)PlayerShield);
                            }
                        }
                    }
                }
            }
            //체력 깎이고 나서 회복 다시 시작 조건
            if (!HpGenerate.isRegenerating && currentHP < maxHP && healbuttonclicked)
            {
                HpGenerate.isRegenerating = true;
                //Debug.Log("[TakeDamage] 체력 감소 → 자동 회복 재시작 조건 충족");
            }
            if (hpUI != null)
            {
                if (isImmortalNow)
                    hpUI.SetHP(1, (int)maxHP);
                else
                    hpUI.SetHP((int)currentHP, (int)maxHP);
            }

            if (currentHP <= 0 /*&& SoulBuffInvincibility.immortalOnceUsed*/)
            {
                Debug.Log("뒤짐");
                PlayerDie.die();
            }
        }
    }


    public void KnockBack(Transform damageSource, float knockbackForce, float knockbackUpwardForce)
    {
        // 1. 넉백 방향 계산
        // 플레이어의 위치에서 공격한 대상의 위치를 빼서, 공격 대상으로부터 '멀어지는' 방향 벡터를 구합니다.
        Vector2 knockbackDir = (transform.position - damageSource.position).normalized;
        // 2. 기존 속도 초기화
        rb.linearVelocity = Vector2.zero;
        
        // 3. 계산된 방향으로 힘 적용
        Vector2 forceToApply = new Vector2(knockbackDir.x * knockbackForce, knockbackUpwardForce);
        rb.AddForce(forceToApply, ForceMode2D.Impulse);
        Debug.Log($"플레이어가 {damageSource.name}로부터 넉백당했습니다!");
    }

    public IEnumerator ImmortalOnceCoroutine()
    {
        //Debug.Log("[ImmortalOnce] 발동됨: 5초 무적 + 회복 중단");
        isImmortalNow = true;
        StopHealthRegen();
        currentHP = 1;
        SoulBuffInvincibility.UseImmortalOnce();

        yield return new WaitForSeconds(5f);

        currentHP = Mathf.Max(currentHP, 1f);
        isImmortalNow = false;
        // 조건에 따라 자동 회복 다시 시작
        if (currentHP < maxHP && healbuttonclicked)
        {
            StartHealthRegen();
            //Debug.Log("[ImmortalOnce] 5초 후 회복 재시작됨");
        }
        //Debug.Log("[ImmortalOnce] 종료됨");
    }

    public void StartHealthRegen()
    {
        if (regenCoroutine == null && healbuttonclicked)
        {
            regenCoroutine = StartCoroutine(RegenerateHealth());
            //Debug.Log("[PlayerHealth] 체력 자동 회복 시작됨");
        }
    }

    public void StopHealthRegen()
    {
        if (regenCoroutine != null)
        {
            StopCoroutine(regenCoroutine);
            regenCoroutine = null;
            HpGenerate.isRegenerating = false;
            //Debug.Log("[PlayerHealth] 체력 자동 회복 중단됨");
        }
    }

    private IEnumerator RegenerateHealth()
    {
        while (true)
        {
            if (isImmortalNow)
            {
                //Debug.Log("[RegenerateHealth] 무적 상태 → 회복 중단");
                yield break;
            }
            if (currentHP < maxHP && healbuttonclicked)
            {
                float regenAmount = baseRegenRate; // 회복량

                currentHP += Mathf.CeilToInt(regenAmount * regenInterval);
                currentHP = Mathf.Clamp(currentHP, 0, maxHP);

                if (hpUI != null)
                    hpUI.SetHP((int)currentHP, (int)maxHP); // UI 업데이트

            }
            else if ((currentHP >= maxHP && healbuttonclicked) || isImmortalNow)
            {
                StopHealthRegen(); // 다 찼으면 루프 종료
                yield break;
            }

            yield return new WaitForSeconds(0.5f);
        }
    }
    private IEnumerator knockback(float holdSeconds = 0.1f)
    {
        // 물리 계산 한 틱 지난 뒤
        yield return new WaitForFixedUpdate();

        // (선택) 아주 짧게 더 유지하고 싶으면
        if (holdSeconds > 0f)
            yield return new WaitForSeconds(holdSeconds);

        isknockback = false;
    }
}
