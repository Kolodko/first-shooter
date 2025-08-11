using UniRx;
using UnityEngine;

namespace Game.Player
{
    public interface IPlayerController
    {
        IReadOnlyReactiveProperty<float> CurrentHealth { get; }
        IReadOnlyReactiveProperty<float> MaxHealth { get; }
        void TakeDamage(float damage);
        void Move(Vector3 direction);
        void Rotate(float horizontal, float vertical);
        void Shoot();
    }
}
