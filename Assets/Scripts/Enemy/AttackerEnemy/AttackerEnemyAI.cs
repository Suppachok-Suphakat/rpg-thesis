using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackerEnemyAI : MonoBehaviour
{
    public static AttackerEnemyAI Instance;

    [SerializeField] private float roamChangeDirFloat = 2f;
    [SerializeField] private float attackRange = 0f;
    [SerializeField] private float damageRange = 0f;
    [SerializeField] private float chaseSpeed = 0f;
    [SerializeField] private MonoBehaviour enemyType;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private bool stopMovingWhileAttacking = false;

    //GameObject partner;

    private bool canAttack = true;

    private enum State
    {
        Roaming,
        Attacking
    }

    private Vector2 roamPosition;
    private float timeRoaming = 0f;

    private State state;
    private AttackerEnemyPathfinding attackerEnemyPathfinding;

    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        attackerEnemyPathfinding = GetComponent<AttackerEnemyPathfinding>();
        spriteRenderer = GetComponent<SpriteRenderer>();
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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Partner"))
        {
            
        }
    }

    private void Roaming()
    {
        timeRoaming += Time.deltaTime;

        attackerEnemyPathfinding.MoveTo(roamPosition);

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
        }

        Vector3 directionToPlayer = (PlayerController.instance.transform.position - transform.position).normalized;
        float distanceToPlayer = Vector2.Distance(transform.position, PlayerController.instance.transform.position);

        attackerEnemyPathfinding.MoveTo(directionToPlayer);
        //transform.Translate(directionToPlayer * chaseSpeed * Time.deltaTime);

        //FlipSprite();

        if (distanceToPlayer <= damageRange && canAttack)
        {
            canAttack = false;
            (enemyType as IEnemy).Attack();

            if (stopMovingWhileAttacking)
            {
                animator.SetTrigger("Attack");
                attackerEnemyPathfinding.StopMoving();
            }
            else
            {
                attackerEnemyPathfinding.MoveTo(roamPosition);
            }

            StartCoroutine(AttackCooldownRoutine());
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

    void FlipSprite()
    {
        // Flip the sprite based on the player's facing direction
        if (PlayerController.instance.transform.position.x < transform.position.x)
        {
            transform.localScale = new Vector3(1, 1, 1); // Reset the scale to face right
        }
        else
        {
            transform.localScale = new Vector3(-1, 1, 1); // Flip the sprite to face left
        }
    }
}
