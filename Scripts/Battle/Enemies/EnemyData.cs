// File for setting the enemy's stats and parameters in the inspector

using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemy", menuName = "Battle/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Identity")]
    public string enemyName = "Enemy";
    public Sprite sprite;

    [Header("Stats")]
    public int maxHealth = 100;
    public int attackDamage = 15;
    public float attackDelay = 0.5f;
    public float returnDelay = 0.3f;
    public float moveSpeed = 5f;

    [Header("Attack Position")]
    public float attackOffsetX = 1.5f;
    public float attackOffsetY = 0f;

    [Header("Animations")]
    public string idleAnimationName = "Idle";
    public string moveAnimationName = "Move";
    public string attackAnimationName = "Attack";

    [Header("Audio")]
    public AudioClip attackSound;
}