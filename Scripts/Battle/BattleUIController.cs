// Script to handle the combo and QTE Ui elements in the battle UI

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleUIController : MonoBehaviour
{
    [Header("Combo UI")]
    public Image comboCircle;

    [Header("QTE UI")]
    public Image qteCircle;
    public TMP_Text qteText;

    float comboMaxTime;
    float qteMaxTime;

    void Awake()
    {
        HideQTE();
        HideCombo();
    }

// Handling the Combo circles in the battle UI

    public void showCombo(float duration)
    {
        comboMaxTime = duration;
        comboCircle.fillAmount = 1f;
        comboCircle.gameObject.SetActive(true);
    }

    public void UpdateCombo(float timeRemaining)
    {
        comboCircle.fillAmount = timeRemaining / comboMaxTime;
    }

    public void HideCombo()
    {
        comboCircle.gameObject.SetActive(false);

    }

    // Handling the QTE event in the battle UI
    public void showQTE(float duration)
    {
        qteMaxTime = duration;
        qteCircle.fillAmount = 1f;
        qteCircle.gameObject.SetActive(true);
        qteText.gameObject.SetActive(true);
        qteText.text = "TRIGGER";
    }

    public void updateQTE(float timeRemaining)
    {
        qteCircle.fillAmount = timeRemaining / qteMaxTime;
    }

    public void HideQTE()
    {
        qteCircle.gameObject.SetActive(false);
        qteText.gameObject.SetActive(false);
    }
}