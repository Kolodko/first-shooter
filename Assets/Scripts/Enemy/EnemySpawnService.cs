using System.Collections.Generic;
using Game.Installers;
using UniRx;
using UnityEngine;
using Zenject;

namespace Game.Enemy
{
    public class EnemySpawnService : IEnemySpawnService, ITickable, IInitializable
    {
        [Inject] private EnemySettings _settings;
        [Inject] private EnemyPool _enemyPool;
        [Inject] private SignalBus _signalBus;
        
        private float _lastSpawnTime;
        private bool _isSpawning;
        private readonly List<EnemyAI> _activeEnemies = new List<EnemyAI>();
        private Transform _playerTransform;

        public void Initialize()
        {
            _playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
            
            _signalBus.GetStream<GameInstaller.EnemyDeathSignal>()
                .Subscribe(signal => OnEnemyDeath(signal.Enemy));
                
            StartSpawning();
        }

        public void Tick()
        {
            if (!_isSpawning || _playerTransform == null) return;
            
            if (Time.time - _lastSpawnTime >= _settings.SpawnInterval)
            {
                if (_activeEnemies.Count < _settings.MaxEnemies)
                {
                    SpawnEnemy();
                    _lastSpawnTime = Time.time;
                }
            }
        }

        public void StartSpawning()
        {
            _isSpawning = true;
        }

        public void StopSpawning()
        {
            _isSpawning = false;
        }

        private void SpawnEnemy()
        {
            Vector3 spawnPosition = GetRandomSpawnPosition();
            float health = Random.Range(_settings.MinHealth, _settings.MaxHealth);
            
            var enemy = _enemyPool.Spawn(health);
            enemy.transform.position = spawnPosition;
            
            _activeEnemies.Add(enemy);
            _signalBus.Fire(new GameInstaller.EnemySpawnedSignal { Enemy = enemy });
        }

        private Vector3 GetRandomSpawnPosition()
        {
            if (_playerTransform == null)
                return Vector3.zero;
            
            float angle = Random.Range(0, 360) * Mathf.Deg2Rad;
            float distance = Random.Range(_settings.MinSpawnDistance, _settings.MaxSpawnDistance);
            
            Vector3 offset = new Vector3(Mathf.Sin(angle) * distance, 0, Mathf.Cos(angle) * distance);
            
            return _playerTransform.position + offset;
        }

        private void OnEnemyDeath(EnemyAI enemy)
        {
            _activeEnemies.Remove(enemy);
        }
    }
}
