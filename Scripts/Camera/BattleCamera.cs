// Handles the camera and tracking all player characters and enemies

using UnityEngine;

public class BattleCamera : MonoBehaviour
{
    // Assign all of these parameters in the inspector
    [Header("Targets")]
    public Transform[] playerCharacters;
    public Transform[] enemyCharacters;

    [Header("Follow Settings")]
    public float smoothSpeed = 5f;
    public Vector2 offset = Vector2.zero;

    [Header("Zoom Settings")]
    public bool adjustZoom = true;
    public float minSize = 5f;
    public float maxSize = 10f;
    public float zoomPadding = 2f;
    public float zoomSmoothSpeed = 3f;

    Camera cam;

    void Awake()
    {
        cam = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        // Ensures camera moves after all characters have moved this frame

        Vector3 centerPoint = GetCenterPoint();
        if (centerPoint == Vector3.zero) return;

        Vector3 targetPos = new Vector3(
            centerPoint.x + offset.x,
            centerPoint.y + offset.y,
            transform.position.z
        );

        transform.position = Vector3.Lerp(transform.position, targetPos, smoothSpeed * Time.deltaTime);

        // Optionally adjust zoom to fit all characters
        if (adjustZoom && cam != null)
            AdjustZoom();
    }

    // Calculates the average position of all active characters
    Vector3 GetCenterPoint()
    {
        Vector3 sum = Vector3.zero;
        int count = 0;

        foreach (Transform t in playerCharacters)
        {
            if (t != null && t.gameObject.activeInHierarchy)
            {
                sum += t.position;
                count++;
            }
        }

        foreach (Transform t in enemyCharacters)
        {
            if (t != null && t.gameObject.activeInHierarchy)
            {
                sum += t.position;
                count++;
            }
        }

        if (count == 0) return transform.position; // No targets — stay put
        return sum / count;
    }

    // Zooms to fit all characters within view

    void AdjustZoom()
    {
        // Find the furthest character from the center
        Vector3 center = GetCenterPoint();
        float maxDistance = 0f;

        foreach (Transform t in playerCharacters)
        {
            if (t != null && t.gameObject.activeInHierarchy)
                maxDistance = Mathf.Max(maxDistance, Vector3.Distance(center, t.position));
        }

        foreach (Transform t in enemyCharacters)
        {
            if (t != null && t.gameObject.activeInHierarchy)
                maxDistance = Mathf.Max(maxDistance, Vector3.Distance(center, t.position));
        }

        float targetSize = Mathf.Clamp(maxDistance + zoomPadding, minSize, maxSize);

        if (cam.orthographic)
        {
            // If 2D orthographic camera
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetSize, zoomSmoothSpeed * Time.deltaTime);
        }
        else
        {
            // If 3D perspective camera
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetSize, zoomSmoothSpeed * Time.deltaTime);
        }
    }
}