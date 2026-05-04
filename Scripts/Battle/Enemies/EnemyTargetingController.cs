using UnityEngine;

public class EnemyTargetingController : MonoBehaviour
{
    [Header("References")]
    public EnemyAttackManager enemyAttackManager;
    public PlayerAttackManager playerAttackManager;

    [Header("Indicator")]
    public TargetIndicator targetIndicator;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip targetSwitchSound;

    int currentTargetIndex = 0;

    // ─────────────────────────────────────────────
    //  Called by EnemySpawner once enemies are ready
    // ─────────────────────────────────────────────

    public void InitializeTargets()
    {
        currentTargetIndex = 0;
        SelectTarget(0);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A))
            CycleTarget(1);

        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D))
            CycleTarget(-1);
    }

    // ─────────────────────────────────────────────
    //  Targeting
    // ─────────────────────────────────────────────

    void CycleTarget(int direction)
    {
        if (enemyAttackManager.enemies == null || enemyAttackManager.enemies.Length == 0)
            return;

        int startIndex = currentTargetIndex;
        int next = currentTargetIndex;

        for (int i = 0; i < enemyAttackManager.enemies.Length; i++)
        {
            next = (next + direction + enemyAttackManager.enemies.Length) % enemyAttackManager.enemies.Length;

            if (enemyAttackManager.enemies[next].gameObject.activeInHierarchy)
            {
                if (next != startIndex)
                {
                    SelectTarget(next);
                    PlaySwitchSound();
                }
                return;
            }
        }
    }

    void SelectTarget(int index)
    {
        Debug.Log($"SelectTarget called — index: {index}, playerAttackManager null? {playerAttackManager == null}");

        Debug.Log($"SelectTarget — index: {index}");
        Debug.Log($"enemies null? {enemyAttackManager.enemies == null}");
        Debug.Log($"enemies length: {enemyAttackManager.enemies?.Length}");

        if (enemyAttackManager.enemies == null || index >= enemyAttackManager.enemies.Length)
        {
            Debug.Log("Early return — enemies null or index out of range");
            return;
        }
            
        Transform enemy = enemyAttackManager.enemies[index];
        Debug.Log($"enemy null? {enemy == null}");
        Debug.Log($"enemy active? {enemy?.gameObject.activeInHierarchy}");
        if (enemy == null || !enemy.gameObject.activeInHierarchy)
        {
            Debug.Log("Early return — enemy null or inactive");
            return;
        }

        currentTargetIndex = index;
        playerAttackManager.enemyTarget = enemy;
        Debug.Log($"enemyTarget set to: {enemy.name}");

        if (targetIndicator != null)
        {
            targetIndicator.SetTarget(enemy);
            targetIndicator.Show();
        }

        Debug.Log($"Target switched to: {enemy.name}");
    }

    void PlaySwitchSound()
    {
        if (targetSwitchSound != null && audioSource != null)
            audioSource.PlayOneShot(targetSwitchSound);
    }

    // ─────────────────────────────────────────────
    //  Called when an enemy dies
    // ─────────────────────────────────────────────

    public void OnEnemyDefeated()
    {
        if (enemyAttackManager.enemies == null) return;

        // If current target is dead, find the next living enemy
        if (!enemyAttackManager.enemies[currentTargetIndex].gameObject.activeInHierarchy)
            CycleTarget(1);
    }
}