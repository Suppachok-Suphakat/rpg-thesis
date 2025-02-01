using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDamageArea : MonoBehaviour
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
        if (other.CompareTag("Player") || other.CompareTag("Hero"))
        {
            // Spawn the poison effect if not already applied to this enemy
            if (other.transform.Find("EnemyPoisonEffect") == null)
            {
                GameObject poisonEffect = Instantiate(poisonEffectPrefab, other.transform.position, Quaternion.identity);
                poisonEffect.name = "EnemyPoisonEffect"; // Ensure a unique name for tracking
                poisonEffect.transform.parent = other.transform; // Attach to the enemy
            }
        }
    }
}
