using UnityEngine;

public class HpGenerate
{
    public static bool isRegenerating = false;
    public static void HealCoroutine()
    {
        
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) return;
        PlayerHealth health = player.GetComponent<PlayerHealth>();
        if (health != null)
        {
            isRegenerating = true;
            PlayerHealth.healbuttonclicked = true;
        }
    }
}