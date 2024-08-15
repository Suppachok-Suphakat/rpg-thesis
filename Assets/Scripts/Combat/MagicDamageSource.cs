using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicDamageSource : MonoBehaviour
{
    [SerializeField] private int damageAmount;

    [SerializeField] private GameObject sliderObject;
    [SerializeField] private SkillStatusBar statusComponent;
    public int chargeAmount;

    private void Start()
    {
        MonoBehaviour currenActiveWeapon = ActiveWeapon.Instance.CurrentActiveWeapon;
        damageAmount = (currenActiveWeapon as IWeapon).GetWeaponInfo().weaponDamage;

        sliderObject = GameObject.Find("MagicWeaponSkillBar");
        statusComponent = sliderObject.GetComponent<SkillStatusBar>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        EnemyHealth enemyHealth = other.gameObject.GetComponent<EnemyHealth>();
        enemyHealth?.TakeDamage(damageAmount);

        if (other.gameObject.CompareTag("Enemy"))
        {
            statusComponent.chargeAmount++;
        }
    }
}
