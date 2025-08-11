using System;
using System.Collections;
using System.Collections.Generic;
using Game.Input;
using Game.Installers;
using Game.Upgrades;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Game.UI
{
    public class UpgradeMenuUI : MonoBehaviour
    {
        [Inject] private IUpgradeService _upgradeService;
        [Inject] private IInputService _inputService;
        [Inject] private SignalBus _signalBus;

        [Header("UI References")]
        [SerializeField] private GameObject _menuPanel;

        [SerializeField] private TextMeshProUGUI _availablePointsText;
        [SerializeField] private Button _applyButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Transform _statsContainer;
        [SerializeField] private StatUpgradeItemUI _statItemPrefab;

        [Header("Animations")] 
        [SerializeField] private CanvasGroup _canvasGroup;

        [SerializeField] private float _fadeSpeed = 5f;

        private readonly Dictionary<StatType, StatUpgradeItemUI> _statItems =
            new Dictionary<StatType, StatUpgradeItemUI>();

        private readonly CompositeDisposable _disposables = new CompositeDisposable();
        private bool _isOpen = false;

        private void Start()
        {
            InitializeUI();
            SubscribeToEvents();
        }

        private void InitializeUI()
        {
            _menuPanel.SetActive(false);
            
            foreach (StatType statType in Enum.GetValues(typeof(StatType)))
            {
                var config = _upgradeService.GetStatConfig(statType);
                
                if (config != null)
                {
                    var item = Instantiate(_statItemPrefab, _statsContainer);
                    item.Initialize(statType, config, _upgradeService);
                    _statItems[statType] = item;
                }
            }
            
            _applyButton.OnClickAsObservable()
                .Subscribe(_ => OnApplyClicked())
                .AddTo(_disposables);

            _closeButton.OnClickAsObservable()
                .Subscribe(_ => OnCloseClicked())
                .AddTo(_disposables);
        }

        private void SubscribeToEvents()
        {
            _inputService.OpenUpgradeMenu
                .Subscribe(_ => ToggleMenu())
                .AddTo(_disposables);
            
            _upgradeService.AvailablePoints
                .Subscribe(points => UpdatePointsDisplay())
                .AddTo(_disposables);
            
            Observable.CombineLatest(
                    _upgradeService.AvailablePoints,
                    _upgradeService.PendingLevels.ObserveCountChanged(),
                    (points, _) => true)
                .Subscribe(_ => UpdateApplyButtonState())
                .AddTo(_disposables);
        }

        public void ToggleMenu()
        {
            if (_isOpen)
                CloseMenu();
            else
                OpenMenu();
        }

        private void OpenMenu()
        {
            _isOpen = true;
            _menuPanel.SetActive(true);
            Time.timeScale = 0f;
            
            StopAllCoroutines();
            StartCoroutine(FadePanel(1f));

            UpdatePointsDisplay();
            UpdateAllStatItems();

            _signalBus.Fire(new GameInstaller.MenuOpenedSignal { MenuType = MenuType.Upgrade });
        }

        private void CloseMenu()
        {
            _isOpen = false;
            Time.timeScale = 1f;
            _upgradeService.ResetPendingUpgrades();
            
            if (_canvasGroup != null)
            {
                StopAllCoroutines();
                StartCoroutine(FadePanel(0f, () => _menuPanel.SetActive(false)));
            }
            else
            {
                _menuPanel.SetActive(false);
            }
            
            ReturnToGameplay();
        
            _signalBus.Fire(new GameInstaller.MenuClosedSignal { MenuType = MenuType.Upgrade });
        
            Debug.Log("Upgrade Menu Closed");
        }
        
        private void ReturnToGameplay()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            
            if (_inputService is InputService inputService)
            {
                var handler = inputService.GetCurrentHandler();
                if (handler is PCInputHandler pcHandler)
                {
                    pcHandler.EnterGameplayMode();
                }
            }
        }

        private void OnApplyClicked()
        {
            _upgradeService.ApplyPendingUpgrades();
            CloseMenu();
        }

        private void OnCloseClicked()
        {
            CloseMenu();
        }

        private void UpdatePointsDisplay()
        {
            int available = _upgradeService.AvailablePoints.Value;
            int pending = GetPendingPointsUsed();
            int remaining = available - pending;

            _availablePointsText.text = $"Points: {remaining}/{available}";

            if (remaining == 0)
                _availablePointsText.color = Color.red;
            else if (pending > 0)
                _availablePointsText.color = Color.yellow;
            else
                _availablePointsText.color = Color.white;
        }

        private int GetPendingPointsUsed()
        {
            int total = 0;
            
            foreach (var kvp in _upgradeService.PendingLevels)
            {
                total += kvp.Value;
            }

            return total;
        }

        private void UpdateAllStatItems()
        {
            foreach (var item in _statItems.Values)
            {
                item.UpdateDisplay();
            }
        }

        private void UpdateApplyButtonState()
        {
            bool hasPendingUpgrades = GetPendingPointsUsed() > 0;
            _applyButton.interactable = hasPendingUpgrades;
        }

        private IEnumerator FadePanel(float targetAlpha, Action onComplete = null)
        {
            while (Mathf.Abs(_canvasGroup.alpha - targetAlpha) > 0.01f)
            {
                _canvasGroup.alpha = Mathf.Lerp(_canvasGroup.alpha, targetAlpha, Time.unscaledDeltaTime * _fadeSpeed);
                
                yield return null;
            }

            _canvasGroup.alpha = targetAlpha;
            onComplete?.Invoke();
        }

        private void OnDestroy()
        {
            _disposables?.Dispose();
            
            if (_isOpen)
            {
                Time.timeScale = 1f;
            }
        }
    }
}