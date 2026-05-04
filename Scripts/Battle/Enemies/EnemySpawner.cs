// Not being used because of the wave feature, but keeping for when I need it again later on

// Handles Enemy spawning in a single match

// using System.Collections.Generic;
// using UnityEngine;

// public class EnemySpawner : MonoBehaviour
// {
//     [Header("Enemy Setup")]
//     public GameObject enemyPrefab;
//     public EnemyData[] possibleEnemyTypes;
//     [Header("Spawn Positions")]

//     public Transform[] spawnPoints;

//     [Header("Spawn Settings")]
//     public int minEnemies = 1;
//     public int maxEnemies = 4;

//     [Header("References")]
//     public EnemyAttackManager enemyAttackManager;
//     public EnemyTargetingController targetingController;
//     public BattleCamera battleCamera;

//     void Awake()
//     {
//         SpawnEnemies();
//     }

//     void SpawnEnemies()
//     {
//         if (possibleEnemyTypes == null || possibleEnemyTypes.Length == 0)
//         {
//             Debug.LogWarning("No enemy types assigned to EnemySpawner!");
//             return;
//         }

//         if (spawnPoints == null || spawnPoints.Length == 0)
//         {
//             Debug.LogWarning("No spawn points assigned to EnemySpawner!");
//             return;
//         }

//         // Pick a random enemy count
//         int count = Random.Range(minEnemies, maxEnemies + 1);
//         count = Mathf.Min(count, spawnPoints.Length); // Can't exceed spawn points

//         Debug.Log($"Spawning {count} enemies.");

//         List<Transform> spawnedEnemies = new List<Transform>();

//         for (int i = 0; i < count; i++)
//         {
//             // Pick a random enemy type
//             EnemyData randomType = possibleEnemyTypes[Random.Range(0, possibleEnemyTypes.Length)];

//             // Spawn at this slot's position
//             GameObject enemyObj = Instantiate(enemyPrefab, spawnPoints[i].position, Quaternion.identity);
//             enemyObj.name = $"{randomType.enemyName}_{i}";

//             // Apply the random type to the EnemyInstance
//             EnemyInstance instance = enemyObj.GetComponent<EnemyInstance>();
//             if (instance != null)
//             {
//                 instance.data = randomType;
//                 instance.originalPosition = spawnPoints[i].position;

//                 // Apply sprite
//                 SpriteRenderer sr = enemyObj.GetComponent<SpriteRenderer>();
//                 if (sr != null && randomType.sprite != null)
//                     sr.sprite = randomType.sprite;
//             }

//             // Apply health from data using InitializeHealth
//             // so BattleUnit.Start() doesn't overwrite it
//             BattleUnit unit = enemyObj.GetComponent<BattleUnit>();
//             if (unit != null)
//             {
//                 unit.InitializeHealth(randomType.maxHealth);
//                 unit.enemyAttackManager = enemyAttackManager;
//             }

//             spawnedEnemies.Add(enemyObj.transform);
//         }

//         // Hand the spawned enemy list to the managers
//         Transform[] enemyArray = spawnedEnemies.ToArray();

//         if (enemyAttackManager != null)
//         {
//             enemyAttackManager.enemies = enemyArray;
//             enemyAttackManager.CacheOriginalPositions();
//         }

//         if (targetingController != null)
//             targetingController.InitializeTargets();

//         // Update camera targets if BattleCamera is assigned
//         if (battleCamera != null)
//             battleCamera.enemyCharacters = enemyArray;

//         Debug.Log("Enemy spawning complete.");
//     }
// }