// Assets/Scripts/Level/Skills/Decorators/DamageUpgradeDecorator.cs
using UnityEngine;

/// <summary>
/// 增加技能傷害
/// </summary>
public class DamageUpgradeDecorator : SkillDecorator
{
    private float damageBonus;

    public DamageUpgradeDecorator(BaseSkill skill, float bonus) : base(skill)
    {
        damageBonus = bonus;
        Debug.Log($"Applied Damage upgrade: +{bonus}. New damage: {BaseDamage}");
    }

    public override float BaseDamage => wrappedSkill.BaseDamage + damageBonus;
}