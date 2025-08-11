using UniRx;

namespace Game.Upgrades
{
    public interface IUpgradeService
    {
        IReadOnlyReactiveProperty<int> AvailablePoints { get; }
        IReadOnlyReactiveDictionary<StatType, int> CurrentLevels { get; }
        IReadOnlyReactiveDictionary<StatType, int> PendingLevels { get; }
        
        void AddUpgradePoint();
        void IncreasePendingLevel(StatType statType);
        void ResetPendingUpgrades();
        void ApplyPendingUpgrades();
        float GetStatValue(StatType statType);
        StatUpgradeConfig GetStatConfig(StatType statType);
        bool CanUpgrade(StatType statType);
    }
}
