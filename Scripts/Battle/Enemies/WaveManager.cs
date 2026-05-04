// Late addition file made so that there are waves of enemies in the battle
// Controls everythig regarding this, such as player turns, enemy turns, music and stuff

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [Header("Wave Settings")]
    public int totalWaves = 3;

    [Header("Enemy Setup")]
    public GameObject enemyPrefab;
    public EnemyData[] regularEnemyTypes;
    public EnemyData[] bossWaveEnemyTypes;
    public Transform[] spawnPoints;

    [Header("Spawn Settings")]
    public int minEnemies = 1;
    public int maxEnemies = 4;

    [Header("References")]
    public EnemyAttackManager enemyAttackManager;
    public EnemyTargetingController targetingController;
    public PlayerAttackManager playerAttackManager;
    public BattleCamera battleCamera;
    public BattleAudioManager battleAudioManager;

    [Header("Wave Transition")]
    public float delayBetweenWaves = 2f;
    public GameObject waveAnnouncementPanel;
    public UnityEngine.UI.Image waveAnnouncementImage;
    public float announcementDuration = 1.5f;
    public AudioClip waveStartSound;
    public AudioClip finalWaveStartSound;

    [Header("Wave Music")]
    public AudioClip[] waveMusicTracks;
    public AudioClip bossWaveMusic;
    public float musicFadeTime = 1f;

    [Header("Victory")]
    public GameObject victoryPanel;
    public AudioClip victoryMusic;
    public AudioClip victorySound;
    public AudioSource audioSource;
    public float delayBeforeVictory = 1.5f;

    int currentWave = 0;
    bool waveActive = false;
    bool waveClearProcessing = false;
    List<GameObject> spawnedEnemies = new List<GameObject>();

    // Start
    void Start()
    {
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (waveAnnouncementPanel != null) waveAnnouncementPanel.SetActive(false);

        StartCoroutine(StartNextWave());
    }

    // Poll for wave clear
    void Update()
    {
        // Only check when a wave is active and not already processing
        if (!waveActive || waveClearProcessing) return;

        // Check if all spawned enemies are gone
        bool allGone = true;
        foreach (GameObject enemy in spawnedEnemies)
        {
            if (enemy != null && enemy.activeInHierarchy)
            {
                allGone = false;
                break;
            }
        }

        if (allGone && spawnedEnemies.Count > 0)
        {
            waveClearProcessing = true;
            waveActive = false;
            StartCoroutine(OnWaveCleared());
        }
    }

    // Wave progression
    IEnumerator StartNextWave()
    {
        currentWave++;
        waveClearProcessing = false;

        // Show announcement
        // Play wave start sound
        bool isFinal = (currentWave == totalWaves);
        AudioClip soundToPlay = (isFinal && finalWaveStartSound != null)
            ? finalWaveStartSound
            : waveStartSound;

        if (soundToPlay != null && audioSource != null)
            audioSource.PlayOneShot(soundToPlay);

        if (waveAnnouncementPanel != null)
        {
            waveAnnouncementPanel.SetActive(true);
            yield return new WaitForSeconds(announcementDuration);
            waveAnnouncementPanel.SetActive(false);
        }

        // Pick pool
        bool isFinalWave = (currentWave == totalWaves);
        EnemyData[] pool = (isFinalWave && bossWaveEnemyTypes != null && bossWaveEnemyTypes.Length > 0)
            ? bossWaveEnemyTypes
            : regularEnemyTypes;

        SpawnWave(pool);
        StartCoroutine(PlayWaveMusic());

        // Small delay before handing control to player
        yield return new WaitForSeconds(0.1f);

        waveActive = true;
        playerAttackManager.StartPlayerTurn();
    }

    // Spawn enemies
    void SpawnWave(EnemyData[] pool)
    {
        foreach (GameObject enemy in spawnedEnemies)
        {
            if (enemy != null) Destroy(enemy);
        }
        spawnedEnemies.Clear();

        if (pool == null || pool.Length == 0)
        {
            return;
        }

        int count = Random.Range(minEnemies, Mathf.Min(maxEnemies, spawnPoints.Length) + 1);
        List<Transform> newEnemies = new List<Transform>();

        for (int i = 0; i < count; i++)
        {
            EnemyData randomType = pool[Random.Range(0, pool.Length)];
            GameObject enemyObj = Instantiate(enemyPrefab, spawnPoints[i].position, Quaternion.identity);
            enemyObj.name = $"{randomType.enemyName}_{i}";

            EnemyInstance instance = enemyObj.GetComponent<EnemyInstance>();
            if (instance != null)
            {
                instance.data = randomType;
                instance.originalPosition = spawnPoints[i].position;

                SpriteRenderer sr = enemyObj.GetComponent<SpriteRenderer>();
                if (sr != null && randomType.sprite != null)
                    sr.sprite = randomType.sprite;
            }

            BattleUnit unit = enemyObj.GetComponent<BattleUnit>();
            if (unit != null)
            {
                unit.InitializeHealth(randomType.maxHealth);
                // No waveManager reference needed — polling handles wave clear
            }

            spawnedEnemies.Add(enemyObj);
            newEnemies.Add(enemyObj.transform);
        }

        Transform[] enemyArray = newEnemies.ToArray();
        enemyAttackManager.enemies = enemyArray;
        enemyAttackManager.CacheOriginalPositions();

        if (targetingController != null)
            targetingController.InitializeTargets();

        if (battleCamera != null)
            battleCamera.enemyCharacters = enemyArray;
    }

    IEnumerator OnWaveCleared()
    {
        // Notify targeting controller to clean up
        if (targetingController != null)
            targetingController.OnEnemyDefeated();

        if (currentWave >= totalWaves)
        {
            yield return StartCoroutine(TriggerVictory());
        }
        else
        {
            yield return new WaitForSeconds(delayBetweenWaves);
            StartCoroutine(StartNextWave());
        }
    }

    // Wave music, handles all music regarding the wave

    IEnumerator PlayWaveMusic()
    {
        if (battleAudioManager == null) yield break;

        bool isFinalWave = (currentWave == totalWaves);
        AudioClip track = null;

        if (isFinalWave && bossWaveMusic != null)
            track = bossWaveMusic;
        else if (waveMusicTracks != null && waveMusicTracks.Length > 0)
            track = waveMusicTracks[Mathf.Min(currentWave - 1, waveMusicTracks.Length - 1)];

        if (track == null) yield break;

        // Prevents restaring the same track
        if (battleAudioManager.musicSource.clip == track && battleAudioManager.musicSource.isPlaying)
            yield break;

        if (currentWave == 1 && battleAudioManager.sfxSource != null)
            yield return new WaitUntil(() => !battleAudioManager.sfxSource.isPlaying);

        // Fade out current
        if (battleAudioManager.musicSource.isPlaying)
        {
            battleAudioManager.FadeOutMusic(musicFadeTime);
            yield return new WaitForSeconds(musicFadeTime);
        }

        battleAudioManager.musicSource.Stop();
        battleAudioManager.musicSource.clip = track;
        battleAudioManager.musicSource.loop = true;
        battleAudioManager.musicSource.volume = 1f;
        battleAudioManager.musicSource.Play();
    }

    // Handles victory

    IEnumerator TriggerVictory()
    {
        yield return new WaitForSeconds(delayBeforeVictory);

        if (battleAudioManager != null)
            battleAudioManager.FadeOutMusic(1f);

        yield return new WaitForSeconds(1f);

        if (victorySound != null && audioSource != null)
            audioSource.PlayOneShot(victorySound);

        if (victoryMusic != null && battleAudioManager != null)
        {
            yield return new WaitForSeconds(victorySound != null ? victorySound.length : 0f);
            battleAudioManager.musicSource.clip = victoryMusic;
            battleAudioManager.musicSource.loop = true;
            battleAudioManager.musicSource.volume = 1f;
            battleAudioManager.musicSource.Play();
        }

        if (victoryPanel != null)
            victoryPanel.SetActive(true);
    }

    // Handles buttons
    public void OnMainMenuPressed()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.ReturnToMainMenu();
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    public void OnPlayAgainPressed()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}