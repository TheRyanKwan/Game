// Assets/Scripts/Level/Skills/Decorators/MaxPoolUpgradeDecorator.cs
using UnityEngine;

/// <summary>
/// 增加冷卻池最大容量
/// </summary>
public class MaxPoolUpgradeDecorator : SkillDecorator
{
    private float poolBonus;

    public MaxPoolUpgradeDecorator(BaseSkill skill, float bonus) : base(skill)
    {
        poolBonus = bonus;
        Debug.Log($"Applied MaxPool upgrade: +{bonus}. New max: {MaxCooldownPool}");
    }

    // 覆寫屬性，直接返回修改後的值
    public override float MaxCooldownPool => wrappedSkill.MaxCooldownPool + poolBonus;
}