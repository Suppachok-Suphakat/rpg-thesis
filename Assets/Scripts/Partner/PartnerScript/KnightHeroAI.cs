using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class KnightHeroAI : MonoBehaviour
{
    [System.Serializable]
    public class Status
    {
        public float attackDistance = 5;
        public int followSpeed = 2;
        public int chaseSpeed = 2;
    }

    public Transform player;
    public Status status;
    public State currentState = State.follow;

    public enum State
    {
        follow = 0,
        chase = 1,
        attack = 2,
        skill = 3,
        defense = 4,
        death = 5
    }

    //RaycastHit2D hit;
    public LayerMask focus;

    //==Combat==//
    private Knockback knockback;
    private Flash flash;

    private Animator animator;
    private Rigidbody2D rb;

    public Transform damageCollider;
    public bool isAttacking = false;
    private float timeBetweenAttacks = 10f;

    private Transform enemyTransform;
    public Transform focusEnemy;

    [SerializeField] private float cooldownTime;
    private float attackCooldown;
    [SerializeField] private bool isCooldown;

    KnightHeroSkill skill;

    [SerializeField] private EnemyHealth currentEnemyHealth;

    [SerializeField] private float slideDuration = 0.5f;  // Duration for sliding past obstacles
    private bool isSliding = false;

    private LineTrigger lineTrigger;

    [SerializeField] public GameObject skillBar;

    private void Awake()
    {
        player = GameObject.FindWithTag("Player").transform;

        flash = GetComponent<Flash>();
        knockback = GetComponent<Knockback>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();

        attackCooldown = cooldownTime;
        skill = gameObject.GetComponent<KnightHeroSkill>();

        lineTrigger = GameObject.Find("Player").GetComponent<LineTrigger>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        HandleMouseInput();

        if (Input.GetKeyDown(KeyCode.R))
        {
            currentState = State.follow;
            enemyTransform = null;
            animator.SetBool("isWalking", false);
        }

        // Switch between states
        switch (currentState)
        {
            case State.follow:
                FollowLogic();
                break;
            case State.chase:
                ChaseLogic();
                break;
            case State.attack:
                AttackLogic();
                break;
            case State.skill:
                SkillLogic();
                break;
            case State.defense:
                AttackLogic();
                break;
            case State.death:
                /////////////////
                break;
        }
    }

    void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(1))
        {
            if (lineTrigger.currentTarget == this.transform)
            {
                Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mousePosition.z = 0f;

                float detectionRadius = 0.5f; // Adjust this radius as needed
                Collider2D hit = Physics2D.OverlapCircle(mousePosition, detectionRadius, focus);

                if (hit != null)
                {
                    Debug.Log("Collider detected: " + hit.name);
                    if (hit.CompareTag("Enemy"))
                    {
                        // Hide the arrow on the previous enemy if it exists
                        if (currentEnemyHealth != null)
                        {
                            Debug.Log("Hiding arrow on previous enemy: " + currentEnemyHealth.name);
                            currentEnemyHealth.HideArrow();
                        }

                        // Set the new enemy and show the arrow
                        Debug.Log("Enemy detected: " + hit.name);
                        focusEnemy = hit.transform;
                        enemyTransform = focusEnemy;
                        currentState = State.chase;

                        // Update the currentEnemyHealth reference
                        currentEnemyHealth = hit.GetComponent<EnemyHealth>();
                        if (currentEnemyHealth != null)
                        {
                            Debug.Log("Showing arrow on new enemy: " + currentEnemyHealth.name);  // This log should now appear
                            currentEnemyHealth.ShowArrow();
                        }
                    }
                    else
                    {
                        Debug.Log("Detected but not an enemy: " + hit.name);
                    }
                }
                else
                {
                    Debug.Log("No collider detected within radius.");
                }
            }
        }
    }

    void FollowLogic()
    {
        FlipSprite(PlayerController.instance.transform);
        float distancePlayer = Vector3.Distance(transform.position, player.position);

        if (distancePlayer > 2)
        {
            animator.SetBool("isWalking", true);
            Vector2 targetPosition = new Vector2(player.transform.position.x, player.transform.position.y);
            Vector2 directionToPlayer = (targetPosition - (Vector2)transform.position).normalized;

            if (!isSliding && !CanMove(directionToPlayer))
            {
                StartCoroutine(SlidePastObstacle(directionToPlayer));
            }
            else
            {
                transform.position = Vector2.Lerp(transform.position, targetPosition, status.followSpeed * Time.deltaTime);
            }
        }
        else
        {
            animator.SetBool("isWalking", false);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            focusEnemy = other.transform;
            enemyTransform = focusEnemy;
            currentState = State.chase;
        }
        else if (other.CompareTag("EnemyAttack"))
        {
            currentState = State.defense;
            animator.SetTrigger("Defend");  // Trigger the shield defense animation
            rb.velocity = Vector2.zero;     // Stop movement during defense

            // Optionally, add a brief pause after defending
            StartCoroutine(DefenseCooldown());
        }
    }

    private void ChaseLogic()
    {
        // If not attacking and the enemy is within range, chase the enemy
        if (enemyTransform != null && !isAttacking)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            float distanceToEnemy = Vector2.Distance(transform.position, enemyTransform.position);

            if (distanceToPlayer > 10f)
            {
                currentState = State.follow;
            }
            else if (distanceToEnemy <= status.attackDistance)
            {
                AttackLogic();
            }
            else
            {
                Vector3 directionToEnemy = (enemyTransform.position - transform.position).normalized;
                animator.SetBool("isWalking", true);

                FlipSprite(enemyTransform);
                transform.Translate(directionToEnemy * status.chaseSpeed * Time.deltaTime);
            }
        }
        else
        {
            currentState = State.follow;
        }
    }

    private void GuardLogic()
    {
        if (enemyTransform != null)
        {
            Vector3 directionToEnemy = (enemyTransform.position - transform.position).normalized;
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            float distanceToEnemy = Vector2.Distance(transform.position, enemyTransform.position);

            // If the enemy is close, block or attack
            if (distanceToEnemy <= status.attackDistance)
            {
                AttackLogic();
            }
            // Stay close to the player if no enemies nearby
            else if (distanceToPlayer > 2)
            {
                FollowLogic();
            }
        }
        else
        {
            FollowLogic();
        }
    }

    private bool CanMove(Vector2 direction)
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, 0.1f, LayerMask.GetMask("Obstacles"));
        return hit.collider == null;
    }

    private IEnumerator SlidePastObstacle(Vector2 direction)
    {
        isSliding = true;
        float slideTime = 0;

        while (slideTime < slideDuration)
        {
            transform.Translate(direction * status.followSpeed * Time.deltaTime);
            slideTime += Time.deltaTime;
            yield return null;
        }

        isSliding = false;
    }

    public void AttackLogic()
    {
        if (enemyTransform != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            if (distanceToPlayer > 10f)
            {
                currentState = State.follow;
                return;
            }

            if (!isAttacking)
            {
                isCooldown = true;
                if (isCooldown)
                {
                    cooldownTime -= Time.deltaTime;
                    currentState = State.attack;

                    if (cooldownTime <= 0)
                    {
                        AggroEnemy();
                        animator.SetBool("isWalking", false);
                        FlipSprite(enemyTransform);
                        animator.SetTrigger("Attack");
                        isAttacking = true;
                        isCooldown = false;
                        cooldownTime = attackCooldown;

                        // After attacking, stand still and brace for another attack
                        StartCoroutine(BraceForNextAttack());
                    }
                }
            }
        }
    }

    private IEnumerator BraceForNextAttack()
    {
        // Pause briefly to brace for another attack
        yield return new WaitForSeconds(1f);  // Adjust the time as needed
        isAttacking = false;

        // Transition to defense state or back to chase/follow
        currentState = State.defense;
    }

    IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(attackCooldown);
        isAttacking = false;

        // Check if the enemy is still alive or gone, then decide next state
        if (enemyTransform != null)
        {
            currentState = State.chase; // Continue chasing if enemy is alive
        }
        else
        {
            currentState = State.follow; // Return to follow if enemy is gone
        }
    }

    // Aggro the enemy to prioritize attacking the Knight
    void AggroEnemy()
    {
        AttackerEnemyAI enemyAI = enemyTransform.GetComponent<AttackerEnemyAI>();
        if (enemyAI != null)
        {
            enemyAI.currentTarget = this.transform; // Make the enemy target the Knight
        }
    }

    // Cooldown after defending
    private IEnumerator DefenseCooldown()
    {
        yield return new WaitForSeconds(1.5f);  // Adjust duration of defense
        currentState = State.chase;  // Return to chasing the enemy after defending
    }

    public void SkillLogic()
    {
            if (skill.barrierCircleInstance != null)
            {
                
            }
            else
            {
                currentState = State.follow;
            }
    }

    public void Attack()
    {
        // When attacking, the Knight should stop moving
        damageCollider.gameObject.SetActive(true);
        rb.velocity = Vector2.zero;  // Stop the Knight from moving
    }

    public void AttackEnd() // Call this when the attack animation ends
    {
        isAttacking = false;
        damageCollider.gameObject.SetActive(false);
    }

    void FlipSprite(Transform flipTo)
    {
        if (flipTo.transform.position.x < transform.position.x)
            transform.localScale = new Vector3(-1, 1, 1);
        else
            transform.localScale = new Vector3(1, 1, 1);
    }
}
