using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealMagic : MonoBehaviour
{
    [SerializeField] private int hpRecoveryRate = 10;
    [SerializeField] private float hpRecoveryDelay = 0.1f;

    private float hpRecoveryTimer;
    private float heroHpRecoveryTimer;

    private void OnTriggerStay2D(Collider2D other)
    {
        Charecter charecter = other.gameObject.GetComponent<Charecter>();
        HeroHealth heroHealth = other.gameObject.GetComponent<HeroHealth>();

        if (charecter != null && hpRecoveryTimer <= 0f && charecter.hp.currVal < charecter.hp.maxVal)
        {
            charecter.Heal(hpRecoveryRate);
            hpRecoveryTimer = hpRecoveryDelay; // Reset the timer
        }

        if (heroHealth != null && heroHpRecoveryTimer <= 0f && heroHealth.hp.currVal < heroHealth.hp.maxVal)
        {
            heroHealth.Heal(hpRecoveryRate);
            heroHpRecoveryTimer = hpRecoveryDelay; // Reset the timer
        }
    }

    private void Update()
    {
        // Update timers
        if (hpRecoveryTimer > 0f)
        {
            hpRecoveryTimer -= Time.deltaTime;
        }

        if (heroHpRecoveryTimer > 0f)
        {
            heroHpRecoveryTimer -= Time.deltaTime;
        }
    }
}