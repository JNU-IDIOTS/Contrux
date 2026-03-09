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
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                Debug.Log("버스터한테 맞음");
                playerHealth.TakeDamage(chargeDamage);
            }
            // TODO: 넉백 기능은 나중에 구현
            // playerKnockback.ApplyKnockback(transform, BusterknockbackForce, BusterknockbackUpForce);
        }
    }
    
    
}
