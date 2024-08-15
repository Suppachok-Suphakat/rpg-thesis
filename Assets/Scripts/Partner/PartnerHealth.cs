using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartnerHealth : MonoBehaviour
{
    public static PartnerHealth Instance;

    [SerializeField] private int startingHealth = 3;
    [SerializeField] private GameObject deathVFXPrefab;
    [SerializeField] private float knockBackThrust = 15f;

    private int currentHealth;
    private Knockback knockback;
    private Flash flash;

    PartnerSkillManager partnerSkillManager;
    [SerializeField] private StatusBar hpStatusComponent;

    private void Awake()
    {
        Instance = this;

        flash = GetComponent<Flash>();
        knockback = GetComponent<Knockback>();
    }

    private void Start()
    {
        currentHealth = startingHealth;

        partnerSkillManager = GameObject.Find("PartnerCanvas").GetComponent<PartnerSkillManager>();
        hpStatusComponent = partnerSkillManager.hpSliderObject.GetComponent<StatusBar>();
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        knockback.GetKnockedBack(transform, knockBackThrust);
        StartCoroutine(flash.FlashRoutine());
        StartCoroutine(CheckDetectDeathRoutine());
    }

    private IEnumerator CheckDetectDeathRoutine()
    {
        yield return new WaitForSeconds(flash.GetRestoreMatTime());
        DetectDeath();
    }

    public void DetectDeath()
    {
        if (currentHealth <= 0)
        {
            Instantiate(deathVFXPrefab, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}
