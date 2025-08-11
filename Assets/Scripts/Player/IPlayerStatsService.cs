using System;

namespace Game.Player
{
    public interface IPlayerStatsService
    {
        float MovementSpeed { get; }
        float MaxHealth { get; }
        float CurrentHealth { get; }
        float Damage { get; }
        
        IObservable<float> SpeedChanged { get; }
        IObservable<float> HealthChanged { get; }
        IObservable<float> DamageChanged { get; }
        
        void SetMovementSpeed(float speed);
        void SetMaxHealth(float health);
        void SetDamage(float damage);
        void RestoreHealth(float amount);
    }
}
