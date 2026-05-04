// Script to handle most main menu interaction

using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject howToPlayPanel;
    public GameObject creditsPanel;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip buttonSound;

    void Start()
    {
        // Always start on main menu
        ShowMainMenu();
    }

    // Button methods to be used when interacting with menu buttons
    public void OnStartPressed()
    {
        PlayButtonSound();
        if (GameManager.Instance != null)
            GameManager.Instance.StartGame();
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Battle");
        }
    }

    public void OnHowToPlayPressed()
    {
        PlayButtonSound();
        ShowHowToPlay();
    }

    public void OnCreditsPressed()
    {
        PlayButtonSound();
        ShowCredits();
    }

    public void OnBackPressed()
    {
        PlayButtonSound();
        ShowMainMenu();
    }

    public void OnQuitPressed()
    {
        PlayButtonSound();
        if (GameManager.Instance != null)
            GameManager.Instance.QuitGame();
        else
            Application.Quit();
    }

    // Handles panel switching

    void ShowMainMenu()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (howToPlayPanel != null) howToPlayPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false); 
    }

    void ShowHowToPlay()
    {
        if (howToPlayPanel != null) howToPlayPanel.SetActive(true);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);
    }

    void ShowCredits()
    {
        if (creditsPanel != null) creditsPanel.SetActive(true);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (howToPlayPanel != null) howToPlayPanel.SetActive(false);
    }

    void PlayButtonSound()
    {
        if (buttonSound != null && audioSource != null)
            audioSource.PlayOneShot(buttonSound);
    }
}