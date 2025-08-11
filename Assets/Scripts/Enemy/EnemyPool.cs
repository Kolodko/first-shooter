using Game.Enemy;
using UnityEngine;
using Zenject;

public class EnemyPool : MonoMemoryPool<float, EnemyAI>
{
    protected override void Reinitialize(float health, EnemyAI enemy)
    {
        enemy.transform.position = GetRandomSpawnPosition();
        enemy.Initialize(health);
    }
        
    private Vector3 GetRandomSpawnPosition()
    {
        float radius = 20f;
        Vector2 randomCircle = Random.insideUnitCircle * radius;
        
        return new Vector3(randomCircle.x, 0, randomCircle.y);
    }
}
