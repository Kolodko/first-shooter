using System.Collections;
using Game.Upgrades;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Game.UI
{
    public class UpgradeButtonUI : MonoBehaviour
    {
        [Inject] private IUpgradeService _upgradeService;

        [SerializeField] private Button _upgradeButton;
        [SerializeField] private TextMeshProUGUI _pointsIndicator;
        [SerializeField] private GameObject _notificationBadge;

        private CompositeDisposable _disposables = new CompositeDisposable();

        private void Start()
        {
            _upgradeService.AvailablePoints
                .Subscribe(points => UpdatePointsDisplay(points))
                .AddTo(_disposables);
        }

        private void UpdatePointsDisplay(int points)
        {
            if (_pointsIndicator != null)
            {
                _pointsIndicator.text = points.ToString();
            }

            if (_notificationBadge != null)
            {
                _notificationBadge.SetActive(points > 0);
            }
            
            if (points > 0)
            {
                AnimateBounce();
            }
        }

        private void AnimateBounce()
        {
            StopAllCoroutines();
            StartCoroutine(BounceAnimation());
        }

        private IEnumerator BounceAnimation()
        {
            Vector3 originalScale = transform.localScale;
            Vector3 targetScale = originalScale * 1.2f;

            float duration = 0.3f;
            float elapsed = 0;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float bounce = Mathf.Sin(t * Mathf.PI);
                transform.localScale = Vector3.Lerp(originalScale, targetScale, bounce);
                
                yield return null;
            }

            transform.localScale = originalScale;
        }

        private void OnDestroy()
        {
            _disposables?.Dispose();
        }
    }
}