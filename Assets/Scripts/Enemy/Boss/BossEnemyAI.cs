using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossEnemyAI : MonoBehaviour
{
    public static AttackerEnemyAI Instance;

    [SerializeField] private float roamChangeDirFloat = 2f;
    [SerializeField] private float attackRange = 0f;
    [SerializeField] private float rangeAttackRange = 0f;
    [SerializeField] private float damageRange = 0f;
    [SerializeField] private MonoBehaviour enemyType;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float rangeAttackCooldown = 5f;
    [SerializeField] private bool stopMovingWhileAttacking = false;
    [SerializeField] private bool stopMovingWhileRangeAttacking = false;

    [Header("Phase2")]
    [SerializeField] public float phase2MoveSpeed = 2f;

    [SerializeField] private float phase2AttackRange = 0f;
    [SerializeField] private float phase2RangeAttackRange = 0f;
    [SerializeField] private float phase2DamageRange = 0f;
    [SerializeField] private float phase2AttackCooldown = 2f;
    [SerializeField] private float phase2RangeAttackCooldown = 5f;
    [SerializeField] private int phase2BurstCount;
    [SerializeField] private int phase2ProjectilesPerBurst;
    [SerializeField] private int phase2TimeBetweenBursts;

    //GameObject partner;

    private bool canAttack = true;
    private bool canRangeAttack = true;

    private enum State
    {
        Roaming,
        Attacking,
        AttackingPhase2
    }

    private Vector2 roamPosition;
    private float timeRoaming = 0f;

    private State state;
    private BossEnemyPathfinding bossEnemyPathfinding;
    private EnemyHealth enemyHealth;
    private BossEnemy bossEnemy;

    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        bossEnemyPathfinding = GetComponent<BossEnemyPathfinding>();
        enemyHealth = GetComponent<EnemyHealth>();
        bossEnemy = GetComponent<BossEnemy>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        state = State.Attacking;
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
            case State.AttackingPhase2:
                AttackingPhase2();
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

        bossEnemyPathfinding.MoveTo(roamPosition);

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
        //Phase2
        if (enemyHealth.currentHealth <= enemyHealth.maxHealth /2)
        {
            state = State.AttackingPhase2;
        }

        Vector3 directionToPlayer = (PlayerController.instance.transform.position - transform.position).normalized;
        float distanceToPlayer = Vector2.Distance(transform.position, PlayerController.instance.transform.position);

        bossEnemyPathfinding.MoveTo(directionToPlayer);

        if (distanceToPlayer <= damageRange && canAttack)
        {
            canAttack = false;
            (enemyType as BEnemy).Attack();

            if (stopMovingWhileAttacking)
            {
                bossEnemyPathfinding.StopMoving();
            }

            StartCoroutine(AttackCooldownRoutine());
        }
        else if(distanceToPlayer >= damageRange && canRangeAttack)
        {
            canRangeAttack = false;
            (enemyType as BEnemy).RangeAttack();

            if (stopMovingWhileRangeAttacking)
            {
                bossEnemyPathfinding.StopMoving();
            }

            StartCoroutine(RangeAttackCooldownRoutine());
        }
    }

    private void AttackingPhase2()
    {
        bossEnemyPathfinding.moveSpeed = phase2MoveSpeed;

        bossEnemy.burstCount = phase2BurstCount;
        bossEnemy.projectilesPerBurst = phase2ProjectilesPerBurst;
        bossEnemy.timeBetweenBursts = phase2TimeBetweenBursts;
        Vector3 directionToPlayer = (PlayerController.instance.transform.position - transform.position).normalized;
        float distanceToPlayer = Vector2.Distance(transform.position, PlayerController.instance.transform.position);

        bossEnemyPathfinding.MoveTo(directionToPlayer);

        if (distanceToPlayer <= damageRange && canAttack)
        {
            canAttack = false;

            int attackChoice = Random.Range(1, 3);
            Debug.Log(attackChoice);
            if (attackChoice == 1)
            {
                (enemyType as BEnemy).Attack();
            }
            else
            {
                (enemyType as BEnemy).Attack2();
            }

            if (stopMovingWhileAttacking)
            {
                bossEnemyPathfinding.StopMoving();
            }

            StartCoroutine(AttackCooldownRoutine());
        }
        else if (distanceToPlayer >= damageRange && canRangeAttack)
        {
            canRangeAttack = false;
            (enemyType as BEnemy).RangeAttack();

            if (stopMovingWhileRangeAttacking)
            {
                bossEnemyPathfinding.StopMoving();
            }

            StartCoroutine(RangeAttackCooldownRoutine());
        }
    }

    private IEnumerator AttackCooldownRoutine()
    {
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    private IEnumerator RangeAttackCooldownRoutine()
    {
        yield return new WaitForSeconds(rangeAttackCooldown);
        canRangeAttack = true;
    }

    private Vector2 GetRoamingPosition()
    {
        timeRoaming = 0f;
        return new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
    }

    void FlipSprite()
    {
        if (PlayerController.instance.transform.position.x < transform.position.x)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        else
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }
}
