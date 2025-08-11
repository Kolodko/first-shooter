using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UpgradeSettings", menuName = "Game/UpgradeSettings")]
public class UpgradeSettings : ScriptableObject
{
    public List<StatUpgradeConfig> StatConfigs = new List<StatUpgradeConfig>
    {
        new StatUpgradeConfig 
        { 
            Type = StatType.Speed, 
            BaseValue = 5f, 
            UpgradeIncrement = 0.25f,
            MaxValue = 10f,
            LocalizationKey = "stat_speed"
        },
        new StatUpgradeConfig 
        { 
            Type = StatType.Health, 
            BaseValue = 100f, 
            UpgradeIncrement = 10f,
            MaxValue = 300f,
            LocalizationKey = "stat_health"
        },
        new StatUpgradeConfig 
        { 
            Type = StatType.Damage, 
            BaseValue = 10f, 
            UpgradeIncrement = 2f,
            MaxValue = 50f,
            LocalizationKey = "stat_damage"
        }
    };
}
