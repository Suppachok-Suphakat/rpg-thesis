using System.Collections;
using UnityEngine;

public class Trap : MonoBehaviour
{
    public float trapLifetime = 5f; // Time before the trap self-destructs
    public GameObject trapEffectPrefab; // Reference to the trap effect prefab

    void Start()
    {
        // Destroy the trap after its lifetime
        Destroy(gameObject, trapLifetime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            // Spawn the trap effect if not already applied to this enemy
            if (other.transform.Find("TrapEffect") == null)
            {
                GameObject trapEffect = Instantiate(trapEffectPrefab, other.transform.position, Quaternion.identity);
                trapEffect.name = "TrapEffect"; // Ensure a unique name for tracking
                trapEffect.transform.parent = other.transform; // Attach to the enemy
            }
        }
    }
}
