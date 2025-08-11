using System;
using Game.Enemy;
using UnityEngine;
using Zenject;

public class Projectile : MonoBehaviour, IPoolable<Vector3, Quaternion, float, IMemoryPool>, IDisposable
{
    private Rigidbody _rigidbody;
    private float _damage;
    private float _lifetime;
    private IMemoryPool _pool;
    private float _spawnTime;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        if (_rigidbody == null)
        {
            _rigidbody = gameObject.AddComponent<Rigidbody>();
            _rigidbody.useGravity = false;
            _rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }
    }

    public void Initialize(Vector3 velocity, float lifetime, float damage)
    {
        _rigidbody.velocity = velocity;
        _lifetime = lifetime;
        _damage = damage;
        _spawnTime = Time.time;
    }

    private void Update()
    {
        if (Time.time - _spawnTime > _lifetime)
        {
            Despawn();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            var enemy = other.GetComponent<IEnemy>();
            enemy?.TakeDamage(_damage);
            CreateHitEffect(other.ClosestPoint(transform.position));
            Despawn();
        }
        else if (!other.CompareTag("Player"))
        {
            CreateHitEffect(transform.position);
            Despawn();
        }
    }

    private void CreateHitEffect(Vector3 position)
    {
        //TODO
    }

    private void Despawn()
    {
        _pool?.Despawn(this);
    }

    public void OnSpawned(Vector3 position, Quaternion rotation, float damage, IMemoryPool pool)
    {
        _pool = pool;
        transform.position = position;
        transform.rotation = rotation;
        _damage = damage;
        gameObject.SetActive(true);
    }

    public void OnDespawned()
    {
        _pool = null;
        gameObject.SetActive(false);
        _rigidbody.velocity = Vector3.zero;
    }

    public void Dispose() { }

    public class Factory : PlaceholderFactory<Vector3, Quaternion, float, Projectile> { }
}
