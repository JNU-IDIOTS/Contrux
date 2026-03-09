using UnityEngine;

[DisallowMultipleComponent]
public class EnemyHealthBarSettings : MonoBehaviour
{
    [Header("Health Bar Offset")]
    [Tooltip("체력바가 이 Enemy 위에서 표시될 Offset")]
    public Vector3 healthBarOffset = new Vector3(0, 1.5f, 0);
}
