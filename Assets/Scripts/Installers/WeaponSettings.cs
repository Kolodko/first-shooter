using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponSettings", menuName = "Game/WeaponSettings")]
public class WeaponSettings : ScriptableObject
{
    [Header("Shooting")]
    public float FireRate = 0.5f;
    public float ProjectileSpeed = 20f;
    public float ProjectileLifetime = 3f;
    public float MaxRange = 50f;
        
    [Header("Effects")]
    public GameObject MuzzleFlashPrefab;
    public AudioClip ShootSound;
    public GameObject HitEffectPrefab;
}
