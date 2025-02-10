using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineEffect : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public Transform target;
    public float lineSpeed = 10f;

    public float maxWidth = 1f;  // Thickest width when close
    public float minWidth = 0.5f; // Thinnest width when far
    public float maxDistance = 10f; // Distance at which the line becomes the thinnest

    private Vector3 initialPosition;
    private bool isHealing;

    void Start()
    {
        if (lineRenderer == null)
            lineRenderer = GetComponent<LineRenderer>();

        lineRenderer.positionCount = 2;
        initialPosition = transform.position;
        lineRenderer.SetPosition(0, initialPosition);
        lineRenderer.SetPosition(1, initialPosition);
        isHealing = false;
    }

    void Update()
    {
        if (isHealing && target != null)
        {
            // Update line positions
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, target.position);

            // Adjust line width based on distance
            float distance = Vector3.Distance(transform.position, target.position);
            float newWidth = Mathf.Lerp(maxWidth, minWidth, distance / maxDistance);
            lineRenderer.startWidth = newWidth;
            lineRenderer.endWidth = newWidth;
        }
    }

    public void StartHealing(Transform newTarget)
    {
        target = newTarget;
        isHealing = true;
    }

    public void StopHealing()
    {
        isHealing = false;
        lineRenderer.SetPosition(1, transform.position); // Reset end position
    }
}
