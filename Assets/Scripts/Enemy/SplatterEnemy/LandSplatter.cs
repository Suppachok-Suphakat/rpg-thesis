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
        Charecter player = other.gameObject.GetComponent<Charecter>();
        player?.TakeDamage(damage, transform);
    }

    private void DisableCollider()
    {
        GetComponent<CapsuleCollider2D>().enabled = false;
    }
}
