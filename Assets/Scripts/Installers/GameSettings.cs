using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameSettings", menuName = "Game/GameSettings")]
public class GameSettings : ScriptableObject
{
    [Header("Input")]
    public float MouseSensitivity = 2f;
    public float MobileSwipeSensitivity = 0.5f;
        
    [Header("Gameplay")]
    public float PlayerStartHealth = 100f;
    public float PlayerStartSpeed = 5f;
    public float PlayerStartDamage = 10f;
}
