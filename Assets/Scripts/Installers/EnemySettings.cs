using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemySettings", menuName = "Game/EnemySettings")]
public class EnemySettings : ScriptableObject
{
    [Header("Health")]
    public float MinHealth = 10f;
    public float MaxHealth = 100f;
        
    [Header("AI")]
    public float DetectionRange = 15f;
    public float AttackRange = 2f;
    public float MovementSpeed = 3.5f;
        
    [Header("Combat")]
    public float Damage = 10f;
    public float AttackCooldown = 1.5f;
        
    [Header("Spawning")]
    public float SpawnInterval = 5f;
    public int MaxEnemies = 10;
    public float MinSpawnDistance = 10f;
    public float MaxSpawnDistance = 30f;
}
