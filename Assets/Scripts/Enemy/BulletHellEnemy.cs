// BulletHellEnemy.cs
using UnityEngine;

/// <summary>
/// 彈幕敵人：12 發彈幕（每 0.5 秒一發），然後走位。
/// 完全由派生類代碼決定攻擊流程與何時結束。
/// </summary>
public class BulletHellEnemy : BaseEnemy
{
    [Header("彈幕設定")]
    [SerializeField] private float bulletInterval = 0.5f;
    [SerializeField] private int bulletsPerCard = 12;
    [SerializeField] private float maxAngleDeviation = 5f;
    [SerializeField] private float bulletSpeed = 10f;
    [SerializeField] private float bulletLifetime = 5f;

    private float nextBulletTime = 0f;
    private int bulletsFired = 0;

    protected override void ExecuteSpellCard()
    {
        // 檢查是否到達發射時機
        if (Time.time >= nextBulletTime)
        {
            FireSingleBullet();
            bulletsFired++;
            nextBulletTime = Time.time + bulletInterval;
        }
    }

    /// <summary>
    /// 決定符卡何時結束：所有彈幕發完即為結束。
    /// </summary>
    protected override bool IsSpellCardFinished()
    {
        return bulletsFired >= bulletsPerCard;
    }

    /// <summary>
    /// 攻擊結束，重置計數器。
    /// </summary>
    protected override void OnAttackEnd()
    {
        base.OnAttackEnd();
        bulletsFired = 0;
        nextBulletTime = Time.time;  // 下次符卡時重新開始計時
    }

    private void FireSingleBullet()
    {
        if (playerTransform == null)
            return;

        Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
        float randomAngle = Random.Range(-maxAngleDeviation, maxAngleDeviation);
        Vector3 finalDirection = Quaternion.AngleAxis(randomAngle, Vector3.up) * directionToPlayer;

        if (BulletManager.Instance != null)
        {
            BulletManager.Instance.SpawnBullet(
                startPosition: transform.position,
                direction: finalDirection,
                speed: bulletSpeed,
                lifetime: bulletLifetime
            );
        }

        Debug.Log($"{gameObject.name} 發射第 {bulletsFired}/{bulletsPerCard} 枚彈幕");
    }

    protected override void OnStateChange(EnemyState newState)
    {
        base.OnStateChange(newState);
        switch (newState)
        {
            case EnemyState.Idle:
                Debug.Log($"{gameObject.name} -> 待機");
                break;
            case EnemyState.Attacking:
                Debug.Log($"{gameObject.name} -> 攻擊");
                break;
            case EnemyState.Evading:
                Debug.Log($"{gameObject.name} -> 走位躲避");
                break;
            case EnemyState.Dead:
                Debug.Log($"{gameObject.name} -> 死亡");
                break;
        }
    }
}
