using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoKBDamageSource : MonoBehaviour
{
    [SerializeField] private int damageAmount;
    private bool canDamage = true;

    private void Start()
    {

    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (canDamage)
        {
            EnemyHealth enemyHealth = other.gameObject.GetComponent<EnemyHealth>();
            enemyHealth?.TakeDamageNoKnockBack(damageAmount);
            StartCoroutine(DamageCooldown());
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        Debug.Log("TriggerStay2D");
        if (canDamage)
        {
            EnemyHealth enemyHealth = other.gameObject.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamageNoKnockBack(damageAmount);
                StartCoroutine(DamageCooldown());
            }
            else
            {
                Debug.Log("EnemyHealth component not found on collided object.");
            }
        }
    }

    private IEnumerator DamageCooldown()
    {
        canDamage = false;
        yield return new WaitForSeconds(1f); // Adjust cooldown duration as needed
        canDamage = true;
    }
}
