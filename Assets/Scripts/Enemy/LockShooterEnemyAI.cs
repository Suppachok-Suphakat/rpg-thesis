using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockShooterEnemyAI : MonoBehaviour
{
    public float speed;
    public float lineOfSight;
    public float shootingRange;
    public float fireRate = 1f;
    private float nextFireTime;
    private Transform player;
    private Animator animator;

    public GameObject bullet;
    public GameObject bulletPoint;

    void Start()
    {
        player = PlayerController.instance.transform;
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        float distanceFromPlayer = Vector2.Distance(player.position, transform.position);

        if (distanceFromPlayer < lineOfSight && distanceFromPlayer > shootingRange)
        {
            FlipSprite(player);
            animator.SetBool("isMoving", true);
            transform.position = Vector2.MoveTowards(transform.position, player.position, speed * Time.deltaTime);
        }
        else if (distanceFromPlayer <= shootingRange)
        {
            animator.SetBool("isMoving", false);
            if (Time.time > nextFireTime)
            {
                FlipSprite(player);
                animator.SetTrigger("Attack");
                nextFireTime = Time.time + fireRate;
            }
        }
    }
    public void Shoot()
    {
        // Get the base direction the enemy is facing
        Vector2 forwardDirection = (player.position - transform.position).normalized;

        // Get enemy facing direction (1 = right, -1 = left)
        float facingDirection = transform.localScale.x;

        // Corrected directional adjustments
        Vector2 upperDirection = new Vector2(forwardDirection.x, 1f).normalized;
        Vector2 lowerDirection = new Vector2(forwardDirection.x, -1f).normalized;

        // Determine which direction to fire based on the player's relative position
        Vector2 selectedDirection = forwardDirection; // Default to straight

        if (player.position.y > transform.position.y + 1) // Player is above
        {
            selectedDirection = upperDirection;
        }
        else if (player.position.y < transform.position.y - 1) // Player is below
        {
            selectedDirection = lowerDirection;
        }

        // Adjust shooting based on enemy facing direction
        FireBullet(selectedDirection * facingDirection);
    }

    private void FireBullet(Vector2 shootDirection)
    {
        GameObject spawnedBullet = Instantiate(bullet, bulletPoint.transform.position, Quaternion.identity);
        Rigidbody2D rb = spawnedBullet.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.velocity = shootDirection * 10f; // This is correct

            // Rotate the bullet to match its movement direction
            float angle = Mathf.Atan2(shootDirection.y, shootDirection.x) * Mathf.Rad2Deg;
            spawnedBullet.transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    private void FlipSprite(Transform target)
    {
        if (target.position.x < transform.position.x)
            transform.localScale = new Vector3(1, 1, 1);
        else
            transform.localScale = new Vector3(-1, 1, 1);
    }
}
