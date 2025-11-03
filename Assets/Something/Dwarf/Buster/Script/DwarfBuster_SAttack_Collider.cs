using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using System.Collections;

public class DwarfBuster_SAttack_Collider : MonoBehaviour
{
    public int chargeDamage = 50;
    private bool isActive = false;
    private PlayerHealth player;
    [Header("넉백")]
    [SerializeField] public float BusterknockbackForce = 0.5f;
    [SerializeField] public float BusterknockbackUpForce = 1f;
    public bool isplayerhit = false;

    public void Activate() => isActive = true;
    public void Deactivate() => isActive = false;
    public bool IsBusterKnockback = false;
  
    private void OnTriggerEnter2D(Collider2D other)
    { 
        if (other.CompareTag("Player"))
        {
            PlayerHealth player = other.GetComponent<PlayerHealth>();
            if (player != null)
            {
                Debug.Log("버스터한테 맞음");
                player.TakeDamage(chargeDamage,transform, BusterknockbackForce, BusterknockbackUpForce);
            }
        }
    }
    
}
