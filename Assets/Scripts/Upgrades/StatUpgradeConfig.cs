using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class StatUpgradeConfig
{
    public StatType Type;
    public float BaseValue;
    public float UpgradeIncrement;
    public float MaxValue;
    public string LocalizationKey;
    public string IconPath;
}
