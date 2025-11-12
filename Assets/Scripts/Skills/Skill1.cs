// Assets/Scripts/Level/Skills/Skill1.cs (Skill2 3 4 are similar)
using UnityEngine;

public class Skill1 : BaseSkill
{
    public Skill1()
    {
        skillName = "Fireball";
        description = "Launch a fireball at enemies";
        
        maxCooldownPool = 40f;
        cooldownCostPerCast = 10f;
        cooldownRegenRate = 1f;
        
        castTime = 1f;
        baseDamage = 10f;
        
        currentCooldownPool = maxCooldownPool;
    }

    public override void Cast(Transform casterTransform)
    {
        base.Cast(casterTransform);
    }

    public override void OnCastComplete()
    {
        base.OnCastComplete();
        
        Debug.Log("Fireball Effect Applied!");
        // Add your skill effect logic here
    }
}
