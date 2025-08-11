using Game.Upgrades;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class StatUpgradeItemUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _statNameText;
        [SerializeField] private TextMeshProUGUI _currentLevelText;
        [SerializeField] private Slider _levelSlider;
        [SerializeField] private Button _upgradeButton;
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _valueText;
        
        private StatType _statType;
        private StatUpgradeConfig _config;
        private IUpgradeService _upgradeService;
        private CompositeDisposable _disposables = new CompositeDisposable();

        public void Initialize(StatType statType, StatUpgradeConfig config, IUpgradeService upgradeService)
        {
            _statType = statType;
            _config = config;
            _upgradeService = upgradeService;
            
            SetupUI();
            SubscribeToChanges();
        }

        private void SetupUI()
        {
            _statNameText.text = GetLocalizedName();
            
            if (!string.IsNullOrEmpty(_config.IconPath))
            {
                var icon = Resources.Load<Sprite>(_config.IconPath);
                
                if (icon != null)
                    _iconImage.sprite = icon;
            }
            
            _upgradeButton.OnClickAsObservable()
                .Subscribe(_ => OnUpgradeClicked())
                .AddTo(_disposables);
        }

        private void SubscribeToChanges()
        {
            Observable.CombineLatest(
                _upgradeService.CurrentLevels.ObserveReplace().StartWith(default(DictionaryReplaceEvent<StatType, int>)),
                _upgradeService.PendingLevels.ObserveReplace().StartWith(default(DictionaryReplaceEvent<StatType, int>)),
                (_, __) => true)
                .Subscribe(_ => UpdateDisplay())
                .AddTo(_disposables);
                
            _upgradeService.AvailablePoints
                .Subscribe(_ => UpdateButtonState())
                .AddTo(_disposables);
        }

        public void UpdateDisplay()
        {
            int currentLevel = _upgradeService.CurrentLevels.ContainsKey(_statType) 
                ? _upgradeService.CurrentLevels[_statType] : 0;
            int pendingLevel = _upgradeService.PendingLevels.ContainsKey(_statType) 
                ? _upgradeService.PendingLevels[_statType] : 0;
            int totalLevel = currentLevel + pendingLevel;
            
            if (pendingLevel > 0)
            {
                _currentLevelText.text = $"Lv.{currentLevel} <color=yellow>(+{pendingLevel})</color>";
            }
            else
            {
                _currentLevelText.text = $"Lv.{currentLevel}";
            }
            
            float maxPossibleLevels = (_config.MaxValue - _config.BaseValue) / _config.UpgradeIncrement;
            _levelSlider.maxValue = maxPossibleLevels;
            _levelSlider.value = totalLevel;
            
            float currentValue = _upgradeService.GetStatValue(_statType);
            float pendingValue = _config.BaseValue + totalLevel * _config.UpgradeIncrement;
            
            if (pendingLevel > 0)
            {
                _valueText.text = $"{FormatStatValue(currentValue)} → <color=yellow>{FormatStatValue(pendingValue)}</color>";
            }
            else
            {
                _valueText.text = FormatStatValue(currentValue);
            }
            
            UpdateButtonState();
        }

        private void UpdateButtonState()
        {
            bool canUpgrade = _upgradeService.CanUpgrade(_statType);
            _upgradeButton.interactable = canUpgrade;
            
            if (!canUpgrade)
            {
                if (_upgradeService.GetStatValue(_statType) >= _config.MaxValue)
                {
                    _upgradeButton.GetComponentInChildren<TextMeshProUGUI>().text = "MAX";
                }
                else
                {
                    _upgradeButton.GetComponentInChildren<TextMeshProUGUI>().text = "+";
                }
            }
            else
            {
                _upgradeButton.GetComponentInChildren<TextMeshProUGUI>().text = "+";
            }
        }

        private void OnUpgradeClicked()
        {
            _upgradeService.IncreasePendingLevel(_statType);
        }

        private string GetLocalizedName()
        {
            return _statType.ToString();
        }

        private string FormatStatValue(float value)
        {
            switch (_statType)
            {
                case StatType.Speed:
                    return $"{value:F1} m/s";
                case StatType.Health:
                    return $"{value:F0} HP";
                case StatType.Damage:
                    return $"{value:F0} DMG";
                default:
                    return value.ToString("F1");
            }
        }

        private void OnDestroy()
        {
            _disposables?.Dispose();
        }
    }
