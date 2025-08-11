using UnityEngine;
using UnityEngine.AI;
using UniRx;
using Zenject;
using System;

namespace Game.Enemy
{
    public interface IEnemy
    {
        void Initialize(float health);
        void TakeDamage(float damage);
        IReadOnlyReactiveProperty<float> Health { get; }
        IReadOnlyReactiveProperty<bool> IsDead { get; }
    }
}
