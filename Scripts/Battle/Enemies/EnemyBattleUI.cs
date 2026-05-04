// Method that handles the enemy health and name UI

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyBattleUI : MonoBehaviour
{
    // Assign this stuff in the inspector

    [Header("References")]
    public PlayerAttackManager playerAttackManager;
    public EnemyTargetingController targetingController;

    [Header("Enemy Health UI")]
    public Image enemyHealthBarFill;
    public TMP_Text enemyHealthText;
    public TMP_Text enemyNameText;

    [Header("Round Damage UI")]
    public TMP_Text roundDamageText;
    public Image roundDamageBarFill;
    public TMP_Text thresholdText;

    [Header("Colors")]
    public Color fullHealthColor = Color.green;
    public Color midHealthColor = Color.yellow;
    public Color lowHealthColor = Color.red;
    public float lowHealthThreshold = 0.3f;
    public float midHealthThreshold = 0.6f;

    // Cache current target to detect changes
    Transform currentTarget;
    BattleUnit currentUnit;

    void Update()
    {
        UpdateEnemyTarget();
        UpdateEnemyHealth();
        UpdateRoundDamage();
    }

    // Track current enemy target
    // Changes the UI to relfect what enemy is being targeted
    void UpdateEnemyTarget()
    {
        if (playerAttackManager == null) return;

        Transform target = playerAttackManager.enemyTarget;

        // If target changed, update cached references
        if (target != currentTarget)
        {
            currentTarget = target;
            currentUnit = (target != null) ? target.GetComponent<BattleUnit>() : null;

            // Update name text
            if (enemyNameText != null)
            {
                if (target != null)
                {
                    // Try to get name from EnemyData first
                    EnemyInstance instance = target.GetComponent<EnemyInstance>();
                    enemyNameText.text = (instance?.data != null)
                        ? instance.data.enemyName
                        : target.name;
                }
                else
                {
                    enemyNameText.text = "---";
                }
            }
        }
    }

    //  Update enemy health bar
    void UpdateEnemyHealth()
    {
        if (currentUnit == null)
        {
            if (enemyHealthBarFill != null) enemyHealthBarFill.fillAmount = 0f;
            if (enemyHealthText != null) enemyHealthText.text = "---";
            return;
        }

        int current = currentUnit.currentHealth;
        int max = currentUnit.maxHealth;

        if (max <= 0) return;

        float ratio = (float)current / max;

        if (enemyHealthBarFill != null)
        {
            enemyHealthBarFill.fillAmount = ratio;

            if (ratio <= lowHealthThreshold)
                enemyHealthBarFill.color = lowHealthColor;
            else if (ratio <= midHealthThreshold)
                enemyHealthBarFill.color = midHealthColor;
            else
                enemyHealthBarFill.color = fullHealthColor;
        }

        if (enemyHealthText != null)
            enemyHealthText.text = $"{current} / {max}";
    }


    // Update round damage display in bottom left
    // Uses currentThreshold instead of damageThreshold so the bar correctly reflects the incremented threshold during bonus rounds
    void UpdateRoundDamage()
    {
        if (playerAttackManager == null) return;

        int damage = playerAttackManager.totalDamageThisTurn;
        int threshold = playerAttackManager.currentThreshold;

        if (roundDamageText != null)
            roundDamageText.text = $"DMG: {damage}";

        if (thresholdText != null)
            thresholdText.text = $"{damage} / {threshold}";

        if (roundDamageBarFill != null && threshold > 0)
            roundDamageBarFill.fillAmount = Mathf.Clamp01((float)damage / threshold);
    }
}