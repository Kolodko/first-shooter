using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class MobileInputUI : MonoBehaviour
    {
        [SerializeField] private VirtualJoystick _movementJoystick;
        [SerializeField] private TouchLookArea _lookArea;
        [SerializeField] private Button _shootButton;
        [SerializeField] private Button _menuButton;

        private readonly Subject<Vector2> _joystickInput = new Subject<Vector2>();
        private readonly Subject<Vector2> _lookInput = new Subject<Vector2>();
        private readonly Subject<Unit> _shootPressed = new Subject<Unit>();
        private readonly Subject<Unit> _menuPressed = new Subject<Unit>();

        public IObservable<Vector2> JoystickInput => _joystickInput;
        public IObservable<Vector2> LookInput => _lookInput;
        public IObservable<Unit> ShootButtonPressed => _shootPressed;
        public IObservable<Unit> MenuButtonPressed => _menuPressed;

        private void Start()
        {
            if (_movementJoystick != null)
            {
                _movementJoystick.OnJoystickMove
                    .Subscribe(input => _joystickInput.OnNext(input));
            }

            if (_lookArea != null)
            {
                _lookArea.OnLookDelta
                    .Subscribe(delta => _lookInput.OnNext(delta));
            }

            if (_shootButton != null)
            {
                _shootButton.OnClickAsObservable()
                    .Subscribe(_ => _shootPressed.OnNext(Unit.Default));
            }

            if (_menuButton != null)
            {
                _menuButton.OnClickAsObservable()
                    .Subscribe(_ => _menuPressed.OnNext(Unit.Default));
            }
        }
    }
}