using UnityEngine;
using UnityEngine.Tilemaps;

public class GlowPulse : MonoBehaviour
{
    public SpriteRenderer glowSpriteRenderer;
    public TilemapRenderer glowTilemapRenderer;
    public float pulseSpeed = 2f;
    public float minAlpha = 0.3f;
    public float maxAlpha = 0.6f;

    private Material tilemapMaterialInstance;

    void Start()
    {
        // Tilemap은 공유 머티리얼을 사용하므로 인스턴스 복제
        if (glowTilemapRenderer != null)
        {
            tilemapMaterialInstance = new Material(glowTilemapRenderer.material);
            glowTilemapRenderer.material = tilemapMaterialInstance;
        }
    }

    void Update()
    {
        float alpha = Mathf.Lerp(minAlpha, maxAlpha, (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f);

        if (glowSpriteRenderer != null)
        {
            Color c = glowSpriteRenderer.color;
            c.a = alpha;
            glowSpriteRenderer.color = c;
        }

        if (glowTilemapRenderer != null && tilemapMaterialInstance != null)
        {
            Color c = tilemapMaterialInstance.color;
            c.a = alpha;
            tilemapMaterialInstance.color = c;
        }
    }
}
