// Assets/Scripts/Bullet/IBulletBehavior.cs
public interface IBulletBehavior
{
    /// <summary>
    /// 每幀更新子彈行為。返回 true 表示行為繼續，false 表示行為完成。
    /// </summary>
    bool Update(Bullet bullet, float deltaTime);
    
    /// <summary>
    /// 行為初始化時調用（可選）。
    /// </summary>
    void Initialize(Bullet bullet);
    
    /// <summary>
    /// 行為結束時調用（可選）。
    /// </summary>
    void OnBehaviorEnd(Bullet bullet);
}
