// Code for the targeting indicator above enemies' heads
// Set parameters in the inspector

using System.Collections;
using UnityEngine;

public class TargetIndicator : MonoBehaviour
{
    [Header("Position Settings")]
    public float heightAboveEnemy = 1.5f;
    public float followSpeed = 10f;

    [Header("Bob Animation")]
    public bool bob = true;
    public float bobHeight = 0.15f;
    public float bobSpeed = 2f;

    Transform currentTarget;
    Vector3 basePosition;

    void Update()
    {
        if (currentTarget == null) return;

        // Snap to above the target
        basePosition = currentTarget.position + new Vector3(0f, heightAboveEnemy, 0f);
        Vector3 bobOffset = bob ? new Vector3(0f, Mathf.Sin(Time.time * bobSpeed) * bobHeight, 0f) : Vector3.zero;

        transform.position = Vector3.Lerp(transform.position, basePosition + bobOffset, followSpeed * Time.deltaTime);
    }

    // Call this to move the indicator to a new enemy
    public void SetTarget(Transform target)
    {
        currentTarget = target;
    }

    public void Show() => gameObject.SetActive(true);
    public void Hide() => gameObject.SetActive(false);
}
