using System.Collections;
using UnityEngine;

public class PoisonEffect : MonoBehaviour
{
    public float poisonDuration = 5f; // How long the poison lasts
    public float poisonDamage = 10f; // Damage per tick
    public float damageInterval = 1f; // Time between damage ticks

    private EnemyHealth enemyHealth;
    private Character playerHealth;
    private HeroHealth heroHealth;

    void Start()
    {
        enemyHealth = GetComponentInParent<EnemyHealth>();
        if (enemyHealth != null)
        {
            StartCoroutine(ApplyPoison());
        }

        playerHealth = GetComponentInParent<Character>();
        if (playerHealth != null)
        {
            StartCoroutine(ApplyPoisonPlayer());
        }

        heroHealth = GetComponentInParent<HeroHealth>();
        if(heroHealth != null)
        {
            StartCoroutine(ApplyPoisonHero());
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

    private IEnumerator ApplyPoisonPlayer()
    {
        float elapsed = 0f;

        // Apply damage over time
        while (elapsed < poisonDuration)
        {
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(Mathf.RoundToInt(poisonDamage), this.transform);
            }

            elapsed += damageInterval;
            yield return new WaitForSeconds(damageInterval);
        }

        // Poison effect ends; destroy this GameObject
        Destroy(gameObject);
    }

    private IEnumerator ApplyPoisonHero()
    {
        float elapsed = 0f;

        // Apply damage over time
        while (elapsed < poisonDuration)
        {
            if (heroHealth != null)
            {
                heroHealth.TakeDamage(Mathf.RoundToInt(poisonDamage), this.transform);
            }

            elapsed += damageInterval;
            yield return new WaitForSeconds(damageInterval);
        }

        // Poison effect ends; destroy this GameObject
        Destroy(gameObject);
    }
}
