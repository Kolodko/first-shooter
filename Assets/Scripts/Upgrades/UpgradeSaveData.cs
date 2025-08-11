using System;
using System.Collections.Generic;

[Serializable]
public class UpgradeSaveData
{
    public int AvailablePoints = 0;
    public List<StatLevelData> StatLevels = new List<StatLevelData>();
}

[Serializable]
public class StatLevelData
{
    public StatType Type;
    public int Level;
    
    public StatLevelData(StatType type, int level)
    {
        Type = type;
        Level = level;
    }
}