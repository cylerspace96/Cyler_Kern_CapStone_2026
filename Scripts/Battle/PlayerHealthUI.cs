// This file handles the player character's health in the UI

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealthUI : MonoBehaviour
{
    [Header("References")]
    public PlayerStats playerStats;

    [Header("UI Elements")]
    public Image healthBarFill;
    public TMP_Text healthText;
    public TMP_Text characterNameText;

    [Header("Colors")]
    public Color fullHealthColor = Color.green;
    public Color midHealthColor = Color.yellow;
    public Color lowHealthColor = Color.red;
    // Bellow 30%
    public float lowHealthThreshold = 0.3f;
    // Bellow 60%
    public float midHealthThreshold = 0.6f;

    void Start()
    {
        if (characterNameText != null && playerStats != null)
            characterNameText.text = playerStats.gameObject.name;

        UpdateUI();
    }

    void Update()
    {
        // Update health UI every frame
        UpdateUI();
    }

    void UpdateUI()
    {
        // Updating and filling the health bars based on current health

        if (playerStats == null) return;

        int current = playerStats.currentHp;
        int max = playerStats.maxHp;

        if (max <= 0) return;

        float ratio = (float)current / max;

        // Update bar fill
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = ratio;

            // Color based on health percentage
            if (ratio <= lowHealthThreshold)
                healthBarFill.color = lowHealthColor;
            else if (ratio <= midHealthThreshold)
                healthBarFill.color = midHealthColor;
            else
                healthBarFill.color = fullHealthColor;
        }

        // Update health number text
        if (healthText != null)
            healthText.text = $"{current} / {max}";
    }
}