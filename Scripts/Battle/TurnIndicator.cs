// This file handles the turn indicator text that says player or enemy turn 

using System.Collections;
using UnityEngine;

public class TurnIndicator : MonoBehaviour
{
    // Assign this stuff in the inspector, text sprites and stuff

    [Header("Sprites")]
    public SpriteRenderer playerTurnSprite;
    public SpriteRenderer enemyTurnSprite;

    // Tune the settings of the animation in the inspector

    [Header("Animation Settings")]
    public float spinInDuration = 0.4f;
    public float holdDuration = 0.8f;
    public float slideOutDuration = 0.3f;
    public float slideDistance = 12f;

    [Header("Spin Settings")]
    public float spinStartRotation = 720f;
    public Vector3 spawnOffset = new Vector3(-8f, 0f, 0f);

    [Header("Vertical Offset")]
    public float verticalOffset = 0f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip playerTurnSound;
    public AudioClip enemyTurnSound;

    Camera mainCamera;

    void Awake()
    {
        mainCamera = Camera.main;

        if (playerTurnSprite != null) playerTurnSprite.gameObject.SetActive(false);
        if (enemyTurnSprite != null) enemyTurnSprite.gameObject.SetActive(false);
    }

    // Get camera center in world space — called fresh each time so camera movement is tracked
    // This way the indicator is centered on the camera, not the world space
    
    Vector3 GetScreenCenter()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera == null)
            return Vector3.zero;

        // Convert screen center to world position so indicator knows where to go
        Vector3 screenCenter = new Vector3(
            Screen.width / 2f,
            Screen.height / 2f,
            Mathf.Abs(mainCamera.transform.position.z)
        );

        Vector3 worldCenter = mainCamera.ScreenToWorldPoint(screenCenter);
        worldCenter.z = 0f;
        worldCenter.y += verticalOffset;
        return worldCenter;
    }

    // Entry points
    
    public void ShowPlayerTurn()
    {
        StartCoroutine(AnimateTurnIndicator(playerTurnSprite, playerTurnSound, slideRight: true));
    }

    public void ShowEnemyTurn()
    {
        StartCoroutine(AnimateTurnIndicator(enemyTurnSprite, enemyTurnSound, slideRight: false));
    }

    //  Main animation

    IEnumerator AnimateTurnIndicator(SpriteRenderer sprite, AudioClip sound, bool slideRight)
    {
        if (sprite == null) yield break;

        // Calculates center fresh each time by tracking the camera position
        Vector3 centerPosition = GetScreenCenter();

        sprite.gameObject.SetActive(true);
        sprite.transform.position = centerPosition + spawnOffset;
        sprite.transform.rotation = Quaternion.Euler(0f, 0f, spinStartRotation);

        Color c = sprite.color;
        c.a = 1f;
        sprite.color = c;

        if (sound != null && audioSource != null)
            audioSource.PlayOneShot(sound);

        // Inital spin in
        float elapsed = 0f;
        Vector3 startPos = centerPosition + spawnOffset;

        while (elapsed < spinInDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / spinInDuration);
            float smoothT = 1f - Mathf.Pow(1f - t, 3f);

            // Recalculate center each frame so it tracks camera movement
            centerPosition = GetScreenCenter();

            sprite.transform.position = Vector3.Lerp(startPos, centerPosition, smoothT);
            float currentRotation = Mathf.Lerp(spinStartRotation, 0f, smoothT);
            sprite.transform.rotation = Quaternion.Euler(0f, 0f, currentRotation);

            yield return null;
        }

        centerPosition = GetScreenCenter();
        sprite.transform.position = centerPosition;
        sprite.transform.rotation = Quaternion.identity;

        // Hold in place
        // Keep snapping to center during hold in case camera moves
        elapsed = 0f;
        while (elapsed < holdDuration)
        {
            elapsed += Time.deltaTime;
            sprite.transform.position = GetScreenCenter();
            yield return null;
        }

        // Slide off
        elapsed = 0f;
        Vector3 slideStart = GetScreenCenter();
        Vector3 slideTarget = slideStart + new Vector3(slideRight ? slideDistance : -slideDistance, 0f, 0f);

        while (elapsed < slideOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / slideOutDuration);
            float smoothT = t * t;
            sprite.transform.position = Vector3.Lerp(slideStart, slideTarget, smoothT);
            yield return null;
        }

        sprite.gameObject.SetActive(false);
        sprite.transform.rotation = Quaternion.identity;
    }
}