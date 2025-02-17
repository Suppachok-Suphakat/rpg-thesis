using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockShooterEnemyAI : MonoBehaviour
{
    public float speed;
    public float lineOfSight;
    public float fireRate = 1f;
    private float nextFireTime;
    private Transform player;
    private Animator animator;

    public GameObject bullet;
    public GameObject bulletPoint;

    private SpriteRenderer spriteRenderer;
    private Knockback knockback;
    private Rigidbody2D rb;
    private Vector2 moveDir;
    public float stopMovingTime;
    private bool isImmobilized = false;
    [SerializeField] private bool stopMovingWhileAttacking = true;

    private Vector2 lastKnownPosition; // Store last detected position

    void Start()
    {
        player = PlayerController.instance.transform;
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        knockback = GetComponent<Knockback>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (isImmobilized)
        {
            animator.SetBool("isMoving", false);
            return;
        }

        float distanceFromPlayer = Vector2.Distance(player.position, transform.position);
        float horizontalDistance = Mathf.Abs(player.position.x - transform.position.x);
        float verticalDistance = Mathf.Abs(player.position.y - transform.position.y);

        if (distanceFromPlayer < lineOfSight && verticalDistance <= 2.0f)
        {
            FlipSprite(player);
            animator.SetBool("isMoving", true);
            transform.position = Vector2.MoveTowards(transform.position, new Vector2(player.position.x, transform.position.y), speed * Time.deltaTime);
            lastKnownPosition = player.position;
        }
        else
        {
            animator.SetBool("isMoving", false);
        }

        // **Prevent shooting if the player is directly above or below**
        if (horizontalDistance < 6.0f && verticalDistance <= 2.0f && CanShootAtPlayer())
        {
            if (Time.time > nextFireTime)
            {
                FlipSprite(player);
                animator.SetTrigger("Attack");

                if (stopMovingWhileAttacking)
                {
                    StopMoving();
                }

                nextFireTime = Time.time + fireRate;
            }
        }
    }

    private bool CanShootAtPlayer()
    {
        Vector2 directionToPlayer = (player.position - transform.position).normalized;
        float angle = Vector2.Angle(transform.right, directionToPlayer);

        // Limit shooting to 60 degrees in front (adjust as needed)
        return angle < 60f || angle > 120f;
    }

    public void Shoot()
    {
        Vector2 targetPosition = lastKnownPosition;

        // If the player is still in range, update target position
        if (Mathf.Abs(player.position.y - transform.position.y) <= 2.0f)
        {
            lastKnownPosition = player.position;
        }

        Vector2 shootDirection = (targetPosition - (Vector2)transform.position).normalized;
        FireBullet(shootDirection);
    }

    private void FireBullet(Vector2 shootDirection)
    {
        GameObject spawnedBullet = Instantiate(bullet, bulletPoint.transform.position, Quaternion.identity);
        Rigidbody2D rb = spawnedBullet.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.velocity = shootDirection * 10f;

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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, lineOfSight);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position + new Vector3(-6, -2, 0), transform.position + new Vector3(-6, 2, 0));
        Gizmos.DrawLine(transform.position + new Vector3(6, -2, 0), transform.position + new Vector3(6, 2, 0));
    }

    private void StopMoving()
    {
        if (isImmobilized) return;

        isImmobilized = true;
        rb.velocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
        animator.SetBool("isMoving", false);

        StartCoroutine(ResumeMovementAfterDelay());
    }

    private IEnumerator ResumeMovementAfterDelay()
    {
        yield return new WaitForSeconds(stopMovingTime);
        rb.bodyType = RigidbodyType2D.Dynamic;
        isImmobilized = false;
    }
}
