// Assets/Scripts/Level/Skills/Decorators/FireballTrackingDecorator.cs
using UnityEngine;

/// <summary>
/// 火球追蹤升級（Skill1 專屬）
/// </summary>
public class FireballTrackingDecorator : SkillDecorator
{
    private float trackingStrength;
    private float trackingDuration;

    public FireballTrackingDecorator(BaseSkill skill, float strength = 0.8f, float duration = 3f) : base(skill)
    {
        BaseSkill baseSkill = GetBaseSkill();
        if (baseSkill is not Skill1)
        {
            Debug.LogError("FireballTrackingDecorator can only be applied to Skill1!");
        }

        trackingStrength = strength;
        trackingDuration = duration;
        Debug.Log($"Applied Fireball Tracking: Strength={strength}, Duration={duration}");
    }

    public override void OnCastComplete()
    {
        base.OnCastComplete();
        
        Debug.Log($"Spawning Tracking Fireball! Strength: {trackingStrength}, Duration: {trackingDuration}");
        
        // 實際實作：生成帶追蹤功能的火球
        // GameObject fireball = Object.Instantiate(fireballPrefab);
        // var tracker = fireball.AddComponent<ProjectileTracker>();
        // tracker.Initialize(trackingStrength, trackingDuration);
    }

    public float TrackingStrength => trackingStrength;
    public float TrackingDuration => trackingDuration;
}
