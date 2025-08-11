using Game.Input;
using Game.Installers;
using Game.Weapons;
using UniRx;
using UnityEngine;
using Zenject;

namespace Game.Player
{
    public class PlayerController : MonoBehaviour, IPlayerController
    {
        [Inject] private IInputService _inputService;
        [Inject] private IPlayerStatsService _playerStats;
        [Inject] private IWeaponService _weaponService;
        [Inject] private SignalBus _signalBus;

        [SerializeField] private CharacterController _characterController;
        [SerializeField] private Transform _cameraTransform;
        [SerializeField] private Transform _weaponMount;
        
        private readonly ReactiveProperty<float> _currentHealth = new ReactiveProperty<float>();
        private readonly ReactiveProperty<float> _maxHealth = new ReactiveProperty<float>();
        private readonly CompositeDisposable _disposables = new CompositeDisposable();
        
        private float _verticalVelocity;
        private const float Gravity = -9.81f;
        
        public IReadOnlyReactiveProperty<float> CurrentHealth => _currentHealth;
        public IReadOnlyReactiveProperty<float> MaxHealth => _maxHealth;

        private void Start()
        {
            InitializeStats();
            SubscribeToInput();
            SubscribeToStatsChanges();
        }

        private void InitializeStats()
        {  
            _maxHealth.Value = _playerStats.MaxHealth;
            _currentHealth.Value = _playerStats.CurrentHealth;
        }

        private void SubscribeToInput()
        {
            _inputService.MoveInput
                .Subscribe(
                    input => Move(input),
                    error => Debug.LogError($"Move input error: {error}"),
                    () => Debug.LogWarning("Move input completed unexpectedly!"))
                .AddTo(_disposables);

            _inputService.LookInput
                .Subscribe(input => Rotate(input.x, input.y))
                .AddTo(_disposables);

            _inputService.ShootInput
                .Where(_ => _weaponService.CanShoot())
                .Subscribe(_ => Shoot())
                .AddTo(_disposables);
        }

        private void SubscribeToStatsChanges()
        {
            _playerStats.HealthChanged
                .Subscribe(health =>
                {
                    _maxHealth.Value = health;
                    if (_currentHealth.Value > health)
                        _currentHealth.Value = health;
                })
                .AddTo(_disposables);

            _currentHealth
                .Where(health => health <= 0)
                .Take(1)
                .Subscribe(_ => OnPlayerDeath())
                .AddTo(_disposables);
        }

        public void Move(Vector3 direction)
        {
            if (!_characterController.isGrounded)
                _verticalVelocity += Gravity * Time.deltaTime;
            else if (_verticalVelocity < 0)
                _verticalVelocity = -2f;

            Vector3 moveDirection = transform.TransformDirection(direction);
            moveDirection *= _playerStats.MovementSpeed;
            moveDirection.y = _verticalVelocity;

            _characterController.Move(moveDirection * Time.deltaTime);
        }

        public void Rotate(float horizontal, float vertical)
        {
            transform.Rotate(Vector3.up * horizontal * _inputService.MouseSensitivity);
            
            float currentXRotation = _cameraTransform.localEulerAngles.x;
            if (currentXRotation > 180) currentXRotation -= 360;
            
            float newXRotation = Mathf.Clamp(currentXRotation - vertical * _inputService.MouseSensitivity, -80f, 80f);
            _cameraTransform.localEulerAngles = new Vector3(newXRotation, 0, 0);
        }

        public void Shoot()
        {
            _weaponService.Shoot(_weaponMount.position, _cameraTransform.forward, _playerStats.Damage);
            _signalBus.Fire(new GameInstaller.PlayerShootSignal());
        }

        public void TakeDamage(float damage)
        {
            Debug.Log($"Player TakeDamage called with damage: {damage}");
    
            if (damage <= 0)
            {
                Debug.LogWarning("Damage is 0 or negative!");
                
                return;
            }
    
            float oldHealth = _currentHealth.Value;
            _currentHealth.Value = Mathf.Max(0, _currentHealth.Value - damage);
    
            Debug.Log($"Player health: {oldHealth} -> {_currentHealth.Value}");
    
            _signalBus.Fire(new GameInstaller.PlayerDamagedSignal { Damage = damage });
        }

        private void OnPlayerDeath()
        {
            _signalBus.Fire(new GameInstaller.PlayerDeathSignal());
        }

        private void OnDestroy()
        {
            _disposables?.Dispose();
        }
    }
}
