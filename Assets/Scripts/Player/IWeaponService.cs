using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Weapons
{
    public interface IWeaponService
    {
        bool CanShoot();
        void Shoot(Vector3 origin, Vector3 direction, float damage);
    }
}
