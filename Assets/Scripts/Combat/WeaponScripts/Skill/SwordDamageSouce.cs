using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordDamageSouce : MonoBehaviour
{
    public int damageAmount;
    public int chargeAmount;

    private void Start()
    {
        MonoBehaviour currenActiveWeapon = ActiveWeapon.Instance.CurrentActiveWeapon;
        damageAmount = (currenActiveWeapon as IWeapon).GetWeaponInfo().weaponDamage;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        EnemyHealth enemyHealth = other.gameObject.GetComponent<EnemyHealth>();
        enemyHealth?.TakeDamage(damageAmount);

        if (other.gameObject.CompareTag("Enemy"))
        {
            chargeAmount++;
        }
    }
}
