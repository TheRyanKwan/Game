// Assets/Scripts/Level/Skills/SkillUpgradeManager.cs
using UnityEngine;
using System.Collections.Generic;

public class SkillUpgradeManager : MonoBehaviour
{
    public static SkillUpgradeManager Instance { get; private set; }

    private Dictionary<int, List<string>> appliedUpgrades = new Dictionary<int, List<string>>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        for (int i = 0; i < 4; i++)
        {
            appliedUpgrades[i] = new List<string>();
        }
    }

    /// <summary>
    /// 應用通用升級
    /// </summary>
    public BaseSkill ApplyUpgrade(BaseSkill skill, string upgradeType, float value)
    {
        BaseSkill upgradedSkill = upgradeType switch
        {
            "IncreaseMaxPool" => new MaxPoolUpgradeDecorator(skill, value),
            "IncreaseDamage" => new DamageUpgradeDecorator(skill, value),
            _ => skill
        };

        return upgradedSkill;
    }

    /// <summary>
    /// 應用Skill1火球特殊升級 (范例)
    /// </summary>
    public BaseSkill ApplyFireballUpgrade(BaseSkill skill, string upgradeType)
    {
        BaseSkill baseSkill = skill is SkillDecorator decorator ? decorator.GetBaseSkill() : skill;
        
        if (baseSkill is not Skill1)
        {
            Debug.LogError("Cannot apply Fireball upgrade to non-Fireball skill!");
            return skill;
        }

        return upgradeType switch
        {
            "Tracking" => new FireballTrackingDecorator(skill),
            _ => skill
        };
    }

    public bool HasUpgrade(int skillIndex, string upgradeType)
    {
        return appliedUpgrades.ContainsKey(skillIndex) && 
               appliedUpgrades[skillIndex].Contains(upgradeType);
    }

    public void RecordUpgrade(int skillIndex, string upgradeType)
    {
        if (!appliedUpgrades.ContainsKey(skillIndex))
        {
            appliedUpgrades[skillIndex] = new List<string>();
        }
        
        if (!appliedUpgrades[skillIndex].Contains(upgradeType))
        {
            appliedUpgrades[skillIndex].Add(upgradeType);
        }
    }

    public List<string> GetAppliedUpgrades(int skillIndex)
    {
        return appliedUpgrades.ContainsKey(skillIndex) ? 
            new List<string>(appliedUpgrades[skillIndex]) : 
            new List<string>();
    }
}
