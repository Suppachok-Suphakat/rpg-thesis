using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcherHeroAI : MonoBehaviour
{
    [System.Serializable]
    public class Status
    {
        public float attackDistance = 5f;
        public float distanceToAttack = 1f;
        public float distanceToDefence = 5f;
        public int followSpeed = 2;
        public int chaseSpeed = 2;
        public float retreatDistance = 3f; // Distance to retreat if enemies get too close
        public float safeDistance = 6f; // Ideal shooting distance
        public int retreatSpeed = 3;
    }

    public enum State
    {
        follow = 0,
        chase = 1,
        attack = 2,
        skill = 3,
        retreat = 4,
        death = 5,
    }

    public Transform player;
    public Status status;
    public State currentState = State.follow;


    //RaycastHit2D hit;
    public LayerMask focus;

    //==Combat==//
    private Knockback knockback;
    private Flash flash;

    private Animator animator;
    private Rigidbody2D rb;

    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private Transform arrowSpawnPoint;
    public bool isAttacking = false;

    public Transform enemyTransform;
    public Transform focusEnemy;

    [SerializeField] private float cooldownTime;
    private float attackCooldown;
    [SerializeField] private bool isCooldown;

    ArcherHeroSkill skill;

    [SerializeField] private EnemyHealth currentEnemyHealth;

    [SerializeField] private float slideDuration = 0.5f;  // Duration for sliding past obstacles
    private bool isSliding = false;

    private LineTrigger lineTrigger;

    [SerializeField] public GameObject skillBar;
    [SerializeField] public GameObject weaponBar;

    public ArcherHeroSkill archerHeroSkill;

    private void Awake()
    {
        flash = GetComponent<Flash>();
        knockback = GetComponent<Knockback>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();

        attackCooldown = cooldownTime;
        skill = gameObject.GetComponent<ArcherHeroSkill>();

        archerHeroSkill = GetComponent<ArcherHeroSkill>();
        lineTrigger = GameObject.Find("Player").GetComponent<LineTrigger>();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        cooldownTime -= Time.deltaTime;

        HandleMouseInput();

        if (Input.GetKeyDown(KeyCode.R))
        {
            currentState = State.follow;
            focusEnemy = null;
            animator.SetBool("isWalking", false);
        }

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
            case State.retreat:
                RetreatLogic();
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
        if (Input.GetMouseButtonDown(3))
        {
            if (lineTrigger.currentTarget == this.transform)
            {
                Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mousePosition.z = 0f;

                float detectionRadius = 0.5f; // Adjust this radius as needed
                Collider2D hit = Physics2D.OverlapCircle(mousePosition, detectionRadius);

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
        if (currentState == State.skill) return;

        if (other.CompareTag("Enemy"))
        {
            focusEnemy = other.transform;
            enemyTransform = focusEnemy;
            currentState = State.chase;
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (currentState == State.skill) return;

        if (other.CompareTag("Enemy"))
        {
            focusEnemy = other.transform;
            enemyTransform = focusEnemy;
            currentState = State.chase;
        }
    }

    private void ChaseLogic()
    {
        //if (isUsingSkill) return;
        if (currentState == State.skill) return;

        if (focusEnemy != null)
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

    void AttackLogic()
    {
        if (focusEnemy != null && cooldownTime <= 0f)
        {
            float distanceToEnemy = Vector2.Distance(transform.position, focusEnemy.position);

            if (distanceToEnemy > status.retreatDistance)
            {
                animator.SetBool("isWalking", false);
                animator.SetTrigger("Attack");
                cooldownTime = attackCooldown;
            }
            else
            {
                currentState = State.retreat;
            }
        }
        else
        {
            currentState = State.follow;
        }
    }

    void RetreatLogic()
    {
        if (focusEnemy != null)
        {
            float distanceToEnemy = Vector2.Distance(transform.position, focusEnemy.position);

            // Retreat if the enemy is too close
            if (distanceToEnemy < status.retreatDistance)
            {
                Vector3 directionAwayFromEnemy = (transform.position - focusEnemy.position).normalized;
                transform.Translate(directionAwayFromEnemy * status.retreatSpeed * Time.deltaTime);
                animator.SetBool("isWalking", true);
            }
            else if (distanceToEnemy > status.safeDistance)
            {
                // Switch to attacking when safe distance is reached
                currentState = State.attack;
            }
        }
    }

    public void SkillLogic()
    {
        //if (skill.barrierCircleInstance != null)
        //{

        //}
        //else
        //{
        //    currentState = State.follow;
        //}
    }

    public void Attack()
    {
        // Calculate direction towards the enemy
        Vector2 directionToEnemy = (enemyTransform.position - transform.position).normalized;

        // Calculate rotation angle towards the enemy
        float angle = Mathf.Atan2(directionToEnemy.y, directionToEnemy.x) * Mathf.Rad2Deg;
        arrowSpawnPoint.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        GameObject newArrow = Instantiate(arrowPrefab, arrowSpawnPoint.position, arrowSpawnPoint.rotation);
        newArrow.GetComponent<Arrow>().UpdateProjectileRange(12f);
    }

    private void Unattacked()
    {
        //damageCollider.gameObject.SetActive(false);
    }

    void FlipSprite(Transform flipTo)
    {
        if (flipTo.transform.position.x < transform.position.x)
            transform.localScale = new Vector3(-1, 1, 1);
        else
            transform.localScale = new Vector3(1, 1, 1);
    }
}