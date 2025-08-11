using System.Collections;
using Game.Player;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Game.UI
{
    public class PlayerHealthUI : MonoBehaviour
    {
        [Inject] private IPlayerController _playerController;
        
        [SerializeField] private Slider _healthBar;
        [SerializeField] private TextMeshProUGUI _healthText;
        [SerializeField] private Image _healthFillImage;
        [SerializeField] private CanvasGroup _damageFlashGroup;
        
        [Header("Visual Settings")]
        [SerializeField] private Gradient _healthGradient;
        [SerializeField] private float _flashDuration = 0.2f;
        
        private CompositeDisposable _disposables = new CompositeDisposable();

        private void Start()
        {
            SubscribeToHealthChanges();
        }

        private void SubscribeToHealthChanges()
        {
            Observable.CombineLatest(
                _playerController.CurrentHealth,
                _playerController.MaxHealth,
                (current, max) => new { Current = current, Max = max })
                .Subscribe(health => UpdateHealthDisplay(health.Current, health.Max))
                .AddTo(_disposables);
            
            _playerController.CurrentHealth
                .Buffer(2, 1)
                .Where(buffer => buffer.Count == 2 && buffer[1] < buffer[0])
                .Subscribe(_ => FlashDamageIndicator())
                .AddTo(_disposables);
        }

        private void UpdateHealthDisplay(float current, float max)
        {
            float healthPercent = max > 0 ? current / max : 0;
            
            _healthBar.value = healthPercent;
            _healthText.text = $"{Mathf.Ceil(current)} / {Mathf.Ceil(max)}";
            
            if (_healthGradient != null && _healthFillImage != null)
            {
                _healthFillImage.color = _healthGradient.Evaluate(healthPercent);
            }
            
            if (healthPercent < 0.2f && healthPercent > 0)
            {
                AddLowHealthEffect();
            }
        }

        private void FlashDamageIndicator()
        {
            if (_damageFlashGroup == null) return;
            
            StopAllCoroutines();
            StartCoroutine(FlashCoroutine());
        }

        private IEnumerator FlashCoroutine()
        {
            _damageFlashGroup.alpha = 0.3f;
            
            float elapsed = 0;
            while (elapsed < _flashDuration)
            {
                elapsed += Time.deltaTime;
                _damageFlashGroup.alpha = Mathf.Lerp(0.3f, 0, elapsed / _flashDuration);
                
                yield return null;
            }
            
            _damageFlashGroup.alpha = 0;
        }

        private void AddLowHealthEffect()
        {
            float pulse = Mathf.PingPong(Time.time * 2, 1);
            transform.localScale = Vector3.one * (1 + pulse * 0.05f);
        }

        private void OnDestroy()
        {
            _disposables?.Dispose();
        }
    }
}
