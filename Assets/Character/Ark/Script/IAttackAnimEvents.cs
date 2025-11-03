public interface IAttackAnimEvents
{
    void OnStep(int index);     // 1타/2타 등
    void OnAttackEnd();
    void OnSkillStart();        // 필요시
    void OnSkillEnd();
}
