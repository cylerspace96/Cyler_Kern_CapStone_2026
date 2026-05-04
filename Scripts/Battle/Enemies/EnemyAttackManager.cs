// Manages the enemy
// Basically the equivalent of the player attack manager, but for enemies

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttackManager : MonoBehaviour
{
    [Header("References")]
    public Transform[] enemies;
    public PlayerAttackManager playerAttackManager;
    public EnemyTargetingController targetingController;

    [Header("Attack Settings")]
    public float moveSpeed = 5f;
    public float attackOffsetX = 1.5f;

    [Header("Audio")]
    public AudioSource audioSource;

    bool turnRunning = false;

    // Called by EnemySpawner after enemies are created
    // This method is not used anymore but keeping so nothing crashes
    public void CacheOriginalPositions()
    {
        if (enemies == null) return;
        for (int i = 0; i < enemies.Length; i++)
        {
            if (enemies[i] != null)
            {
                EnemyInstance inst = enemies[i].GetComponent<EnemyInstance>();
                Debug.Log($"Enemy {i} original position: {inst?.originalPosition}");
            }
        }
    }

    // Enemy turn

    public void StartEnemyTurn(PlayerAttackManager playerManager)
    {
        if (turnRunning)
        {
            Debug.LogWarning("Enemy turn already running — ignoring duplicate call.");
            return;
        }
        turnRunning = true;
        StartCoroutine(EnemyTurnSequence(playerManager));
    }

    IEnumerator EnemyTurnSequence(PlayerAttackManager playerManager)
    {
        Debug.Log("Enemy turn started.");

        for (int i = 0; i < enemies.Length; i++)
        {
            if (enemies[i] == null || !enemies[i].gameObject.activeInHierarchy)
                continue;

            // Pick a random living player to attack
            int targetIndex = GetRandomLivingPlayerIndex();
            if (targetIndex == -1)
            {
                Debug.Log("All players defeated — stopping enemy turn.");
                break;
            }

            yield return StartCoroutine(EnemyAttack(i, targetIndex));
        }

        Debug.Log("Enemy turn complete.");
        turnRunning = false;
        playerManager.OnEnemyTurnComplete();
    }

    // Pick a random living player, used when attacking

    int GetRandomLivingPlayerIndex()
    {
        if (playerAttackManager == null || playerAttackManager.players == null) return -1;

        // Build list of active, living player indices
        List<int> living = new List<int>();
        for (int i = 0; i < playerAttackManager.players.Length; i++)
        {
            Transform p = playerAttackManager.players[i];
            if (p == null || !p.gameObject.activeInHierarchy) continue;

            PlayerStats stats = p.GetComponent<PlayerStats>();
            if (stats != null && !stats.IsDead)
                living.Add(i);
        }

        if (living.Count == 0) return -1;
        return living[Random.Range(0, living.Count)];
    }

    // Single enemy attack against a specific player

    IEnumerator EnemyAttack(int enemyIndex, int playerIndex)
    {
        Transform enemy = enemies[enemyIndex];
        Transform target = playerAttackManager.players[playerIndex];
        EnemyInstance instance = enemy.GetComponent<EnemyInstance>();
        Animator animator = enemy.GetComponent<Animator>();

        // Read stats from EnemyData
        float attackDelay = 0.5f;
        float returnDelay = 0.3f;
        int damage = 15;
        float speed = moveSpeed;
        float offsetX = attackOffsetX;
        float offsetY = 0f;
        AudioClip attackSound = null;

        if (instance != null && instance.data != null)
        {
            attackDelay = instance.data.attackDelay;
            returnDelay = instance.data.returnDelay;
            damage = instance.data.attackDamage;
            speed = instance.data.moveSpeed;
            attackSound = instance.data.attackSound;
            offsetX = instance.data.attackOffsetX;
            offsetY = instance.data.attackOffsetY;
        }

        // Guard to make sure target is still alive before moving
        if (target == null || !target.gameObject.activeInHierarchy)
            yield break;

        // Move toward target player
        if (animator != null && instance?.data != null)
            PlayAnimation(animator, instance.data.moveAnimationName);

        Vector3 attackPos = target.position + new Vector3(offsetX, offsetY, 0f);
        yield return StartCoroutine(MoveToPosition(enemy, attackPos, speed));

        // Play attack animation
        if (animator != null && instance?.data != null)
            PlayAnimation(animator, instance.data.attackAnimationName);

        // Play attack sound
        if (attackSound != null && audioSource != null)
            audioSource.PlayOneShot(attackSound);

        yield return new WaitForSeconds(attackDelay);

        // Deal damage
        if (target != null && target.gameObject.activeInHierarchy)
        {
            if (playerAttackManager != null)
            {
                playerAttackManager.OnPlayerHit(playerIndex, damage);
                Debug.Log($"{enemy.name} hits player {playerIndex} for {damage}.");
            }
        }

        yield return new WaitForSeconds(returnDelay);

        // Return to OG position
        if (animator != null && instance?.data != null)
            PlayAnimation(animator, instance.data.moveAnimationName);

        // Read original position from EnemyInstance — set at spawn time by EnemySpawner
        EnemyInstance instanceForReturn = enemy.GetComponent<EnemyInstance>();
        Vector3 homePos = (instanceForReturn != null) ? instanceForReturn.originalPosition : enemy.position;
        Debug.Log($"Enemy {enemyIndex} returning to {homePos}");
        yield return StartCoroutine(MoveToPosition(enemy, homePos, speed));

        // Hard snap to exact home position to prevent drift
        enemy.position = homePos;

        if (animator != null && instance?.data != null)
            PlayAnimation(animator, instance.data.idleAnimationName);
    }

    // Death notification

    public void OnEnemyDefeated()
    {
        if (targetingController != null)
            targetingController.OnEnemyDefeated();
    }

    public bool AllEnemiesDefeated()
    {
        if (enemies == null || enemies.Length == 0) return true;

        foreach (Transform enemy in enemies)
        {
            // Check both null and destroyed Unity objects
            if (enemy == null) continue;

            if (!enemy.Equals(null) && enemy.gameObject.activeInHierarchy)
                return false;
        }
        return true;
    }

    // Plays the animations
    void PlayAnimation(Animator animator, string animationName)
    {
        if (animator == null || string.IsNullOrEmpty(animationName)) return;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName(animationName)) return;

        animator.Play(animationName);
    }

    // Method to move enemy

    IEnumerator MoveToPosition(Transform obj, Vector3 target, float speed)
    {
        while (Vector3.Distance(obj.position, target) > 0.05f)
        {
            obj.position = Vector3.MoveTowards(obj.position, target, speed * Time.deltaTime);
            yield return null;
        }
        obj.position = target;
    }
}