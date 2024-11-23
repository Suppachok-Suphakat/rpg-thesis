using System.Collections;
using UnityEngine;

public class PoisonEffect : MonoBehaviour
{
    public float poisonDuration = 5f; // How long the poison lasts
    public float poisonDamage = 10f; // Damage per tick
    public float damageInterval = 1f; // Time between damage ticks

    private EnemyHealth enemyHealth;

    void Start()
    {
        // Find the EnemyHealth component on the parent (the enemy)
        enemyHealth = GetComponentInParent<EnemyHealth>();

        if (enemyHealth != null)
        {
            StartCoroutine(ApplyPoison());
        }
    }

    private IEnumerator ApplyPoison()
    {
        float elapsed = 0f;

        // Apply damage over time
        while (elapsed < poisonDuration)
        {
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamageNoCamShake(Mathf.RoundToInt(poisonDamage)); // Apply damage
            }

            elapsed += damageInterval;
            yield return new WaitForSeconds(damageInterval);
        }

        // Poison effect ends; destroy this GameObject
        Destroy(gameObject);
    }
}
