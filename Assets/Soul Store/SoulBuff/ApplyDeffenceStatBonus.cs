using System.Diagnostics;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;
using Unity.VisualScripting;

public class ApplyDeffenceStatBonus
{
    private MonoBehaviour mono;
    private HpGenerate hp;
    //체력 ui
    public HealthBarUIController hpUI;  // 인스펙터에서 직접 연결할 것
    public static bool MaxHpChange = false;

    public void Init()
    {
        mono = mono.GetComponent<MonoBehaviour>();
    }

    public static float heal = 0;
    public static void applyDeffenceStatBonus()
    {
        UnityEngine.Debug.Log($"버프 전 최대 체력 : {PlayerHealth.maxHP}");
        UnityEngine.Debug.Log("[applyDeffenceStatBonus] 함수 호출됨");
        if (SoulBuffManager.IsButtonUnlocked("deffence1"))
        {
            UnityEngine.Debug.Log("deffence1번 눌려서 실행됨");
            PlayerHealth.maxHP = 200;
            PlayerHealth.currentHP = PlayerHealth.maxHP;
            MaxHpChange = true;
            UnityEngine.Debug.Log($"버프 후 최대 체력 : {PlayerHealth.maxHP}");
            var ui = UnityEngine.Object.FindFirstObjectByType<HealthBarUIController>();
            ui?.RefreshFromPlayerHealth();
            SaveManager.Instance.SaveNow();
        }
    }

    public void MaxHpChangeNow()
    {
        if (MaxHpChange)
        {
            if (hpUI != null)
            {
                hpUI.SetHP((int)PlayerHealth.currentHP, (int)PlayerHealth.maxHP);
                MaxHpChange = false;

            }
        }
        else
        {
            UnityEngine.Debug.Log("최대체력의 변화값이 없습니다.");
            return;
        }

    }
}

