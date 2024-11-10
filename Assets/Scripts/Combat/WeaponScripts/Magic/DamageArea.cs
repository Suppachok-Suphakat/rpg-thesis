using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageArea : MonoBehaviour
{
    public float trapDuration = 5f; // Duration the enemy stays trapped and poisoned
    public float trapLifetime = 5f; // Time before the trap self-destructs
    public float poisonDamage = 10f; // Amount of poison damage per tick
    public float damageInterval = 1f; // Time between poison damage ticks
    public GameObject trapEffectPrefab; // Reference to the trap effect prefab

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
        // Instantiate the trap effect at the enemy's position
        GameObject trapEffect = Instantiate(trapEffectPrefab, enemy.transform.position, Quaternion.identity);
        trapEffect.transform.parent = enemy.transform; // Attach effect to the enemy

        // Start poison damage coroutine
        StartCoroutine(ApplyPoisonDamage(enemy));

        // Wait for the trap duration to end
        yield return new WaitForSeconds(trapDuration);

        // Destroy the trap effect and trap itself
        Destroy(trapEffect);

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

    IEnumerator ApplyPoisonDamage(Collider2D enemy)
    {
        EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();

        // Apply poison damage at intervals while the trap is active
        float elapsed = 0f;
        while (elapsed < trapDuration)
        {
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamageNoCamShake(Mathf.RoundToInt(poisonDamage)); // Apply poison damage
            }

            // Wait for the next tick
            yield return new WaitForSeconds(damageInterval);
            elapsed += damageInterval;
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
