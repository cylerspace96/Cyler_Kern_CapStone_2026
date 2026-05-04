using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Stats")]
    public string characterName = "Player";
    public int maxHp = 100;
    public int currentHp;

    [Header("Attacks")]
    public AttackData[] attacks = new AttackData[5];

    [Header("Animations")]
    public string idleAnimationName = "Idle";
    public string moveAnimationName = "Move";
    public string hitAnimationName = "Hit";
    public string deathAnimationName = "Death";

    [Header("Audio")]
    public AudioClip hitSound;

    public bool IsDead => currentHp <= 0;

    void Awake()
    {
        currentHp = maxHp;
    }

    public void TakeDamage(int damage)
    {
        if (IsDead) return;

        currentHp -= damage;
        currentHp = Mathf.Max(currentHp, 0);

        if (currentHp <= 0)
            Die();
    }

    void Die()
    {
        // Play death animation
        Animator animator = GetComponent<Animator>();
        if (animator != null && !string.IsNullOrEmpty(deathAnimationName))
            animator.Play(deathAnimationName);
    }

    public AttackData GetAttackByIndex(int index)
    {
        if (attacks == null || index < 0 || index >= attacks.Length)
            return null;

        return attacks[index];
    }
}