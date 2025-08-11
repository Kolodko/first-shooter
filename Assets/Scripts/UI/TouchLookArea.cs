using System;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.UI
{
    public class TouchLookArea : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] private float _sensitivity = 1f;

        private readonly Subject<Vector2> _lookDelta = new Subject<Vector2>();
        public IObservable<Vector2> OnLookDelta => _lookDelta;

        private Vector2 _lastPointerPosition;
        private bool _isDragging;

        public void OnPointerDown(PointerEventData eventData)
        {
            _isDragging = true;
            _lastPointerPosition = eventData.position;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_isDragging) 
                return;

            Vector2 delta = eventData.position - _lastPointerPosition;
            delta *= _sensitivity * 0.1f;

            _lookDelta.OnNext(delta);
            _lastPointerPosition = eventData.position;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _isDragging = false;
            _lookDelta.OnNext(Vector2.zero);
        }
    }
}