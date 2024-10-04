using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trap : MonoBehaviour
{
    public float trapDuration = 5f; // Time the enemy stays trapped
    public float trapLifetime = 5f; // Time before the trap self-destructs

    void Start()
    {
        // Start a timer to destroy the trap even if no enemy steps on it
        StartCoroutine(DestroyTrapAfterLifetime());
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the enemy steps on the trap
        if (other.CompareTag("Enemy"))
        {
            StartCoroutine(ActivateTrap(other));
        }
    }

    IEnumerator ActivateTrap(Collider2D enemy)
    {
        // Disable enemy movement by accessing its script
        AttackerEnemyPathfinding attackerEnemyMovement = enemy.GetComponent<AttackerEnemyPathfinding>();
        if (attackerEnemyMovement != null)
        {
            attackerEnemyMovement.isImmobilized = true; // Custom flag to stop movement
        }

        // Wait for the trap duration to end
        yield return new WaitForSeconds(trapDuration);

        // Re-enable enemy movement
        if (attackerEnemyMovement != null)
        {
            attackerEnemyMovement.isImmobilized = false;
        }

        // Destroy both the trap and its parent object after it's done
        if (transform.parent != null)
        {
            Destroy(transform.parent.gameObject); // Destroy parent and all children
        }
        else
        {
            Destroy(gameObject); // Destroy only the trap if no parent exists
        }
    }

    IEnumerator DestroyTrapAfterLifetime()
    {
        // Wait for the lifetime duration to pass
        yield return new WaitForSeconds(trapLifetime);

        // Destroy both the trap and its parent object after lifetime expires
        if (transform.parent != null)
        {
            Destroy(transform.parent.gameObject); // Destroy parent and all children
        }
        else
        {
            Destroy(gameObject); // Destroy only the trap if no parent exists
        }
    }
}