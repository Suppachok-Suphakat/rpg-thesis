using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResoluteHeroAI : MonoBehaviour
{
    [Header("Status")]
    public float attackDistance = 5f;
    public float distanceToAttack = 1f;
    public float distanceToDefence = 5f;
    public float followSpeed = 2f;
    public int chaseSpeed = 2;
    public int retreatSpeed = 3;
    public float retreatDistance = 3f;
    public float retreatDuration = 1f;
    private float retreatTimer = 0f;
    public float safeDistance = 6f;
    public float maxChaseDistance = 10f;
    [SerializeField] private float stateSwitchBuffer = 0.5f;

    private float followOverrideTimer = 0f;
    private float followOverrideDuration = 2f;

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
    //public Status status;
    public State currentState = State.follow;

    public float minDistance = 1.0f; // Minimum distance between heroes
    public float repulsionStrength = 2.0f; // Force to push heroes apart

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

    ResoluteHeroSkill skill;

    [SerializeField] private EnemyHealth currentEnemyHealth;

    [SerializeField] private float slideDuration = 0.5f;  // Duration for sliding past obstacles
    private bool isSliding = false;

    private LineTrigger lineTrigger;

    [SerializeField] public GameObject skillBar;
    [SerializeField] public GameObject selectedIndicator;
    [SerializeField] public GameObject skillbutton1;
    [SerializeField] public GameObject skillbutton2;

    public Sprite heroFaceSprite;

    public ResoluteHeroSkill archerHeroSkill;

    private void Awake()
    {
        flash = GetComponent<Flash>();
        knockback = GetComponent<Knockback>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();

        attackCooldown = cooldownTime;
        skill = gameObject.GetComponent<ResoluteHeroSkill>();

        archerHeroSkill = GetComponent<ResoluteHeroSkill>();
        lineTrigger = GameObject.Find("Player").GetComponent<LineTrigger>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (currentState == State.follow) //&& Vector3.Distance(transform.position, player.position) <= 3f)
        {
            RepelHeroes();
        }

        if (cooldownTime > 0f)
        {
            cooldownTime -= Time.deltaTime;
        }

        HandleMouseInput();

        // Check if the archer should retreat based on distance to the enemy
        if (ShouldRetreat())
        {
            currentState = State.retreat;
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            currentState = State.follow;
            focusEnemy = null;
            animator.SetBool("isWalking", false);
            followOverrideTimer = followOverrideDuration;
        }

        if (followOverrideTimer > 0)
        {
            followOverrideTimer -= Time.deltaTime;
        }

        switch (currentState)
        {
            case State.follow:
                FollowLogic();
                break;
            case State.chase:
                if (followOverrideTimer <= 0)
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

        if (distancePlayer > 2.5f)
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
                transform.position = Vector2.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
            }
        }
        else
        {
            animator.SetBool("isWalking", false);
        }
    }

    protected void RepelHeroes()
    {
        // Find all heroes of type BaseHeroAI (includes Knight, Archer, Priestess, etc.)
        BaseHero[] allHeroes = FindObjectsOfType<BaseHero>();

        foreach (var hero in allHeroes)
        {
            if (hero != this) // Skip self
            {
                float distance = Vector3.Distance(transform.position, hero.transform.position);

                if (distance < minDistance)
                {
                    Vector3 direction = (transform.position - hero.transform.position).normalized;
                    Vector3 repulsion = direction * repulsionStrength * (minDistance - distance);

                    transform.position += repulsion * Time.deltaTime;
                }
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (currentState == State.skill || followOverrideTimer > 0) return;

        if (other.CompareTag("Enemy"))
        {
            focusEnemy = other.transform;
            enemyTransform = focusEnemy;
            currentState = State.chase;
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (currentState == State.skill || followOverrideTimer > 0) return;

        if (other.CompareTag("Enemy"))
        {
            focusEnemy = other.transform;
            enemyTransform = focusEnemy;
            currentState = State.chase;
        }
    }

    private void ChaseLogic()
    {
        if (enemyTransform != null)
        {
            float distanceToEnemy = Vector2.Distance(transform.position, enemyTransform.position);
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);

            // Return to follow if too far from the player
            if (distanceToPlayer > maxChaseDistance)
            {
                currentState = State.follow;
                focusEnemy = null;
                return;
            }

            if (distanceToEnemy <= attackDistance)
            {
                currentState = State.attack;
                return;
            }

            // Continue chasing the enemy
            Vector3 directionToEnemy = (enemyTransform.position - transform.position).normalized;
            animator.SetBool("isWalking", true);
            FlipSprite(enemyTransform);
            if (!isSliding && !CanMove(directionToEnemy))
            {
                StartCoroutine(SlidePastObstacle(directionToEnemy));
            }
            else
            {
                transform.Translate(directionToEnemy * chaseSpeed * Time.deltaTime);
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
            transform.Translate(direction * followSpeed * Time.deltaTime);
            slideTime += Time.deltaTime;
            yield return null;
        }

        isSliding = false;
    }

    void AttackLogic()
    {
        if (enemyTransform == null)
        {
            currentState = State.follow;
            return;
        }

        float distanceToEnemy = Vector2.Distance(transform.position, enemyTransform.position);

        if (distanceToEnemy <= attackDistance && cooldownTime <= 0f)
        {
            animator.SetBool("isWalking", false);
            FlipSprite(enemyTransform);
            animator.SetTrigger("attack");
            cooldownTime = attackCooldown;  // Reset cooldown
        }
        else if (distanceToEnemy > attackDistance)
        {
            currentState = State.chase; // Return to chasing if out of range
        }
        else if (distanceToEnemy <= retreatDistance)
        {
            currentState = State.retreat;  // Retreat if too close
        }
    }

    bool ShouldRetreat()
    {
        if (enemyTransform != null)
        {
            float distanceToEnemy = Vector2.Distance(transform.position, enemyTransform.position);
            return distanceToEnemy < retreatDistance;  // Retreat if the enemy is very close
        }
        return false;
    }

    void RetreatLogic()
    {
        if (enemyTransform != null)
        {
            float distanceToEnemy = Vector2.Distance(transform.position, enemyTransform.position);

            // Start or continue retreating
            if (distanceToEnemy < retreatDistance)
            {
                // Reset retreat timer if too close to enemy
                retreatTimer = retreatDuration;
            }

            // Countdown retreat timer
            if (retreatTimer > 0)
            {
                retreatTimer -= Time.deltaTime;

                // Move away from the enemy
                Vector3 directionAwayFromEnemy = (transform.position - enemyTransform.position).normalized;
                transform.Translate(directionAwayFromEnemy * retreatSpeed * Time.deltaTime);
                animator.SetBool("isWalking", true);
            }
            else if (distanceToEnemy > safeDistance + stateSwitchBuffer)
            {
                // Only switch back to attack if we are beyond the safe distance plus buffer
                currentState = State.attack;
            }
            else
            {
                // Prevent rapid switching; stay in retreat until the timer ends
                retreatTimer = retreatDuration;
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
        Debug.Log("Attack method triggered");
        // Calculate direction towards the enemy
        Vector2 directionToEnemy = (enemyTransform.position - transform.position).normalized;

        // Calculate rotation angle towards the enemy
        float angle = Mathf.Atan2(directionToEnemy.y, directionToEnemy.x) * Mathf.Rad2Deg;
        arrowSpawnPoint.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        Debug.Log("Shoot");
        GameObject newArrow = Instantiate(arrowPrefab, arrowSpawnPoint.position, arrowSpawnPoint.rotation);
        newArrow.GetComponent<Arrow>().UpdateProjectileRange(12f);
    }

    private void Unattacked()
    {
        //damageCollider.gameObject.SetActive(false);
    }

    private void SetWalkingAnimation(bool isWalking)
    {
        if (animator.GetBool("isWalking") != isWalking)
        {
            animator.SetBool("isWalking", isWalking);
        }
    }

    void FlipSprite(Transform flipTo)
    {
        if (flipTo.transform.position.x < transform.position.x)
            transform.localScale = new Vector3(-1, 1, 1);
        else
            transform.localScale = new Vector3(1, 1, 1);
    }
}