using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PriestessHeroAI : MonoBehaviour
{
    [Header("Status")]
    public float attackDistance = 5f;
    public float distanceToAttack = 1f;
    public int followSpeed = 2;
    public float healDistance = 5f;

    public enum State
    {
        follow = 0,
        heal = 1,
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

    [SerializeField] private GameObject magicPrefab; // The magic effect to instantiate
    public bool isAttacking = false;

    [SerializeField] private GameObject healEffectPrefab; // Healing effect to instantiate
    [SerializeField] private float healCooldown = 5f;
    private float currentHealCooldown;

    private Character playerHealth; // Reference to player's health
    private List<HeroHealth> heroesInRange = new List<HeroHealth>(); // List of nearby heroes
    private bool isHealing = false;

    public Transform enemyTransform;
    public Transform focusEnemy;

    [SerializeField] private float cooldownTime = 2f;
    [SerializeField] private float currentCooldown;
    [SerializeField] private float attackCooldown = 5f;
    [SerializeField] private bool isCooldown;

    PriestessHeroSkill skill;

    [SerializeField] private EnemyHealth currentEnemyHealth;

    [SerializeField] private float slideDuration = 0.5f;
    private bool isSliding = false;

    private LineTrigger lineTrigger;

    [SerializeField] public GameObject skillBar;
    [SerializeField] public GameObject selectedIndicator;
    [SerializeField] public GameObject skillbutton1;
    [SerializeField] public GameObject skillbutton2;

    public PriestessHeroSkill priestessHeroSkill;

    private void Awake()
    {
        flash = GetComponent<Flash>();
        knockback = GetComponent<Knockback>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();

        playerHealth = player.GetComponent<Character>();

        attackCooldown = cooldownTime;
        skill = gameObject.GetComponent<PriestessHeroSkill>();

        priestessHeroSkill = GetComponent<PriestessHeroSkill>();
        lineTrigger = GameObject.Find("Player").GetComponent<LineTrigger>();
    }

    // Start is called before the first frame update
    void Start()
    {
        currentHealCooldown = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (currentState == State.follow && Vector3.Distance(transform.position, player.position) <= 2f)
        {
            RepelHeroes();
        }

        // Reduce the cooldown over time
        if (cooldownTime > 0)
        {
            cooldownTime -= Time.deltaTime;
        }

        HandleMouseInput();

        if (Input.GetKeyDown(KeyCode.R))
        {
            currentState = State.follow;
            focusEnemy = null;
            animator.SetBool("isWalking", false);

            // Stop attack or skill logic
            isCooldown = false;
            isAttacking = false;
        }

        // Reduce the cooldown over time
        if (currentHealCooldown > 0)
        {
            currentHealCooldown -= Time.deltaTime;
        }

        FollowAndHeal(); // Follow player and perform healing logic

        //FollowAndAttack(); // Follow the player and attack enemies within range
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
                        currentState = State.follow;

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

    void FollowAndHeal()
    {
        FollowLogic();

        // Heal logic
        if (currentHealCooldown <= 0 && !isHealing)
        {
            HealTarget();
        }
    }

    void HealTarget()
    {
        // Determine the player's current health as a potential target for healing
        float playerHealthValue = playerHealth.hp.currVal;
        Transform targetToHeal = null;

        // Check if player is below max health and set as target if necessary
        if (playerHealthValue < playerHealth.hp.maxVal)
        {
            targetToHeal = playerHealth.transform;
        }

        // Find the hero with the lowest health in range
        HeroHealth lowestHealthHero = null;
        float lowestHeroHealth = float.MaxValue;

        foreach (HeroHealth hero in heroesInRange)
        {
            if (hero.hp.currVal < hero.hp.maxVal && hero.hp.currVal < lowestHeroHealth)
            {
                lowestHealthHero = hero;
                lowestHeroHealth = hero.hp.currVal;
            }
        }

        // Determine final target to heal: player or lowest-health hero
        if (lowestHealthHero != null && (playerHealthValue >= lowestHeroHealth || targetToHeal == null))
        {
            targetToHeal = lowestHealthHero.transform;
        }

        // Heal the target if one is found
        if (targetToHeal != null)
        {
            StartCoroutine(HealCoroutine(targetToHeal));
        }
        else
        {
            Debug.Log("No target needs healing.");
        }
    }

    private IEnumerator HealCoroutine(Transform target)
    {
        if (isHealing) yield break; // Prevent overlapping heals

        isHealing = true;
        animator.SetTrigger("attack"); // Trigger healing animation

        // Instantiate healing effect at the target's position
        Vector3 healEffectPosition = target.position;
        Instantiate(healEffectPrefab, healEffectPosition, Quaternion.identity);

        // Apply healing effect to the target
        Character character = target.GetComponent<Character>();
        if (character != null)
        {
            character.Heal(10); // Adjust healing amount as needed
            Debug.Log("Healed player: " + character.name);
        }
        else
        {
            HeroHealth heroHealth = target.GetComponent<HeroHealth>();
            if (heroHealth != null)
            {
                heroHealth.Heal(10); // Adjust healing amount as needed
                Debug.Log("Healed hero: " + heroHealth.name);
            }
        }

        currentHealCooldown = healCooldown; // Reset cooldown
        yield return new WaitForSeconds(healCooldown);
        isHealing = false;
    }

    void FollowAndAttack()
    {
        FollowLogic(); // Continue to follow the player

        // Attack enemies while following
        if (focusEnemy != null && enemyTransform != null)
        {
            float distanceToEnemy = Vector2.Distance(transform.position, enemyTransform.position);

            // Check if the enemy is within attack distance
            if (distanceToEnemy <= attackDistance && !isCooldown)
            {
                animator.SetBool("isWalking", false);
                FlipSprite(enemyTransform);
                animator.SetTrigger("attack"); // Trigger attack animation
                Attack(); // Perform the actual attack
                isCooldown = true; // Start cooldown
                currentCooldown = attackCooldown;  // Reset currentCooldown here
            }

            // Handle cooldown countdown
            if (isCooldown)
            {
                currentCooldown -= Time.deltaTime;  // Use a new cooldown variable
                if (currentCooldown <= 0)
                {
                    isCooldown = false; // Reset cooldown
                }
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            focusEnemy = other.transform;
            enemyTransform = focusEnemy;
        }

        if (other.CompareTag("Hero"))
        {
            HeroHealth heroHealth = other.GetComponent<HeroHealth>();
            if (heroHealth != null && !heroesInRange.Contains(heroHealth))
            {
                heroesInRange.Add(heroHealth);
            }
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            focusEnemy = other.transform;
            enemyTransform = focusEnemy;
        }

        if (other.CompareTag("Hero"))
        {
            HeroHealth heroHealth = other.GetComponent<HeroHealth>();
            if (heroHealth != null && !heroesInRange.Contains(heroHealth))
            {
                heroesInRange.Add(heroHealth);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Hero"))
        {
            HeroHealth heroHealth = other.GetComponent<HeroHealth>();
            if (heroHealth != null && heroesInRange.Contains(heroHealth))
            {
                heroesInRange.Remove(heroHealth);
            }
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
        if (enemyTransform != null && cooldownTime <= 0f)
        {
            float distanceToEnemy = Vector2.Distance(transform.position, enemyTransform.position);

            // Check if the enemy is within attack distance
            if (distanceToEnemy <= attackDistance)
            {
                animator.SetBool("isWalking", false);
                FlipSprite(enemyTransform);
                // Trigger the casting animation instead of the attack animation
                animator.SetTrigger("attack");
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
        if (enemyTransform != null && !isAttacking) // Ensure the Priestess is not already attacking
        {
            isAttacking = true; // Set the attacking flag

            // Instantiate magic effect at enemy location
            Vector3 magicPosition = enemyTransform.position;
            Instantiate(magicPrefab, magicPosition, Quaternion.identity);

            StartCoroutine(AttackCooldown());
        }
    }

    private IEnumerator AttackCooldown()
    {
        // Prevent multiple attacks during cooldown
        yield return new WaitForSeconds(attackCooldown);

        isAttacking = false; // Allow the Priestess to attack again
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

