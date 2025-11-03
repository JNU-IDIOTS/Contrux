using UnityEngine;

public class CustomGravity : MonoBehaviour
{
    
    public float gravity = -30f; // 중력 가속도
    public float maxFallSpeed = -20f; // 낙하 속도 제한

    public Rigidbody2D rb;
    private Ground ground;
    private Player_move move;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        ground = GetComponent<Ground>();
        move = GetComponent<Player_move>();
    }

    private void FixedUpdate()
    {
        if (move.dash.IsDashing)
            return;
        
        // 수직속도 감소 (중력 가속)
        if (!ground.isGrounded) // 바닥에 있을 때는 적용하지 않음
        {
            float newYVelocity = rb.linearVelocity.y + gravity * Time.fixedDeltaTime;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Max(newYVelocity, maxFallSpeed));
        }
    }
        
}