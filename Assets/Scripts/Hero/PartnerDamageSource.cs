using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartnerDamageSource : MonoBehaviour
{
    [SerializeField] private int damageAmount;
    private Dictionary<GameObject, float> damageCooldowns = new Dictionary<GameObject, float>();
    [SerializeField] private float damageCooldown = 1.0f; // Delay before hitting the same enemy again

    private void OnTriggerEnter2D(Collider2D other)
    {
        EnemyHealth enemyHealth = other.gameObject.GetComponent<EnemyHealth>();
        enemyHealth?.TakeDamage(damageAmount);
        damageCooldowns[other.gameObject] = Time.time + damageCooldown;
    }
}
