// Used only for enemies, tracks health and damage and handles death

using UnityEngine;

public class BattleUnit : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;
    public EnemyAttackManager enemyAttackManager;

    bool healthInitialized = false;

    public void InitializeHealth(int max)
    {
        maxHealth = max;
        currentHealth = max;
        healthInitialized = true;
    }

    void Start()
    {
        if (!healthInitialized)
            currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        if (currentHealth <= 0)
            Die();
    }

    void Die()
    {

        if (enemyAttackManager != null)
            enemyAttackManager.OnEnemyDefeated();

        gameObject.SetActive(false);
    }
}