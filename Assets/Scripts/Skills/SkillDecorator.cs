// Assets/Scripts/Level/Skills/SkillDecorator.cs
using UnityEngine;

/// <summary>
/// 技能 Decorator 基類，通過屬性覆寫來修改技能行為
/// </summary>
public abstract class SkillDecorator : BaseSkill
{
    protected BaseSkill wrappedSkill;

    public SkillDecorator(BaseSkill skill)
    {
        wrappedSkill = skill;
    }

    // 委派方法到被包裝的技能
    public override void Cast(Transform casterTransform)
    {
        wrappedSkill.Cast(casterTransform);
    }

    public override void UpdateSkill()
    {
        wrappedSkill.UpdateSkill();
    }

    public override void OnCastComplete()
    {
        wrappedSkill.OnCastComplete();
    }

    public override void ResetCooldown()
    {
        wrappedSkill.ResetCooldown();
    }

    // 默認行為：直接返回被包裝技能的屬性
    public override float CurrentCooldownPool => wrappedSkill.CurrentCooldownPool;
    public override float MaxCooldownPool => wrappedSkill.MaxCooldownPool;
    public override float CooldownCostPerCast => wrappedSkill.CooldownCostPerCast;
    public override float CooldownRegenRate => wrappedSkill.CooldownRegenRate;
    public override float CastTime => wrappedSkill.CastTime;
    public override float BaseDamage => wrappedSkill.BaseDamage;
    public override bool IsCasting => wrappedSkill.IsCasting;
    public override bool IsReady => wrappedSkill.IsReady;
    public override int AvailableCasts => wrappedSkill.AvailableCasts;

    /// <summary>
    /// 獲取最底層的原始技能
    /// </summary>
    public BaseSkill GetBaseSkill()
    {
        BaseSkill current = wrappedSkill;
        while (current is SkillDecorator decorator)
        {
            current = decorator.wrappedSkill;
        }
        return current;
    }

    public BaseSkill GetWrappedSkill()
    {
        return wrappedSkill;
    }
}
