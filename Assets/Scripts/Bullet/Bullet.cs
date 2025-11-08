// Assets/Scripts/Bullet/Bullet.cs
using UnityEngine;
using System.Collections.Generic;

public class Bullet : MonoBehaviour
{
    [Header("基礎屬性")]
    public Vector3 velocity = Vector3.zero;
    public Vector3 position;
    public Rigidbody rb;
    
    [Header("生存時間")]
    public float lifetime = 5f;
    private float elapsedTime = 0f;
    
    // 行為系統
    private List<IBulletBehavior> behaviors = new List<IBulletBehavior>();
    private List<IBulletBehavior> behaviorToRemove = new List<IBulletBehavior>();
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = false;
            rb.useGravity = false;
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }
    }
    
    private void Update()
    {
        elapsedTime += Time.deltaTime;
        position = transform.position;
        
        // 更新所有行為
        UpdateBehaviors(Time.deltaTime);
        
        // 應用速度
        rb.velocity = velocity;
        
        // 檢查生存時間
        if (elapsedTime >= lifetime)
        {
            Destroy(gameObject);
        }
    }
    
    private void UpdateBehaviors(float deltaTime)
    {
        behaviorToRemove.Clear();
        
        foreach (var behavior in behaviors)
        {
            bool shouldContinue = behavior.Update(this, deltaTime);
            if (!shouldContinue)
            {
                behavior.OnBehaviorEnd(this);
                behaviorToRemove.Add(behavior);
            }
        }
        
        // 移除已完成的行為
        foreach (var behavior in behaviorToRemove)
        {
            behaviors.Remove(behavior);
        }
    }
    
    /// <summary>
    /// 添加行為到子彈。
    /// </summary>
    public void AddBehavior(IBulletBehavior behavior)
    {
        behavior.Initialize(this);
        behaviors.Add(behavior);
    }
    
    /// <summary>
    /// 移除指定行為。
    /// </summary>
    public void RemoveBehavior(IBulletBehavior behavior)
    {
        behaviors.Remove(behavior);
    }
    
    /// <summary>
    /// 一次性添加多個行為。
    /// </summary>
    public void AddBehaviors(params IBulletBehavior[] behaviorArray)
    {
        foreach (var behavior in behaviorArray)
        {
            AddBehavior(behavior);
        }
    }
    
    /// <summary>
    /// 清空所有行為。
    /// </summary>
    public void ClearBehaviors()
    {
        behaviors.Clear();
    }
}
