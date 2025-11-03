using Unity.VisualScripting;
using UnityEngine;

public class SoulBuffInvincibility
{
    public static bool Defence6Clicked = false;
    public static bool immortalOnceUsed = false;

    public Die PlayerDie;
    public static void DButton6Clicked()
    {
        Defence6Clicked = true;
    }

    public static void UseImmortalOnce()
    {
        immortalOnceUsed = true;
    }

    public static void ResetImmortalOnce()
    {
        immortalOnceUsed = false;
        Debug.Log($"»ç¸Á ½ÇÇàµÊ{immortalOnceUsed}");
    }

}
