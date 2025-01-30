using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TransparentDetection : MonoBehaviour
{
    [Range(0, 1)]
    [SerializeField] private float transparencyAmount = 0.8f;
    [SerializeField] private float fadeTime = .4f;

    //[SerializeField] private string changeSortingLayer = "Highground";
    //[SerializeField] private string originalSortingLayer = "Foreground";

    private SpriteRenderer spriteRenderer;
    private Tilemap tilemap;

    private int objectsInTrigger = 0; // Track number of objects (Player and Hero)

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        tilemap = GetComponent<Tilemap>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.GetComponent<PlayerController>() || other.gameObject.tag == "Hero")
        {
            objectsInTrigger++; // Increase count when Player or Hero enters

            //ChangeSortingLayer(changeSortingLayer);

            if (spriteRenderer)
            {
                StartCoroutine(FadeRoutine(spriteRenderer, fadeTime, spriteRenderer.color.a, transparencyAmount));
            }
            else if (tilemap)
            {
                StartCoroutine(FadeRoutine(tilemap, fadeTime, tilemap.color.a, transparencyAmount));
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.GetComponent<PlayerController>() || other.gameObject.tag == "Hero")
        {
            objectsInTrigger--; // Decrease count when Player or Hero exits

            // Only change back when no objects are inside the trigger
            if (objectsInTrigger <= 0)
            {
                //ChangeSortingLayer(originalSortingLayer);

                if (spriteRenderer)
                {
                    StartCoroutine(FadeRoutine(spriteRenderer, fadeTime, spriteRenderer.color.a, 1f));
                }
                else if (tilemap)
                {
                    StartCoroutine(FadeRoutine(tilemap, fadeTime, tilemap.color.a, 1f));
                }
            }
        }
    }

    private void ChangeSortingLayer(string layerName)
    {
        if (spriteRenderer)
        {
            spriteRenderer.sortingLayerName = layerName;
        }
        else if (tilemap)
        {
            tilemap.GetComponent<Renderer>().sortingLayerName = layerName;
        }
    }

    private IEnumerator FadeRoutine(SpriteRenderer spriteRenderer, float fadeTime, float startValue, float targetTransparency)
    {
        float elapsedTime = 0;
        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startValue, targetTransparency, elapsedTime / fadeTime);
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, newAlpha);
            yield return null;
        }
    }

    private IEnumerator FadeRoutine(Tilemap tilemap, float fadeTime, float startValue, float targetTransparency)
    {
        float elapsedTime = 0;
        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startValue, targetTransparency, elapsedTime / fadeTime);
            tilemap.color = new Color(tilemap.color.r, tilemap.color.g, tilemap.color.b, newAlpha);
            yield return null;
        }
    }
}