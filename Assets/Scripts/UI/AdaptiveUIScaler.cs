using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class AdaptiveUIScaler : MonoBehaviour
    {
        [SerializeField] private CanvasScaler _canvasScaler;
        [SerializeField] private float _referenceRatio = 16f / 9f;

        private void Start()
        {
            AdaptToScreen();
        }

        private void AdaptToScreen()
        {
            if (_canvasScaler == null) 
                return;

            float currentRatio = (float)Screen.width / Screen.height;

            if (currentRatio >= _referenceRatio)
            {
                _canvasScaler.matchWidthOrHeight = 1f;
            }
            else
            {
                _canvasScaler.matchWidthOrHeight = 0f;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            AdaptToScreen();
        }
#endif
    }
}
