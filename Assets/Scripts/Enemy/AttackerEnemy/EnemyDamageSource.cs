using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDamageSource : MonoBehaviour
{
    [SerializeField] private int damageAmount;
    [SerializeField] private int knockBackThrust;

    private void Start()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Character playerHealth = other.gameObject.GetComponent<Character>();
        playerHealth?.TakeDamage(damageAmount, transform);

        HeroHealth partnerHealth = other.gameObject.GetComponent<HeroHealth>();
        partnerHealth?.TakeDamage(damageAmount, transform);

        //Knockback knockback = other.gameObject.GetComponent<Knockback>();
        //knockback.GetKnockedBack(transform, this.knockBackThrust);
    }
}
