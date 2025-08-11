using System;
using System.Collections.Generic;
using System.Linq;
using Game.Installers;
using Game.Player;
using Game.Save;
using UniRx;
using UnityEngine;
using Zenject;

namespace Game.Upgrades
{
    public class UpgradeService : IUpgradeService, IInitializable, ISaveableService
    {
        [Inject] private UpgradeSettings _settings;
        [Inject] private IPlayerStatsService _playerStats;
        [Inject] private SignalBus _signalBus;
        [Inject] private ISaveService _saveService;
        
        private readonly ReactiveProperty<int> _availablePoints = new ReactiveProperty<int>(0);
        private readonly ReactiveDictionary<StatType, int> _currentLevels = new ReactiveDictionary<StatType, int>();
        private readonly ReactiveDictionary<StatType, int> _pendingLevels = new ReactiveDictionary<StatType, int>();
        
        private Dictionary<StatType, StatUpgradeConfig> _configCache;
        private int _pendingPointsUsed = 0;
        
        public IReadOnlyReactiveProperty<int> AvailablePoints => _availablePoints;
        public IReadOnlyReactiveDictionary<StatType, int> CurrentLevels => _currentLevels;
        public IReadOnlyReactiveDictionary<StatType, int> PendingLevels => _pendingLevels;

        public void Initialize()
        {
            InitializeConfigs();
            LoadSavedData();
            SubscribeToEvents();
        }

        private void InitializeConfigs()
        {
            _configCache = _settings.StatConfigs.ToDictionary(c => c.Type);
            
            foreach (var config in _settings.StatConfigs)
            {
                if (!_currentLevels.ContainsKey(config.Type))
                {
                    _currentLevels[config.Type] = 0;
                    _pendingLevels[config.Type] = 0;
                }
            }
        }

        private void SubscribeToEvents()
        {
            _signalBus.GetStream<GameInstaller.EnemyDeathSignal>()
                .Subscribe(_ => AddUpgradePoint());
        }

        public void AddUpgradePoint()
        {
            _availablePoints.Value++;
            _signalBus.Fire(new GameInstaller.UpgradePointEarnedSignal());
        }

        public void IncreasePendingLevel(StatType statType)
        {
            if (!CanUpgrade(statType)) 
                return;
            
            _pendingLevels[statType]++;
            _pendingPointsUsed++;
            
            _signalBus.Fire(new GameInstaller.PendingUpgradeChangedSignal { StatType = statType });
        }

        public bool CanUpgrade(StatType statType)
        {
            if (_availablePoints.Value - _pendingPointsUsed <= 0) 
                return false;
            
            var config = GetStatConfig(statType);
            
            if (config == null)
                return false;
            
            int totalLevel = _currentLevels[statType] + _pendingLevels[statType];
            float projectedValue = config.BaseValue + (totalLevel + 1) * config.UpgradeIncrement;
            
            return projectedValue <= config.MaxValue;
        }

        public void ResetPendingUpgrades()
        {
            foreach (var statType in _pendingLevels.Keys.ToList())
            {
                _pendingLevels[statType] = 0;
            }
            _pendingPointsUsed = 0;
            
            _signalBus.Fire(new GameInstaller.PendingUpgradesResetSignal());
        }

        public void ApplyPendingUpgrades()
        {
            if (_pendingPointsUsed == 0)
                return;
            
            foreach (var kvp in _pendingLevels)
            {
                if (kvp.Value > 0)
                {
                    _currentLevels[kvp.Key] += kvp.Value;
                    ApplyStatChange(kvp.Key);
                }
            }
            
            _availablePoints.Value -= _pendingPointsUsed;
            
            ResetPendingUpgrades();
            SaveData();
            
            _signalBus.Fire(new GameInstaller.UpgradesAppliedSignal());
        }

        private void ApplyStatChange(StatType statType)
        {
            float newValue = GetStatValue(statType);
            
            switch (statType)
            {
                case StatType.Speed:
                    _playerStats.SetMovementSpeed(newValue);
                    break;
                case StatType.Health:
                    _playerStats.SetMaxHealth(newValue);
                    break;
                case StatType.Damage:
                    _playerStats.SetDamage(newValue);
                    break;
            }
        }

        public float GetStatValue(StatType statType)
        {
            var config = GetStatConfig(statType);
            
            if (config == null) 
                return 0;
            
            int level = _currentLevels[statType];
            
            return Mathf.Min(config.BaseValue + level * config.UpgradeIncrement, config.MaxValue);
        }

        public StatUpgradeConfig GetStatConfig(StatType statType)
        {
            return _configCache.TryGetValue(statType, out var config) ? config : null;
        }
        
        public void SaveData()
        {
            var saveData = new UpgradeSaveData
            {
                AvailablePoints = _availablePoints.Value,
                StatLevels = new List<StatLevelData>()
            };
            
            foreach (var kvp in _currentLevels)
            {
                saveData.StatLevels.Add(new StatLevelData(kvp.Key, kvp.Value));
            }
    
            _saveService.Save("upgrades", saveData);
            Debug.Log($"Saved: {_availablePoints.Value} points, {saveData.StatLevels.Count} stats");
        }

        public void LoadSavedData()
        {
            try
            {
                var saveData = _saveService.Load<UpgradeSaveData>("upgrades");
        
                if (saveData != null)
                {
                    _availablePoints.Value = saveData.AvailablePoints;
                    
                    if (saveData.StatLevels != null && saveData.StatLevels.Count > 0)
                    {
                        foreach (var statLevel in saveData.StatLevels)
                        {
                            if (_currentLevels.ContainsKey(statLevel.Type))
                            {
                                _currentLevels[statLevel.Type] = statLevel.Level;
                                ApplyStatChange(statLevel.Type);
                                Debug.Log($"Loaded stat {statLevel.Type}: Level {statLevel.Level}");
                            }
                        }
                    }
                    else
                    {
                        Debug.Log("No stat levels in save data");
                        InitializeDefaultLevels();
                    }
            
                    Debug.Log($"Loaded: {_availablePoints.Value} points");
                }
                else
                {
                    Debug.Log("No save data found, using defaults");
                    InitializeDefaultLevels();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading save data: {e.Message}");
                InitializeDefaultLevels();
            }
        }
        
        private void InitializeDefaultLevels()
        {
            foreach (var config in _settings.StatConfigs)
            {
                if (!_currentLevels.ContainsKey(config.Type))
                {
                    _currentLevels[config.Type] = 0;
                    _pendingLevels[config.Type] = 0;
                }
            }
        }
    }
}
