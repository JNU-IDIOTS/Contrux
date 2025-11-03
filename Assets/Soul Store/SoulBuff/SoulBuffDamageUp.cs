
public class SoulBuffDamageUp
{
    public float DamageUP;
    public static bool buttonClicked = false;
    EnemyAI enemyAI;
    

    
    public static void MaxHpDamageUpReady()
    {
        buttonClicked = true;   
    }
    public static void MaxHpDamageUp()
    {
        if(SoulBuffManager.IsButtonUnlocked("attack3") && buttonClicked)
        {
            float HPPercent = 0.05f;
            Attack_Damage.MaxHPDamage = ScanMaxHP.detectedMaxHP * HPPercent;
            
        }
    }
    
}
