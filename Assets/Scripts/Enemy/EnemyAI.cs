using System;
using Game.Installers;
using Game.Player;
using UniRx;
using UnityEngine;
using UnityEngine.AI;
using Zenject;

namespace Game.Enemy
{
    public class EnemyAI : MonoBehaviour, IEnemy, IPoolable<float, IMemoryPool>, IDisposable
    {
        [Inject] private IPlayerController _player;
        [Inject] private SignalBus _signalBus;
        [Inject] private EnemySettings _enemySettings;
        
        [SerializeField] private NavMeshAgent _navMeshAgent;
        [SerializeField] private Animator _animator;
        [SerializeField] private Collider _collider;
        
        private readonly ReactiveProperty<float> _health = new ReactiveProperty<float>();
        private readonly ReactiveProperty<bool> _isDead = new ReactiveProperty<bool>(false);
        private readonly CompositeDisposable _disposables = new CompositeDisposable();
        
        private IMemoryPool _pool;
        private Transform _playerTransform;
        private float _attackCooldown;
        private float _lastAttackTime;
        
        private AIState _currentState = AIState.Idle;
        
        public IReadOnlyReactiveProperty<float> Health => _health;
        public IReadOnlyReactiveProperty<bool> IsDead => _isDead;

        private void Start()
        {
            _playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
            
            Observable.Interval(TimeSpan.FromSeconds(0.2f))
                .Where(_ => !_isDead.Value && _playerTransform != null)
                .Subscribe(_ => UpdateAI())
                .AddTo(_disposables);
                
            _isDead
                .Where(isDead => isDead)
                .Subscribe(_ => OnDeath())
                .AddTo(_disposables);
        }

        public void Initialize(float health)
        {
            _health.Value = health;
            _isDead.Value = false;
            _currentState = AIState.Idle;
            _collider.enabled = true;
            
            if (_navMeshAgent != null)
            {
                _navMeshAgent.enabled = true;
                _navMeshAgent.speed = _enemySettings.MovementSpeed;
                _navMeshAgent.stoppingDistance = _enemySettings.AttackRange;
            }
        }

        private void UpdateAI()
        {
            if (_playerTransform == null) return;
            
            float distanceToPlayer = Vector3.Distance(transform.position, _playerTransform.position);
            
            switch (_currentState)
            {
                case AIState.Idle:
                    if (distanceToPlayer < _enemySettings.DetectionRange)
                    {
                        _currentState = AIState.Chasing;
                    }
                    break;
                    
                case AIState.Chasing:
                    if (distanceToPlayer <= _enemySettings.AttackRange)
                    {
                        _currentState = AIState.Attacking;
                        _navMeshAgent.isStopped = true;
                    }
                    else
                    {
                        _navMeshAgent.isStopped = false;
                        _navMeshAgent.SetDestination(_playerTransform.position);
                    }
                    
                    if (distanceToPlayer > _enemySettings.DetectionRange * 1.5f)
                    {
                        _currentState = AIState.Idle;
                        _navMeshAgent.isStopped = true;
                    }
                    break;
                    
                case AIState.Attacking:
                    LookAtPlayer();
                    
                    if (Time.time - _lastAttackTime >= _enemySettings.AttackCooldown)
                    {
                        Attack();
                        _lastAttackTime = Time.time;
                    }
                    
                    if (distanceToPlayer > _enemySettings.AttackRange)
                    {
                        _currentState = AIState.Chasing;
                        _navMeshAgent.isStopped = false;
                    }
                    break;
            }
            
            UpdateAnimations();
        }

        private void LookAtPlayer()
        {
            Vector3 direction = (_playerTransform.position - transform.position).normalized;
            direction.y = 0;
            
            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
            }
        }

        private void Attack()
        {
            if (_animator != null)
            {
                _animator.SetTrigger("Attack");
            }
            
            float distanceToPlayer = Vector3.Distance(transform.position, _playerTransform.position);
            
            if (distanceToPlayer <= _enemySettings.AttackRange)
            {
                Debug.Log($"In range! Dealing {_enemySettings.Damage} damage");
                _player.TakeDamage(_enemySettings.Damage);
                _signalBus.Fire(new GameInstaller.EnemyAttackSignal { Damage = _enemySettings.Damage });
                return;
            }
    
            Debug.Log("Attack missed - no hit detected");
        }

        private void UpdateAnimations()
        {
            if (_animator == null) return;
            
            float speed = _navMeshAgent.velocity.magnitude;
            _animator.SetFloat("Speed", speed);
            _animator.SetBool("IsAttacking", _currentState == AIState.Attacking);
        }

        public void TakeDamage(float damage)
        {
            if (_isDead.Value) 
                return;
            
            _health.Value = Mathf.Max(0, _health.Value - damage);
            
            if (_health.Value <= 0)
            {
                _isDead.Value = true;
            }
            else
            {
                if (_animator != null)
                {
                    _animator.SetTrigger("Hit");
                }
            }
        }

        private void OnDeath()
        {
            _currentState = AIState.Dead;
            
            if (_navMeshAgent != null)
            {
                _navMeshAgent.enabled = false;
            }
            
            if (_collider != null)
            {
                _collider.enabled = false;
            }
            
            if (_animator != null)
            {
                _animator.SetTrigger("Death");
            }
            
            _signalBus.Fire(new GameInstaller.EnemyDeathSignal { Enemy = this });
            
            Observable.Timer(TimeSpan.FromSeconds(0.5f))
                .Subscribe(_ => 
                {
                    if (_pool != null)
                        _pool.Despawn(this);
                    else
                        Destroy(gameObject);
                })
                .AddTo(_disposables);
        }

        public void OnDespawned()
        {
            _pool = null;
            gameObject.SetActive(false);
        }

        public void OnSpawned(float health, IMemoryPool pool)
        {
            _pool = pool;
            gameObject.SetActive(true);
            Initialize(health);
        }

        public void Dispose()
        {
            _disposables?.Dispose();
        }

        private void OnDestroy()
        {
            Dispose();
        }
    }
}
