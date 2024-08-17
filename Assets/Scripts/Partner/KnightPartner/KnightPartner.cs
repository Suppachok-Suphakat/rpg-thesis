using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class KnightPartner : Partner
{
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
    private Transform focusEnemy;

    [SerializeField] private float cooldownTime;
    private float attackCooldown;
    [SerializeField] private bool isCooldown;

    KnightPartnerSkill skill;

    private EnemyHealth currentEnemyHealth;

    [SerializeField] private float slideDuration = 0.5f;  // Duration for sliding past obstacles
    private bool isSliding = false;

    private void Awake()
    {
        flash = GetComponent<Flash>();
        knockback = GetComponent<Knockback>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();

        attackCooldown = cooldownTime;
        skill = gameObject.GetComponent<KnightPartnerSkill>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        HandleMouseInput();

        // Check for R key press to return to follow state
        if (Input.GetKeyDown(KeyCode.R))
        {
            currentState = State.follow;
            enemyTransform = null; // Clear the current enemy target
            animator.SetBool("isWalking", false); // Stop walking animation
        }

        switch (currentState)
        {
            case State.follow:
                if (skill.barrierCircleInstance != null)
                {
                    currentState = State.skill;
                }
                FollowLogic();
                break;
            case State.chase:
                if (skill.barrierCircleInstance != null)
                {
                    currentState = State.skill;
                }
                ChaseLogic();
                break;
            case State.attack:
                if (skill.barrierCircleInstance != null)
                {
                    currentState = State.skill;
                }
                AttackLogic();
                break;
            case State.skill:
                SkillLogic();
                break;
            case State.death:
                break;
        }
    }

    void HandleMouseInput()
    {
        if (Input.GetKeyDown(KeyCode.E))
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

    void FollowLogic()
    {
        FlipSprite();
        float distancePlayer = Vector3.Distance(transform.position, player.position);

        if (distancePlayer > 2)
        {
            animator.SetBool("isWalking", true);
            Vector2 targetPosition = new Vector2(player.transform.position.x, player.transform.position.y);
            Vector2 directionToPlayer = (targetPosition - (Vector2)transform.position).normalized;

            // Check if the partner is stuck or moving
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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            currentState = State.chase;
            enemyTransform = other.transform;
        }
    }

    private void ChaseLogic()
    {
        if (enemyTransform != null)
        {
            Vector3 directionToEnemy = (enemyTransform.position - transform.position).normalized;

            animator.SetBool("isWalking", true);
            float distanceToEnemy = Vector2.Distance(transform.position, enemyTransform.position);

            if (distanceToEnemy > status.attackDistance)
            {
                if (enemyTransform.position.x < transform.position.x)
                    transform.localScale = new Vector3(-1, 1, 1);
                else
                    transform.localScale = new Vector3(1, 1, 1);

                // Check if the partner is stuck or moving
                if (!isSliding && !CanMove(directionToEnemy))
                {
                    StartCoroutine(SlidePastObstacle(directionToEnemy));
                }
                else
                {
                    transform.Translate(directionToEnemy * status.chaseSpeed * Time.deltaTime);
                }
            }
            else
            {
                AttackLogic();
            }
        }
        else
        {
            currentState = State.follow;
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

    public override void AttackLogic()
    {
        if (enemyTransform != null)
        {

            isCooldown = true;
            if (isCooldown)
            {
                cooldownTime -= Time.deltaTime;
                currentState = State.chase;

                if (cooldownTime <= 0)
                {
                    animator.SetBool("isWalking", false);
                    animator.SetTrigger("Attack");
                    isCooldown = false;
                    cooldownTime = attackCooldown;
                }
            }
        }
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
        damageCollider.gameObject.SetActive(true);
    }

    private void Unattacked()
    {
        //damageCollider.gameObject.SetActive(false);
    }

    void FlipSprite()
    {
        if (PlayerController.instance.transform.position.x < transform.position.x)
            transform.localScale = new Vector3(-1, 1, 1);
        else
            transform.localScale = new Vector3(1, 1, 1);
    }
}
