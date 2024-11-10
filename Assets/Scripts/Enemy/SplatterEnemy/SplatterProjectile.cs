using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplatterProjectile : MonoBehaviour
{
    [SerializeField] private float duration = 1f;
    [SerializeField] private AnimationCurve animCurve;
    [SerializeField] private float heightY = 3f;
    [SerializeField] private GameObject splatterProjectileShadow;
    [SerializeField] private GameObject splatterPrefab;
    [SerializeField] private GameObject landingShadowPrefab; // Reference to the landing shadow prefab
    [SerializeField] private Vector3 maxShadowScale = new Vector3(3f, 3f, 1f); // Maximum size for the shadow

    private GameObject landingShadowInstance;
    private Vector3 targetPosition;

    private void Start()
    {
        // Set the target position to the player's position at the time of firing
        targetPosition = PlayerController.instance.transform.position;

        // Instantiate the moving shadow below the projectile
        GameObject splatterShadow = Instantiate(splatterProjectileShadow, transform.position + new Vector3(0, -0.3f, 0), Quaternion.identity);

        // Instantiate the landing shadow at the target position
        landingShadowInstance = Instantiate(landingShadowPrefab, targetPosition, Quaternion.identity);
        landingShadowInstance.transform.localScale = Vector3.zero; // Start with zero scale

        StartCoroutine(ProjectileCurveRoutine(transform.position, targetPosition));
        StartCoroutine(MoveSplatterShadowRoutine(splatterShadow, splatterShadow.transform.position, targetPosition));
    }

    private IEnumerator ProjectileCurveRoutine(Vector3 startPosition, Vector3 endPosition)
    {
        float timePassed = 0f;

        while (timePassed < duration)
        {
            timePassed += Time.deltaTime;
            float linearT = timePassed / duration;
            float heightT = animCurve.Evaluate(linearT);
            float height = Mathf.Lerp(0f, heightY, heightT);

            // Update projectile position
            transform.position = Vector2.Lerp(startPosition, endPosition, linearT) + new Vector2(0f, height);

            // Scale the landing shadow proportionally to time
            if (landingShadowInstance != null)
            {
                landingShadowInstance.transform.localScale = Vector3.Lerp(Vector3.zero, maxShadowScale, linearT);
            }

            yield return null;
        }

        // Instantiate the splatter at the exact position of the landing shadow
        Instantiate(splatterPrefab, landingShadowInstance.transform.position, Quaternion.identity);

        Destroy(landingShadowInstance); // Remove shadow upon landing
        Destroy(gameObject);
    }

    private IEnumerator MoveSplatterShadowRoutine(GameObject splatterShadow, Vector3 startPosition, Vector3 endPosition)
    {
        float timePassed = 0f;

        while (timePassed < duration)
        {
            timePassed += Time.deltaTime;
            float linearT = timePassed / duration;
            splatterShadow.transform.position = Vector2.Lerp(startPosition, endPosition, linearT);
            yield return null;
        }

        Destroy(splatterShadow);
    }
}
