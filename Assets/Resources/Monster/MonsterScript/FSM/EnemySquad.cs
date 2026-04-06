using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemySquad : MonoBehaviour
{
    private EnemyAI _enemyAI;
    private LayerMask _enemyLayerMask;

    [Header("무리 정보")]
    public EnemyAI currentLeader = null;
    public float allyCourageBonus = 0f;
    public int mySquadRank = 0;
    public bool justAlerted = false; // 경보 방패

    private float courageCheckTimer = 0f;
    private Coroutine alertShieldCoroutine = null;

    private void Awake()
    {
        _enemyAI = GetComponent<EnemyAI>();
        _enemyLayerMask = LayerMask.GetMask("Enemy");
    }

    private void Update()
    {
        if (_enemyAI.speciesData == null || _enemyAI._enemyHealth.isDeath) return;

        courageCheckTimer += Time.deltaTime;
        if (courageCheckTimer >= 0.5f)
        {
            UpdateCourageBonus();
            courageCheckTimer = 0f;
        }
    }

    public void UpdateCourageBonus()
    {
        int allyCount = 0;
        this.currentLeader = null;

        Collider2D[] allies = Physics2D.OverlapCircleAll(transform.position, _enemyAI.speciesData.allyCheckRadius, _enemyLayerMask);

        foreach (var col in allies)
        {
            if (col.gameObject != this.gameObject && col.TryGetComponent<EnemyAI>(out EnemyAI allyAI))
            {
                allyCount++;
                if (allyAI.speciesData.isLeader && !allyAI._enemyHealth.isDeath) this.currentLeader = allyAI;
            }
        }

        allyCount = Mathf.Min(allyCount, (int)_enemyAI.speciesData.MaxPackBonusCount);
        this.allyCourageBonus = allyCount * _enemyAI.speciesData.courageBonusPerAlly;

        float bonusSpeed = allyCount * _enemyAI.speciesData.speedBonusPerAlly;
        _enemyAI.currentMoveSpeed = _enemyAI.speciesData.chaseSpeed + bonusSpeed;

        if (currentLeader != null && currentLeader._enemyHealth.isBerserkMode)
        {
            _enemyAI.currentMoveSpeed *= currentLeader.speciesData.berserkSpeedMultiplier;
            _enemyAI.currentCooldown *= 0.5f;
        }

        float reduction = allyCount * _enemyAI.speciesData.cooldownReductionPerAlly;
        _enemyAI.currentCooldown = Mathf.Max(0.5f, _enemyAI.speciesData.attackCooldown - reduction);
    }

    public void AlertNearbyAllies(bool isChainReaction = false)
    {
        if (isChainReaction && currentLeader == null && !_enemyAI.speciesData.isLeader) return;

        Collider2D[] allies = Physics2D.OverlapCircleAll(transform.position, _enemyAI.speciesData.alertRadius, _enemyLayerMask);
        Vector3 playerPos = _enemyAI.player;

        foreach (var col in allies)
        {
            if (col.gameObject == gameObject) continue;
            if (col.TryGetComponent<EnemyAI>(out EnemyAI allyAI)) allyAI._enemySquad.Alert(playerPos);
        }
    }

    public void Alert(Vector3 playerPosition)
    {
        if (!(_enemyAI.currentState is IdleState)) return;

        _enemyAI.player = playerPosition;
        if (alertShieldCoroutine != null) StopCoroutine(alertShieldCoroutine);
        alertShieldCoroutine = StartCoroutine(AlertShieldCoroutine(3.0f));
        _enemyAI.ChangeState(_enemyAI.chaseState);
    }

    private IEnumerator AlertShieldCoroutine(float duration)
    {
        this.justAlerted = true;
        yield return new WaitForSeconds(duration);
        this.justAlerted = false;
    }

    public void RecalculateSquadRank()
    {
        if (_enemyAI.player == null) return;

        Collider2D[] colliders = Physics2D.OverlapCircleAll(_enemyAI.player, _enemyAI.speciesData.alertRadius, _enemyLayerMask);
        List<EnemyAI> squad = new List<EnemyAI>();

        foreach (var col in colliders)
        {
            if (col.TryGetComponent<EnemyAI>(out EnemyAI ai))
            {
                if (ai._enemyHealth.isDeath || !ai.gameObject.activeInHierarchy) continue;
                squad.Add(ai);
            }
        }

        squad.Sort((a, b) =>
        {
            float distA = Vector2.Distance(a.transform.position, _enemyAI.player);
            float distB = Vector2.Distance(b.transform.position, _enemyAI.player);

            if (Mathf.Abs(distA - distB) < 2.0f) return a.GetInstanceID().CompareTo(b.GetInstanceID());
            return distA.CompareTo(distB);
        });

        mySquadRank = squad.IndexOf(_enemyAI);
    }

    public Vector3 GetFlankingTargetPos()
    {
        if (_enemyAI.player == null) return transform.position;

        float fixedSide = (mySquadRank % 2 == 0) ? -1f : 1f;
        int rowNumber = mySquadRank / 2;
        float finalSpacing = _enemyAI.speciesData.flankSpacing + (rowNumber * 0.5f);

        return new Vector3(_enemyAI.player.x + (fixedSide * finalSpacing), _enemyAI.player.y, _enemyAI.player.z);
    }

    public void NotifyLeaderDeath()
    {
        Collider2D[] followers = Physics2D.OverlapCircleAll(transform.position, _enemyAI.speciesData.commandRadius, _enemyLayerMask);
        foreach (var col in followers)
        {
            if (col.gameObject != gameObject && col.TryGetComponent<EnemyAI>(out EnemyAI follower))
            {
                follower._enemySquad.OnLeaderDied();
            }
        }
    }

    public void OnLeaderDied()
    {
        if (_enemyAI._enemyHealth.isDeath) return;
        currentLeader = null;
        _enemyAI._enemyHealth.currentCourage = 0;
        _enemyAI.ChangeState(_enemyAI.fleeState);
    }

    public void ActivateBerserk()
    {
        _enemyAI._enemyHealth.isBerserkMode = true;
        Collider2D[] followers = Physics2D.OverlapCircleAll(transform.position, _enemyAI.speciesData.commandRadius, _enemyLayerMask);
        foreach (var col in followers)
        {
            if (col.TryGetComponent<EnemyAI>(out EnemyAI minion))
            {
                minion.GetAnimator().speed = 2.0f;
            }
        }
    }
}