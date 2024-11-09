using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandSplatter : MonoBehaviour
{
    public int damage;
    private SpriteFade spriteFade;

    private void Awake()
    {
        spriteFade = GetComponent<SpriteFade>();
    }

    private void Start()
    {
        StartCoroutine(spriteFade.SlowFadeRoutine());

        Invoke("DisableCollider", 0.2f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Character player = other.gameObject.GetComponent<Character>();
        player?.TakeDamage(damage, transform);

        HeroHealth partnerHealth = other.gameObject.GetComponent<HeroHealth>();
        partnerHealth?.TakeDamage(damage, transform);
    }

    private void DisableCollider()
    {
        GetComponent<CapsuleCollider2D>().enabled = false;
    }
}
