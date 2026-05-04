// File for managing audio in battle
// Set music and stuff in inspector
// Some music is handled in WaveManager now so this script isn't used as much

using System.Collections;
using UnityEngine;

public class BattleAudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource sfxSource;
    public AudioSource musicSource;

    [Header("Battle Start")]
    public AudioClip alertSound;
    public AudioClip battleMusic;
    public float delayBeforeMusic = 0f;

    void Start()
    {
        StartCoroutine(PlayBattleIntro());
    }

    IEnumerator PlayBattleIntro()
    {
        // Play the alert sound before battle (not being used right now b/c of the waves)
        if (alertSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(alertSound);

            yield return new WaitForSeconds(alertSound.length + delayBeforeMusic);
        }

        if (battleMusic != null && musicSource != null && !musicSource.isPlaying)
        {
            // Only play music if WaveManager is not present
            if (FindObjectOfType<WaveManager>() == null)
            {
                musicSource.clip = battleMusic;
                musicSource.loop = true;
                musicSource.Play();
            }
        }
    }

    // Methods for other scripts to us

    public void StopMusic()
    {
        if (musicSource != null)
            musicSource.Stop();
    }

    public void FadeOutMusic(float duration)
    {
        StartCoroutine(FadeOut(duration));
    }

    IEnumerator FadeOut(float duration)
    {
        float startVolume = musicSource.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            yield return null;
        }

        musicSource.Stop();
        musicSource.volume = startVolume;
    }
}