using System.Collections;
using UnityEngine;

public class ShooterEnemyAI : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float roamChangeDirTime = 2f;

    [Header("Attack")]
    [SerializeField] private float attackRange = 3f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private bool stopMovingWhileAttacking = false;

    [Header("Shooting")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletMoveSpeed = 5f;
    [SerializeField] private int burstCount = 3;
    [SerializeField] private int projectilesPerBurst = 1;
    [SerializeField] private float timeBetweenBursts = 0.2f;
    [SerializeField] private float restTime = 1f;
    [SerializeField] private float animationDelay = 0.5f;
    [SerializeField] private Transform bulletSpawnpoint;

    private bool canAttack = true;
    private bool isShooting = false;
    private Vector2 roamPosition;
    private float roamTimer = 0f;
    private Rigidbody2D rb;
    private Vector2 moveDir;
    private bool isImmobilized = false;
    private Animator animator;

    private enum State { Roaming, Attacking }
    private State state;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        state = State.Roaming;
    }

    private void Start()
    {
        roamPosition = GetRoamingPosition();
    }

    private void Update()
    {
        if (isImmobilized) return;

        MovementStateControl();
        UpdateAnimationState();
    }

    private void FixedUpdate()
    {
        if (!isImmobilized)
        {
            rb.velocity = moveDir * moveSpeed;
        }
    }

    private void UpdateAnimationState()
    {
        animator.SetBool("isMoving", moveDir != Vector2.zero);
    }

    private void MovementStateControl()
    {
        switch (state)
        {
            case State.Roaming:
                Roaming();
                break;
            case State.Attacking:
                Attacking();
                break;
        }
    }

    private void Roaming()
    {
        roamTimer += Time.deltaTime;
        MoveTo(roamPosition);

        if (Vector2.Distance(transform.position, PlayerController.instance.transform.position) < attackRange)
        {
            state = State.Attacking;
        }

        if (roamTimer > roamChangeDirTime)
        {
            roamPosition = GetRoamingPosition();
            roamTimer = 0f;
        }
    }

    private void Attacking()
    {
        if (Vector2.Distance(transform.position, PlayerController.instance.transform.position) > attackRange)
        {
            state = State.Roaming;
            return;
        }

        if (canAttack)
        {
            canAttack = false;
            Attack();
            if (stopMovingWhileAttacking)
            {
                StopMoving();
            }
            else
            {
                MoveTo(roamPosition);
            }
            StartCoroutine(AttackCooldownRoutine());
        }
    }

    private void Attack()
    {
        if (!isShooting)
        {
            StartCoroutine(ShootRoutine());
        }
    }

    private IEnumerator AttackCooldownRoutine()
    {
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    private IEnumerator ShootRoutine()
    {
        isShooting = true;
        yield return new WaitForSeconds(animationDelay);

        for (int i = 0; i < burstCount; i++)
        {
            for (int j = 0; j < projectilesPerBurst; j++)
            {
                Instantiate(bulletPrefab, bulletSpawnpoint.position, Quaternion.identity)
                    .GetComponent<Projectile>()
                    .UpdateMoveSpeed(bulletMoveSpeed);
            }
            yield return new WaitForSeconds(timeBetweenBursts);
        }
        yield return new WaitForSeconds(restTime);
        isShooting = false;
    }

    private void MoveTo(Vector2 targetPosition)
    {
        moveDir = (targetPosition - (Vector2)transform.position).normalized;
    }

    public void StopMoving()
    {
        isImmobilized = true;
        moveDir = Vector2.zero;
        rb.velocity = Vector2.zero;
        StartCoroutine(StopMovingForSeconds());
    }

    private IEnumerator StopMovingForSeconds()
    {
        yield return new WaitForSeconds(attackCooldown);
        isImmobilized = false;
    }

    private Vector2 GetRoamingPosition()
    {
        return (Vector2)transform.position + Random.insideUnitCircle * 2f;
    }
}
