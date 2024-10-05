using UnityEngine;

public class ShadowTrigger : MonoBehaviour
{
    private Material playerMaterial;
    private Color originalColor;
    private Color shadowColor = new Color(0.5f, 0.5f, 0.5f, 1f); // Adjust shadow color as needed
    private bool inShadow = false;

    void Start()
    {
        // Assuming the player has a tag called "Player"
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerMaterial = player.GetComponent<SpriteRenderer>().material;
            originalColor = playerMaterial.GetColor("_ShadowColor");
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            inShadow = true;
            // Apply shadow effect
            playerMaterial.SetColor("_ShadowColor", shadowColor);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            inShadow = false;
            // Revert to original color
            playerMaterial.SetColor("_ShadowColor", originalColor);
        }
    }

    void Update()
    {
        // Optional: Gradually blend the effect over time
        if (inShadow)
        {
            playerMaterial.SetColor("_ShadowColor", Color.Lerp(playerMaterial.GetColor("_ShadowColor"), shadowColor, Time.deltaTime * 2));
        }
        else
        {
            playerMaterial.SetColor("_ShadowColor", Color.Lerp(playerMaterial.GetColor("_ShadowColor"), originalColor, Time.deltaTime * 2));
        }
    }
}
