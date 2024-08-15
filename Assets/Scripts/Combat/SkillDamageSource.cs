using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillDamageSource : MonoBehaviour
{
    [SerializeField] private int damageAmount;

    private void Start()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        EnemyHealth enemyHealth = other.gameObject.GetComponent<EnemyHealth>();
        enemyHealth?.TakeDamage(damageAmount);
    }
}
