using System.Collections;
using UnityEngine;

public class HitStop : MonoBehaviour
{
    bool waiting;
    EnemyHealth enemyHealth;
    Rigidbody2D rb;
    Animator animator;

    private void Awake()
    {
        enemyHealth = GetComponent<EnemyHealth>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    public void Stop(float duration)
    {
        if (waiting)
            return;
        StartCoroutine(StopEnemyForDuration(duration));
    }

    IEnumerator StopEnemyForDuration(float duration)
    {
        waiting = true;

        // Stop enemy movement
        if (rb != null)
            rb.velocity = Vector2.zero;

        // Stop enemy animations
        if (animator != null)
            animator.speed = 0;

        yield return new WaitForSeconds(duration);

        // Resume enemy animations
        if (animator != null)
            animator.speed = 1;

        waiting = false;
    }
}
