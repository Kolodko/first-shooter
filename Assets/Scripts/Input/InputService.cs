using System;
using UniRx;
using UnityEngine;
using Zenject;

namespace Game.Input
{
    public class InputService : IInputService, ITickable, IInitializable
    {
        private readonly Subject<Vector3> _moveInput = new Subject<Vector3>();
        private readonly Subject<Vector2> _lookInput = new Subject<Vector2>();
        private readonly Subject<Unit> _shootInput = new Subject<Unit>();
        private readonly Subject<Unit> _openUpgradeMenu = new Subject<Unit>();

        private IInputHandler _currentHandler;
        private readonly DiContainer _container;
        private InputMode _currentMode = InputMode.PC;

        [Inject] private GameSettings _gameSettings;

        public IObservable<Vector3> MoveInput => _moveInput;
        public IObservable<Vector2> LookInput => _lookInput;
        public IObservable<Unit> ShootInput => _shootInput;
        public IObservable<Unit> OpenUpgradeMenu => _openUpgradeMenu;
        public float MouseSensitivity => _gameSettings.MouseSensitivity;

        public InputService(DiContainer container)
        {
            _container = container;
        }

        public void Initialize()
        {
            SetInputMode(_currentMode);
        }

        public void SetInputMode(InputMode mode)
        {
            _currentMode = mode;
            _currentHandler?.Dispose();

            switch (mode)
            {
                case InputMode.PC:
                    _currentHandler = _container.Instantiate<PCInputHandler>();
                    break;
                case InputMode.Mobile:
                    _currentHandler = _container.Instantiate<MobileInputHandler>();
                    break;
            }

            _currentHandler.Initialize(_moveInput, _lookInput, _shootInput, _openUpgradeMenu);
        }
        
        public IInputHandler GetCurrentHandler()
        {
            return _currentHandler;
        }

        public void Tick()
        {
            _currentHandler?.Update();
        }
    }
}