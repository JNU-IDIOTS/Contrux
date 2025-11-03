using UnityEngine;

[System.Serializable]
public class BossSkill
{
    public string skillName;
    public System.Action skillAction;
    public float cooldown;
    public float lastUsedTime;  // ← '캐스트 종료 시'에 갱신
    public float castTime;

    public bool IsReady()
    {
        return Time.time >= lastUsedTime + cooldown;
    }

    public void Use()
    {
        skillAction?.Invoke();
        // lastUsedTime 은 여기서 갱신하지 않는다!
    }
}
