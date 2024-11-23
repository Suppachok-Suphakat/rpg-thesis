using System.Collections;
using UnityEngine;

public class DamageArea : MonoBehaviour
{
    public float trapLifetime = 5f; // How long the trap exists
    public GameObject poisonEffectPrefab; // The poison effect to spawn

    void Start()
    {
        // Destroy the trap after its lifetime
        Destroy(gameObject, trapLifetime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            // Spawn the poison effect if not already applied to this enemy
            if (other.transform.Find("PoisonEffect") == null)
            {
                GameObject poisonEffect = Instantiate(poisonEffectPrefab, other.transform.position, Quaternion.identity);
                poisonEffect.name = "PoisonEffect"; // Ensure a unique name for tracking
                poisonEffect.transform.parent = other.transform; // Attach to the enemy
            }
        }
    }
}
