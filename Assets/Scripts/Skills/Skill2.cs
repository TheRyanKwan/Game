// Assets/Scripts/Skills/Skill2.cs
using UnityEngine;

public class Skill2 : BaseSkill
{
    private Transform cachedCasterTransform;

    public Skill2()
    {
        skillName = "Teleport";
        description = "Instantly teleport forward to the furthest valid position.";

        maxCooldownPool = 10f;
        cooldownCostPerCast = 10f;
        cooldownRegenRate = 1f;

        castTime = 0f;
        baseDamage = 0f;

        currentCooldownPool = maxCooldownPool;
    }

    public override void Cast(Transform casterTransform)
    {
        if (!IsReady) return;

        cachedCasterTransform = casterTransform;
        base.Cast(casterTransform); // Always fires — deducts pool, sets isCasting, fires events
    }

    public override void OnCastComplete()
    {
        base.OnCastComplete(); // Sets isCasting = false, clears action lock

        if (cachedCasterTransform == null)
        {
            Debug.LogWarning("Skill2: Caster transform is missing.");
            return;
        }

        PlayerTeleport teleport = cachedCasterTransform.GetComponent<PlayerTeleport>();
        if (teleport == null)
        {
            Debug.LogWarning("Skill2: No PlayerTeleport component found on caster.");
            return;
        }

        Vector3? destination = teleport.FindTeleportPosition();
        if (destination.HasValue)
        {
            teleport.ExecuteTeleport(destination.Value);
        }
        else
        {
            Debug.Log("Skill2: No valid teleport destination found.");
            // No freeze — isCasting is already false from base.OnCastComplete()
        }

        cachedCasterTransform = null;
    }
}
