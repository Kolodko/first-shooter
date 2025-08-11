using System;
using UniRx;
using UnityEngine;
using Zenject;

namespace Game.Player
{
    public class PlayerStatsService : IPlayerStatsService, IInitializable
    {
        [Inject] private GameSettings _gameSettings;
        
        private readonly ReactiveProperty<float> _movementSpeed = new ReactiveProperty<float>();
        private readonly ReactiveProperty<float> _maxHealth = new ReactiveProperty<float>();
        private readonly ReactiveProperty<float> _currentHealth = new ReactiveProperty<float>();
        private readonly ReactiveProperty<float> _damage = new ReactiveProperty<float>();
        
        public float MovementSpeed => _movementSpeed.Value;
        public float MaxHealth => _maxHealth.Value;
        public float CurrentHealth => _currentHealth.Value;
        public float Damage => _damage.Value;
        
        public IObservable<float> SpeedChanged => _movementSpeed;
        public IObservable<float> HealthChanged => _maxHealth;
        public IObservable<float> DamageChanged => _damage;

        public void Initialize()
        {
            _movementSpeed.Value = _gameSettings.PlayerStartSpeed;
            _maxHealth.Value = _gameSettings.PlayerStartHealth;
            _currentHealth.Value = _gameSettings.PlayerStartHealth;
            _damage.Value = _gameSettings.PlayerStartDamage;
        }

        public void SetMovementSpeed(float speed)
        {
            _movementSpeed.Value = speed;
        }

        public void SetMaxHealth(float health)
        {
            float healthRatio = _currentHealth.Value / _maxHealth.Value;
            _maxHealth.Value = health;
            _currentHealth.Value = health * healthRatio;
        }

        public void SetDamage(float damage)
        {
            _damage.Value = damage;
        }

        public void RestoreHealth(float amount)
        {
            _currentHealth.Value = Mathf.Min(_currentHealth.Value + amount, _maxHealth.Value);
        }
    }
}
