using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public static EnemyAI Instance;

    [SerializeField] private float roamChangeDirFloat = 2f;
    [SerializeField] private float attackRange = 0f;
    [SerializeField] private MonoBehaviour enemyType;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private bool stopMovingWhileAttacking = false;

    private bool canAttack = true;

    private enum State
    {
        Roaming,
        Attacking
    }

    private Vector2 roamPosition;
    private float timeRoaming = 0f;

    private State state;
    private EnemyPathfinding enemyPathfinding;

    private Animator animator;

    //public bool isWalking;

    private void Awake()
    {
        enemyPathfinding = GetComponent<EnemyPathfinding>();
        animator = GetComponent<Animator>();
        state = State.Roaming;
    }

    private void Start()
    {
        roamPosition = GetRoamingPosition();
    }

    private void Update()
    {
        MovementStateControl();
        UpdateAnimationState();
    }

    private void UpdateAnimationState()
    {
        if (enemyPathfinding.IsMoving())
        {
            animator.SetBool("isMoving", true);
        }
        else
        {
            animator.SetBool("isMoving", false);
        }
    }

    private void MovementStateControl()
    {
        switch (state)
        {
            default:
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
        timeRoaming += Time.deltaTime;

        enemyPathfinding.MoveTo(roamPosition);

        if (Vector2.Distance(transform.position, PlayerController.instance.transform.position) < attackRange)
        {
            state = State.Attacking;
        }

        if (timeRoaming > roamChangeDirFloat)
        {
            roamPosition = GetRoamingPosition();
        }
    }

    private void Attacking()
    {
        if (Vector2.Distance(transform.position, PlayerController.instance.transform.position) > attackRange)
        {
            state = State.Roaming;
            return;
        }

        LockShooter shooter = enemyType as LockShooter;
        OneAnimLockShooter oneAnimShooter = enemyType as OneAnimLockShooter;

        if (canAttack)
        {
            canAttack = false;

            if (stopMovingWhileAttacking)
            {
                enemyPathfinding.StopMoving();
            }

            if (shooter != null)
            {
                shooter.Attack();
            }

            if (oneAnimShooter != null)
            {
                oneAnimShooter.Attack();
            }

            animator.SetTrigger("Attack");

            StartCoroutine(AttackCooldownRoutine());
        }

        if ((shooter != null && !shooter.CanShootPlayer() && !shooter.isShooting && shooter.hasAttackFinished) ||
            (oneAnimShooter != null && !oneAnimShooter.CanShootPlayer()))
        {
            if (!stopMovingWhileAttacking) // Only reposition if movement is allowed
            {
                FindBetterShootingPosition();
            }
        }
    }

    private void FindBetterShootingPosition()
    {
        if (stopMovingWhileAttacking)
        {
            return; // Prevents movement while attacking
        }

        Vector2 playerPos = PlayerController.instance.transform.position;
        Vector2 enemyPos = transform.position;
        Vector2 directionToPlayer = (playerPos - enemyPos).normalized;

        float bestDistance = 5f;
        float angleStep = 15f;
        int maxAttempts = 12;

        for (int i = 0; i < maxAttempts; i++)
        {
            float angleOffset = (i % 2 == 0 ? 1 : -1) * (angleStep * (i / 2));
            Vector2 testDirection = Quaternion.Euler(0, 0, angleOffset) * directionToPlayer;
            Vector2 testPosition = (Vector2)playerPos - (testDirection * bestDistance);

            if (!stopMovingWhileAttacking) // Ensure movement is only applied when allowed
            {
                enemyPathfinding.MoveTo(testPosition);
            }
        }
    }

    private IEnumerator AttackCooldownRoutine()
    {
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    private Vector2 GetRoamingPosition()
    {
        timeRoaming = 0f;
        return new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
    }
}