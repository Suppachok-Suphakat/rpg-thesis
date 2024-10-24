using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CinemachineEnemyDetection : MonoBehaviour
{
    public CinemachineVirtualCamera cinemachineCam;
    public float expandedFieldOfView = 10f;  // Zoomed out size
    public float normalFieldOfView = 6f;     // Normal zoom size
    public float zoomSpeed = 2f;             // Speed of zoom transition

    private float targetFieldOfView;         // Field of view to zoom towards

    void Start()
    {
        // Set initial field of view to normal size
        targetFieldOfView = normalFieldOfView;
    }

    void Update()
    {
        // Smoothly transition to the target field of view using Lerp
        cinemachineCam.m_Lens.OrthographicSize = Mathf.Lerp(
            cinemachineCam.m_Lens.OrthographicSize,
            targetFieldOfView,
            Time.deltaTime * zoomSpeed
        );
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy")) // Assumes the enemy has the "Enemy" tag
        {
            // When an enemy is detected, set the target to the expanded FOV
            targetFieldOfView = expandedFieldOfView;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            // When the enemy leaves, return to normal field of view
            targetFieldOfView = normalFieldOfView;
        }
    }
}
