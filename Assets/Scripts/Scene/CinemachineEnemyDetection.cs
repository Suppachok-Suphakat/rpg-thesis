using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CinemachineEnemyDetection : MonoBehaviour
{
    public CinemachineVirtualCamera cinemachineCam;
    public float expandedFieldOfView = 10f;    // Zoomed out size when detecting enemies
    public float normalFieldOfView = 6f;       // Normal zoom size
    public float zoomSpeed = 2f;               // Speed of automatic zoom transition
    public float manualZoomSpeed = 5f;         // Speed of manual zoom with mouse scroll
    public float minZoom = 3f;                 // Minimum zoom size (closest)
    public float maxZoom = 15f;                // Maximum zoom size (farthest)

    private float targetFieldOfView;           // Field of view camera is zooming towards
    private bool isManualZooming = false;      // Flag to check if the player is manually zooming

    void Start()
    {
        // Initialize targetFieldOfView to the normal camera size
        targetFieldOfView = normalFieldOfView;
    }

    void Update()
    {
        HandleManualZoom(); // Handle mouse scroll for manual zoom

        // Only apply automatic zoom if the player is not manually zooming
        if (!isManualZooming)
        {
            // Smoothly transition to the target field of view using Lerp
            cinemachineCam.m_Lens.OrthographicSize = Mathf.Lerp(
                cinemachineCam.m_Lens.OrthographicSize,
                targetFieldOfView,
                Time.deltaTime * zoomSpeed
            );
        }
    }

    void HandleManualZoom()
    {
        // Check for mouse scroll input
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");

        if (scrollInput != 0f) // If there is scroll input
        {
            isManualZooming = true; // The player is manually zooming

            // Adjust the field of view based on scroll input and manual zoom speed
            float currentZoom = cinemachineCam.m_Lens.OrthographicSize;
            currentZoom -= scrollInput * manualZoomSpeed;
            currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom); // Clamp zoom to min/max limits

            // Apply the new zoom level
            cinemachineCam.m_Lens.OrthographicSize = currentZoom;

            // Update targetFieldOfView to match current zoom to avoid snapping back
            targetFieldOfView = currentZoom;
        }

        // If the player is no longer scrolling, stop manual zooming
        if (scrollInput == 0f)
        {
            isManualZooming = false; // Stop manual zooming when there's no input
        }
    }

    //void OnTriggerEnter2D(Collider2D other)
    //{
    //    if (other.CompareTag("Enemy"))
    //    {
    //        // When detecting an enemy, set the target field of view to zoom out
    //        targetFieldOfView = expandedFieldOfView;
    //        isManualZooming = false; // Disable manual zoom when auto zoom is triggered
    //    }
    //}

    //void OnTriggerExit2D(Collider2D other)
    //{
    //    if (other.CompareTag("Enemy"))
    //    {
    //        // When the enemy is no longer detected, return to the normal field of view
    //        targetFieldOfView = normalFieldOfView;
    //        isManualZooming = false; // Disable manual zoom when auto zoom is triggered
    //    }
    //}
}
