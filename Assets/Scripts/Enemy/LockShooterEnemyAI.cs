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

    private Vector2 lastKnownPosition;

    // Roaming Variables
    public float roamRadius = 3f;
    public float roamDelay = 2f;
    private Vector2 roamTarget;
    private bool isRoaming = false;

    void Start()
    {
        player = PlayerController.instance.transform;
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        knockback = GetComponent<Knockback>();
        rb = GetComponent<Rigidbody2D>();

        StartCoroutine(Roam());
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

        if (distanceFromPlayer < lineOfSight && !isImmobilized)
        {
            lastKnownPosition = player.position;
            FlipSprite(player.position.x - transform.position.x);
            animator.SetBool("isMoving", true);
            transform.position = Vector2.MoveTowards(transform.position, player.position, speed * Time.deltaTime);
        }
        else if (!isRoaming)
        {
            animator.SetBool("isMoving", false);
        }

        if (horizontalDistance < 6.0f && verticalDistance <= 2.0f && CanShootAtPlayer())
        {
            if (Time.time > nextFireTime)
            {
                //FlipSprite(player.position.x - transform.position.x);
                animator.SetTrigger("Attack");

                if (stopMovingWhileAttacking)
                {
                    StopMoving();
                }

                nextFireTime = Time.time + fireRate;
            }
        }
    }

    private IEnumerator Roam()
    {
        while (true)
        {
            yield return new WaitForSeconds(roamDelay);

            if (!isImmobilized && player != null && Vector2.Distance(player.position, transform.position) >= lineOfSight)
            {
                isRoaming = true;
                roamTarget = (Vector2)transform.position + new Vector2(Random.Range(-roamRadius, roamRadius), Random.Range(-roamRadius, roamRadius));
                animator.SetBool("isMoving", true);

                while (Vector2.Distance(transform.position, roamTarget) > 0.1f)
                {
                    transform.position = Vector2.MoveTowards(transform.position, roamTarget, speed * Time.deltaTime);
                    FlipSprite(roamTarget.x - transform.position.x);
                    yield return null;
                }

                animator.SetBool("isMoving", false);
                isRoaming = false;
            }
        }
    }

    public void Shoot()
    {
        Vector2 targetPosition = lastKnownPosition;
        FlipSprite(lastKnownPosition.x - transform.position.x);

        if (Mathf.Abs(player.position.y - transform.position.y) <= 2.0f && Vector2.Distance(player.position, transform.position) < lineOfSight)
        {
            lastKnownPosition = player.position;
        }

        Vector2 shootDirection = (targetPosition - (Vector2)transform.position).normalized;
        FireBullet(shootDirection);
    }

    private bool CanShootAtPlayer()
    {
        Vector2 directionToPlayer = (player.position - transform.position).normalized;
        float angle = Vector2.Angle(transform.right, directionToPlayer);

        return angle < 60f || angle > 120f;
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

    private void FlipSprite(float direction)
    {
        if (direction < 0)
            transform.localScale = new Vector3(1, 1, 1);
        else if (direction > 0)
            transform.localScale = new Vector3(-1, 1, 1);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, lineOfSight);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, roamRadius);

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

    private IEnumerator FlipDelay()
    {
        yield return new WaitForSeconds(1f);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("EnemyWall")) // Ensure walls have this tag
        {
            if (isRoaming)
            {
                // Reverse direction when roaming
                Vector2 bounceDirection = (Vector2)transform.position - roamTarget;
                roamTarget = (Vector2)transform.position + bounceDirection.normalized * roamRadius;

                // Move enemy back slightly
                transform.position += (Vector3)bounceDirection.normalized * 0.5f;

                // Flip sprite accordingly
                FlipSprite(bounceDirection.x);
            }
        }
    }
}