using System;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.UI
{
    public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        [SerializeField] private RectTransform _background;
        [SerializeField] private RectTransform _handle;
        [SerializeField] private float _range = 100f;

        private readonly Subject<Vector2> _joystickMove = new Subject<Vector2>();
        public IObservable<Vector2> OnJoystickMove => _joystickMove;

        private Vector2 _inputVector;
        private bool _isActive;

        public void OnPointerDown(PointerEventData eventData)
        {
            _isActive = true;
            OnDrag(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _isActive = false;
            _inputVector = Vector2.zero;
            _handle.anchoredPosition = Vector2.zero;
            _joystickMove.OnNext(Vector2.zero);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_isActive)
                return;

            Vector2 position;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _background,
                eventData.position,
                eventData.pressEventCamera,
                out position);

            position = Vector2.ClampMagnitude(position, _range);
            _handle.anchoredPosition = position;

            _inputVector = position / _range;
            _joystickMove.OnNext(_inputVector);
        }
    }
}