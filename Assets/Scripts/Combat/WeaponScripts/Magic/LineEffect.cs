using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineEffect : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public Transform target;
    public float lineSpeed = 10f;

    public float maxOpacity = 1f;  // Maximum opacity when close
    public float minOpacity = 0.1f; // Minimum opacity when far
    public float maxDistance = 10f; // Distance at which the opacity becomes the minimum

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

        // Ensure the LineRenderer uses a material with transparency
        var material = lineRenderer.material;
        if (material != null && material.HasProperty("_Color"))
        {
            material.SetFloat("_Mode", 3); // Set to Transparent mode (assuming the shader supports it)
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = 3000;
        }
    }

    void Update()
    {
        if (isHealing && target != null)
        {
            // Update line positions
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, target.position);

            // Adjust line opacity based on distance
            float distance = Vector3.Distance(transform.position, target.position);
            float newOpacity = Mathf.Lerp(maxOpacity, minOpacity, distance / maxDistance);

            Color startColor = lineRenderer.startColor;
            Color endColor = lineRenderer.endColor;

            // Update opacity (alpha value)
            startColor.a = newOpacity;
            endColor.a = newOpacity;

            lineRenderer.startColor = startColor;
            lineRenderer.endColor = endColor;
        }
    }

    public void StartHealing(Transform newTarget)
    {
        target = newTarget; // Assign the new target first
        isHealing = true;

        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, target.position);
    }


    public void StopHealing()
    {
        isHealing = false;
        lineRenderer.SetPosition(1, transform.position); // Reset end position
    }
}
