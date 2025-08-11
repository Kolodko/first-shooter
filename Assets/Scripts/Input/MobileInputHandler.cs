using Game.UI;
using UniRx;
using UnityEngine;

public class MobileInputHandler : IInputHandler
    {
        private Subject<Vector3> _moveSubject;
        private Subject<Vector2> _lookSubject;
        private Subject<Unit> _shootSubject;
        private Subject<Unit> _menuSubject;
        
        private MobileInputUI _mobileUI;
        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        public void Initialize(Subject<Vector3> move, Subject<Vector2> look, Subject<Unit> shoot, Subject<Unit> menu)
        {
            _moveSubject = move;
            _lookSubject = look;
            _shootSubject = shoot;
            _menuSubject = menu;
            
            _mobileUI = GameObject.FindObjectOfType<MobileInputUI>();
            
            if (_mobileUI == null)
            {
                var prefab = Resources.Load<GameObject>("MobileInputUI");
                
                if (prefab != null)
                {
                    _mobileUI = GameObject.Instantiate(prefab).GetComponent<MobileInputUI>();
                }
            }
            
            if (_mobileUI != null)
            {
                SubscribeToMobileUI();
            }
        }

        private void SubscribeToMobileUI()
        {
            _mobileUI.JoystickInput
                .Subscribe(input => _moveSubject.OnNext(new Vector3(input.x, 0, input.y)))
                .AddTo(_disposables);
                
            _mobileUI.LookInput
                .Subscribe(input => _lookSubject.OnNext(input))
                .AddTo(_disposables);
                
            _mobileUI.ShootButtonPressed
                .Subscribe(_ => _shootSubject.OnNext(Unit.Default))
                .AddTo(_disposables);
                
            _mobileUI.MenuButtonPressed
                .Subscribe(_ => _menuSubject.OnNext(Unit.Default))
                .AddTo(_disposables);
        }

        public void Update()
        {
            //TODO
        }

        public void Dispose()
        {
            _disposables?.Dispose();
            
            if (_mobileUI != null)
            {
                _mobileUI.gameObject.SetActive(false);
            }
        }
    }
