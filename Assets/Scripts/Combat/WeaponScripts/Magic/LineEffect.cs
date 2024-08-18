using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineEffect : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public Transform target;
    public float lineSpeed = 10f;

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
            // Continuously update the line's start position
            lineRenderer.SetPosition(0, transform.position);

            // Directly set the line's end position to the target's position
            lineRenderer.SetPosition(1, target.position);
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
        // Optionally, reset the line renderer or perform other cleanup actions here
        lineRenderer.SetPosition(1, transform.position); // Reset the end position if needed
    }
}