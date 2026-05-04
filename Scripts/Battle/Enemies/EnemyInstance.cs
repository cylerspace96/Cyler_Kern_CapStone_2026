// Creates an enemy instance
using UnityEngine;

public class EnemyInstance : MonoBehaviour
{
    [Header("Data")]
    public EnemyData data;                  
    [Header("References")]
    public SpriteRenderer spriteRenderer;   

    [HideInInspector] public Vector3 originalPosition;
}