// Assets/Scripts/Enemy/BulletHellEnemy.cs
using UnityEngine;

public class BulletHellEnemy : BaseEnemy
{
    [Header("彈幕設定")]
    [SerializeField] private float bulletInterval = 0.5f;
    [SerializeField] private int bulletsPerCard = 12;
    [SerializeField] private float bulletSpeed = 10f;
    
    private float nextBulletTime = 0f;
    private int bulletsFired = 0;
    
    protected override void ExecuteSpellCard()
    {
        if (Time.time >= nextBulletTime)
        {
            FireAdvancedBullet();
            bulletsFired++;
            nextBulletTime = Time.time + bulletInterval;
        }
    }
    
    protected override bool IsSpellCardFinished()
    {
        return bulletsFired >= bulletsPerCard;
    }
    
    protected override void OnAttackEnd()
    {
        base.OnAttackEnd();
        bulletsFired = 0;
        nextBulletTime = Time.time;
    }
    
    private void FireSpellCardBullet()
    {
        if (playerTransform == null)
            return;
        
        Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
        float randomAngle = Random.Range(-5f, 5f);
        Vector3 finalDirection = Quaternion.AngleAxis(randomAngle, Vector3.up) * directionToPlayer;
        
        // 簡單直線彈幕
        BulletManager.Instance.SpawnBullet(
            transform.position,
            new LinearMovementBehavior(finalDirection, bulletSpeed)
        );
    }
    
    // 範例：複雜彈幕——加速彈幕
    private void FireAcceleratingBullet()
    {
        Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
        
        BulletManager.Instance.SpawnBullet(
            transform.position,
            new LinearMovementBehavior(directionToPlayer, 2f),
            new AccelerationBehavior(directionToPlayer * 15f, 30f)
        );
    }
    
    // 範例：螺旋彈幕——無模版
    private void FireSpiralBullet()
    {
        Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
        
        // 定義基礎運動 + 旋轉偏移
        BulletManager.Instance.SpawnBullet(
            transform.position,
            new LinearMovementBehavior(directionToPlayer, 20f),
            new SineOffsetBehavior(
                directionToPlayer * 20f,
                Vector3.up,
                10f,        // 2 Hz 頻率
                15f         // 3 單位振幅
            )
        );
    }
    
    // 範例：追蹤彈幕
    private void FireTrackingBullet()
    {
        BulletManager.Instance.SpawnBullet(
            transform.position,
            new TrackingBehavior(playerTransform, 7f, 180f)
        );
    }
    
    // 範例：在 2 秒後加速的彈幕
    private void FireDelayedAccelerationBullet()
    {
        Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
        
        BulletManager.Instance.SpawnBullet(
            transform.position,
            new LinearMovementBehavior(directionToPlayer, 15f),
            new TimedBehavior(
                new AccelerationBehavior(directionToPlayer * 100f),
                2f  // 2 秒後停止加速
            )
        );
    }
    
    // 範例：自定義組合——旋轉 + 加速 + 距離限制
    private void FireAdvancedBullet()
    {
        Vector3 startPos = transform.position;
        Vector3 directionToPlayer = (playerTransform.position - startPos).normalized;
        
        BulletManager.Instance.SpawnBullet(
            startPos,
            new LinearMovementBehavior(directionToPlayer, 6f),
            new RotationBehavior(Vector3.up, 180f),           // 每秒旋轉 180 度
            new AccelerationBehavior(directionToPlayer * 2f), // 加速
            new ConditionalEndBehavior(bullet =>              // 距離超過 30 單位時結束
            {
                float distFromStart = Vector3.Distance(bullet.position, startPos);
                return distFromStart > 30f;
            })
        );
    }
}
