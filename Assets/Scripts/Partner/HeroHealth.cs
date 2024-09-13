using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroHealth : MonoBehaviour
{
    private bool canTakeDamage = true;
    [SerializeField] private float damageRecoveryTime = 1f;
    private Knockback knockback;
    [SerializeField] private float knockBackThrustAmount = 10f;
    private Flash flash;

    PartnerSkillManager partnerSkillManager;
    public Stat hp;
    [SerializeField] StatusBar hpBar;
    public bool isDead { get; private set; }

    private void Awake()
    {
        flash = GetComponent<Flash>();
        knockback = GetComponent<Knockback>();
    }

    private void Start()
    {
        hp.currVal = hp.maxVal;

        partnerSkillManager = GameObject.Find("PartnerCanvas").GetComponent<PartnerSkillManager>();
    }

    public void TakeDamage(int amount, Transform hitTransform)
    {
        if (!canTakeDamage) { return; }
        knockback.GetKnockedBack(hitTransform, knockBackThrustAmount);
        StartCoroutine(flash.FlashRoutine());
        canTakeDamage = false;

        DeductHP(amount);

        if (hp.currVal <= 0)
        {
            CheckIfPartnerDeath();
        }

        UpdateHpBar();
        StartCoroutine(DamageRecoveryRoutine());
    }

    public void DeductHP(int amount)
    {
        //int damageAfterArmor = Mathf.Max(amount - PlayerStats.instance.armor, 0);
        hp.currVal -= amount;
        //currVal -= (amount - PlayerStats.instance.armor);
    }

    private void CheckIfPartnerDeath()
    {
        if (hp.currVal <= 0 && !isDead)
        {
            isDead = true;
            Destroy(ActiveWeapon.Instance.gameObject);
            hp.currVal = 0;
            //GetComponent<Animator>().SetTrigger(DEATH_HASH);
        }
    }

    private IEnumerator CheckDetectDeathRoutine()
    {
        yield return new WaitForSeconds(flash.GetRestoreMatTime());
        DetectDeath();
    }

    private IEnumerator DamageRecoveryRoutine()
    {
        yield return new WaitForSeconds(damageRecoveryTime);
        canTakeDamage = true;
    }

    public void UpdateHpBar()
    {
        hpBar.Set(hp.currVal, hp.maxVal);
    }

    public void DetectDeath()
    {
        if (hp.currVal <= 0)
        {
            //Instantiate(deathVFXPrefab, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}
