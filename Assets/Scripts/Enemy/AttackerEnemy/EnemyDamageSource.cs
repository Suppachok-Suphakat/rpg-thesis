using System.Collections;
using UnityEngine;

public class EnemyDamageSource : MonoBehaviour
{
    [SerializeField] private int damageAmount;
    [SerializeField] private int knockBackThrust;

    private bool canTakeDamage = true;
    private float damageCooldown = 0.5f;
    private Collider2D damageCollider;

    private void Awake()
    {
        damageCollider = GetComponent<Collider2D>();
    }

    private void OnEnable()
    {
        if (damageCollider != null)
        {
            // Check for overlapping objects when collider is enabled
            Collider2D[] colliders = Physics2D.OverlapBoxAll(damageCollider.bounds.center, damageCollider.bounds.size, 0f);
            foreach (var collider in colliders)
            {
                ApplyDamage(collider);
            }
        }
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
        ApplyDamage(other);
        StartCoroutine(DamageCooldownRoutine());
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        ApplyDamage(other);
    }

    private void ApplyDamage(Collider2D other)
    {
        Character playerHealth = other.gameObject.GetComponent<Character>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damageAmount, transform);
        }

        HeroHealth partnerHealth = other.gameObject.GetComponent<HeroHealth>();
        if (partnerHealth != null)
        {
            partnerHealth.TakeDamage(damageAmount, transform);
        }
    }
}