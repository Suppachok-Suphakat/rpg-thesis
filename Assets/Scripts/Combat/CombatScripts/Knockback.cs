using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Knockback : MonoBehaviour
{
    public bool GettingKnockedBack { get; private set; }

    [SerializeField] private float knockBackTime = 0.2f;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void GetKnockedBack(Transform damageSource, float knockBackThrust)
    {
        if (GettingKnockedBack) return;

        GettingKnockedBack = true;

        // Calculate knockback force based on distance and thrust
        float distance = Vector2.Distance(transform.position, damageSource.position);
        Vector2 knockbackForce = (transform.position - damageSource.position).normalized * knockBackThrust * distance * rb.mass;

        // Apply knockback using the modular method
        ApplyKnockback(rb, knockbackForce);

        // Start smooth deceleration coroutine
        StartCoroutine(KnockRoutine());
    }

    public void ApplyKnockback(Rigidbody2D targetRb, Vector2 knockbackForce)
    {
        // Reset velocity and apply knockback
        targetRb.velocity = Vector2.zero;
        targetRb.AddForce(knockbackForce, ForceMode2D.Impulse);
    }

    private IEnumerator KnockRoutine()
    {
        float elapsedTime = 0f;
        Vector2 initialVelocity = rb.velocity;

        // Gradually reduce velocity to zero over the knockback duration
        while (elapsedTime < knockBackTime)
        {
            rb.velocity = Vector2.Lerp(initialVelocity, Vector2.zero, elapsedTime / knockBackTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Reset velocity and state after knockback
        rb.velocity = Vector2.zero;
        GettingKnockedBack = false;
    }
}
