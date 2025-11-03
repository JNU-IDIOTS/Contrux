using UnityEngine;

public class Sandworm_Animation : MonoBehaviour
{
    public Animator ani;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ani = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AttackPrepare()
    {
        ani.SetTrigger("attackPrepare");
    }

    public void Attack()
    {
        ani.SetTrigger("attack");
    }
    
    public void Jump()
    {
        ani.SetTrigger("jump");
    }

    public void JumpRoll()
    {
        ani.SetTrigger("jumpRoll");
    }

    public void JumpEnd()
    {
        ani.SetTrigger("jumpEnd");
    }

    public void HalfJump()
    {
        ani.SetTrigger("HalfJump");
    }

    public void Jumpoo()
    {
        ani.SetTrigger("Jumpoo");
    }

    public void Spit()
    {
        ani.SetTrigger("spit");
    }
    
    public void SpitEnd()
    {
        ani.SetTrigger("spitEnd");
    }

    public void HalfJump2()
    {
        ani.SetTrigger("halfJump2");
    }
}
