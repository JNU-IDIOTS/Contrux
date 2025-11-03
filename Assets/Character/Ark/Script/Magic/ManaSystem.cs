using System.Collections;
using UnityEngine;

public class ManaSystem : MonoBehaviour
{
    [Header("마력 설정")]
    [SerializeField] private float maxMana = 100000f;
    [SerializeField] private float currentMana;
    [SerializeField] private float regenRate = 8f;
    [SerializeField] private QSkillUI QskillUI;
    [SerializeField] private WSkillUI WskillUI;

    //스킬을 발동시켰는지 확인하기 위한 이벤트 발동 시스템
    public delegate void SkillCastHandler(string skillName);
    
    public static SkillCastHandler OnSkillCast;

    [Header("마력 시스템")]
    [SerializeField] private ManaSystem manaSystem;

    public float skillQCost = 20f;
    public float skillWCost = 30f;
    public float skillECost = 40f;
    public float MaxMana => maxMana;
    public float CurrentMana => currentMana;
    private bool isDepleted = false;

    private PlayerAnimationSync sync;

    private void Awake()
    {
        sync = GetComponent<PlayerAnimationSync>();
    }

    void Start()
    {
        currentMana = maxMana;
        StartCoroutine(RegenerateMana());
    }

    void Update()
    {

    }

    public bool ConsumeMana(float amount)
    {
        if (currentMana < amount)
        {
            isDepleted = true;
            return false;
        }

        currentMana -= amount;
        isDepleted = false;
        return true;
    }

    IEnumerator RegenerateMana()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            if (currentMana < maxMana)
            {
                currentMana += regenRate;
                currentMana = Mathf.Min(currentMana, maxMana);
            }
        }
    }

    public bool IsDepleted() => isDepleted;

    public void SetMana(float value)
    {
        currentMana = Mathf.Clamp(value, 0, maxMana);
    }
    
    public void Q_Skill()
    {
        if(currentMana < skillQCost)
        {
            Debug.Log("Q스킬 마나 부족");
            return;
        }
        ConsumeMana(skillQCost);
        sync.DownCommand();
        Debug.Log("[Skill] Q 스킬 발동!");
        OnSkillCast?.Invoke("Q");
        QskillUI.StartQCooldown();
    }
    public void W_Skill()
    {
        if (currentMana < skillWCost)
        {
            Debug.Log("W스킬 마나 부족");
            return;
        }
        ConsumeMana(skillWCost);
        sync.SideCommand();
        Debug.Log("[Skill] W 스킬 발동!");
        OnSkillCast?.Invoke("W");
        WskillUI.StartWCooldown();
    }
    public void E_Skill()
    {
        if (currentMana < skillECost)
        {
            Debug.Log("E스킬 마나 부족");
            return;
        }
        ConsumeMana(skillECost);
        //sync.마나 스킬 메소드 
        Debug.Log("[Skill] E 스킬 발동!");
        OnSkillCast?.Invoke("E");
    }
}
