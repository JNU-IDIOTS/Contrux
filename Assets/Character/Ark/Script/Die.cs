using UnityEngine;
using UnityEngine.SceneManagement;

public class Die : MonoBehaviour
{
    public bool isDead = false;
    private PlayerHealth health;

    private void Awake()
    {
        health = GetComponent<PlayerHealth>();
    }

    public void die()
    {
        Debug.Log("죽음 함수");
        isDead = true;

        // 체력 감소 후 UI 갱신
        if (health.hpUI != null)
        {
            health.hpUI.SetHP((int)PlayerHealth.currentHP, (int)PlayerHealth.maxHP);
        }

        // ★ 현재 씬 다시 로드 (게임 처음 상태로)
        SceneManager.LoadScene("Tutorial");
    }
}

