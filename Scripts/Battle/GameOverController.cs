// Handles the state of the game when all player characters die

using System.Collections;
using UnityEngine;

public class GameOverController : MonoBehaviour
{
    [Header("UI")]
    public GameObject gameOverPanel;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip gameOverSound;
    public BattleAudioManager battleAudioManager;

    [Header("Settings")]
    public float delayBeforeGameOver = 1.5f;
    public float musicFadeOutDuration = 1f;

    void Start()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    // Called by playerAttackManager when all player characters are killed

    public void TriggerGameOver()
    {
        StartCoroutine(GameOverSequence());
    }

    IEnumerator GameOverSequence()
    {
        // Pause to let any animations finish
        yield return new WaitForSeconds(delayBeforeGameOver);

        // Fade out the battle music
        if (battleAudioManager != null)
            battleAudioManager.FadeOutMusic(musicFadeOutDuration);

        yield return new WaitForSeconds(musicFadeOutDuration);

        // Play game over sound
        if (gameOverSound != null && audioSource != null)
            audioSource.PlayOneShot(gameOverSound);

        // show game over panel
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }

    // Handles buttons on death
    // Press retry or main menu to execute those things

    public void OnRetryPressed()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    public void OnMainMenuPressed()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.ReturnToMainMenu();
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}