using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartnerDamageSource : MonoBehaviour
{
    [SerializeField] private int damageAmount;
    [SerializeField] private int knockBackThrust;

    private bool canTakeDamage = true;
    public float damageCooldown = 0.5f;
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
        if (!canTakeDamage) return;
        ApplyDamage(other);
        StartCoroutine(DamageCooldownRoutine());
    }

    private void ApplyDamage(Collider2D other)
    {
        EnemyHealth enemyHealth = other.gameObject.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(damageAmount);
        }
    }
}
