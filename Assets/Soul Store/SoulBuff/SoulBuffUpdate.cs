using Unity.VisualScripting;
using UnityEngine;
using System.Collections;
using JetBrains.Annotations;
public class SoulBuffUpdate : MonoBehaviour
{
    private void Update()
    {
        if (NonDamage.DefenceBClicked == true)
        {
            StartCoroutine(NonDamage.InvokeEvery30Seconds());
        }
        if (SoulBuffShield.DefenceSClicked == true)
        {
            StartCoroutine(SoulBuffShield.ShieldEvery30Seconds());
        }
    }
}
