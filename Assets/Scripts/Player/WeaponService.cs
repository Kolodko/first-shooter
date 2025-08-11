using UnityEngine;
using Zenject;

namespace Game.Weapons
{
    public class WeaponService : IWeaponService
    {
        [Inject] private WeaponSettings _settings;
        [Inject] private Projectile.Factory _projectileFactory;
        [Inject] private SignalBus _signalBus;
        
        private float _lastShotTime;

        public bool CanShoot()
        {
            return Time.time - _lastShotTime >= _settings.FireRate;
        }

        public void Shoot(Vector3 origin, Vector3 direction, float damage)
        {
            if (!CanShoot())
                return;
            
            _lastShotTime = Time.time;
            
            var projectile = _projectileFactory.Create(origin, Quaternion.LookRotation(direction), damage);
            projectile.Initialize(direction * _settings.ProjectileSpeed, _settings.ProjectileLifetime, damage);
            
            PlayMuzzleFlash(origin);
            PlayShootSound(origin);
        }

        private void PlayMuzzleFlash(Vector3 position)
        {
            if (_settings.MuzzleFlashPrefab != null)
            {
                var flash = GameObject.Instantiate(_settings.MuzzleFlashPrefab, position, Quaternion.identity);
                GameObject.Destroy(flash, 0.1f);
            }
        }

        private void PlayShootSound(Vector3 position)
        {
            if (_settings.ShootSound != null)
            {
                AudioSource.PlayClipAtPoint(_settings.ShootSound, position, 0.5f);
            }
        }
    }
}
