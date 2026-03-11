using UnityEngine;

/// <summary>
/// 데미지 가능 대상이 구현해야 할 공통 계약
/// </summary>
public interface IDamageable
{
    /// <param name="damage">데미지량</param>
    /// <param name="source">데미지 발생원 위치</param>
    /// <param name="knockbackForce">수평 넉백 세기</param>
    /// <param name="upwardForce">수직 넉백 세기</param>
    void TakeDamage(int damage, Transform source, float knockbackForce, float upwardForce);
}