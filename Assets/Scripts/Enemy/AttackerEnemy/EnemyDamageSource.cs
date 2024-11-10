using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDamageSource : MonoBehaviour
{
    [SerializeField] private int damageAmount;
    [SerializeField] private int knockBackThrust;

    private bool canTakeDamage = true;
    private float damageCooldown = 0.5f;

    private void Start()
    {
        
    }

    private IEnumerator DamageCooldownRoutine()
    {
        canTakeDamage = false;
        yield return new WaitForSeconds(damageCooldown);
        canTakeDamage = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!canTakeDamage) return;

        Character playerHealth = other.gameObject.GetComponent<Character>();
        if (playerHealth != null)
        {
            Debug.Log("HitPlayer");
            playerHealth.TakeDamage(damageAmount, transform);
            StartCoroutine(DamageCooldownRoutine());
        }

        HeroHealth partnerHealth = other.gameObject.GetComponent<HeroHealth>();
        if (partnerHealth != null)
        {
            partnerHealth.TakeDamage(damageAmount, transform);
            StartCoroutine(DamageCooldownRoutine());
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        Debug.Log("Collision detected with: " + other.name);

        Character playerHealth = other.gameObject.GetComponent<Character>();
        if (playerHealth != null)
        {
            Debug.Log("HitPlayer");
            playerHealth.TakeDamage(damageAmount, transform);
        }
        else
        {
            Debug.Log("Character component not found on " + other.name);
        }

        HeroHealth partnerHealth = other.gameObject.GetComponent<HeroHealth>();
        if (partnerHealth != null)
        {
            partnerHealth.TakeDamage(damageAmount, transform);
        }
        else
        {
            Debug.Log("HeroHealth component not found on " + other.name);
        }
    }
}
