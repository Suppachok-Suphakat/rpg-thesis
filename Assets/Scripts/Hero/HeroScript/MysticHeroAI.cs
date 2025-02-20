using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MysticHeroAI : MonoBehaviour
{
    public float attackDistance = 5;
    public float distanceToAttack = 1;
    public float followSpeed = 2;
    public int chaseSpeed = 2;

    private float followOverrideTimer = 0f;
    private float followOverrideDuration = 2f;

    public enum State
    {
        follow = 0,
        chase = 1,
        attack = 2,
        death = 3,
    }

    public Transform player;
    public State currentState = State.follow;

    public float minDistance = 1.0f; // Minimum distance between heroes
    public float repulsionStrength = 2.0f; // Force to push heroes apart

    //==Enemy Focus==//
    public LayerMask focus;
    public Transform enemyTransform;
    public Transform focusEnemy;

    //==Combat==//
    private Knockback knockback;
    private Flash flash;

    private Animator animator;
    private Rigidbody2D rb;

    public Transform damageCollider;
    public bool isAttacking = false;


    [SerializeField] private float cooldownTime;
    private float attackCooldown;
    [SerializeField] private bool isCooldown;

    MysticHeroSkill skill;
    public bool isUsingSkill = false;

    [SerializeField] private EnemyHealth currentEnemyHealth;

    [SerializeField] private float slideDuration = 0.5f;  // Duration for sliding past obstacles
    private bool isSliding = false;

    private LineTrigger lineTrigger;

    [SerializeField] public GameObject skillBar;
    [SerializeField] public GameObject selectedIndicator;
    [SerializeField] public GameObject skillbutton1;
    [SerializeField] public GameObject skillbutton2;

    public Sprite heroFaceSprite;

    private bool isFlippingOverridden = false;
    private Vector3 overrideFlipTarget;

    public MysticHeroSkill mysticHeroSkill;

    private void Awake()
    {
        player = FindObjectOfType<PlayerController>().transform;

        flash = GetComponent<Flash>();
        knockback = GetComponent<Knockback>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        attackCooldown = cooldownTime;
        skill = gameObject.GetComponent<MysticHeroSkill>();

        mysticHeroSkill = GetComponent<MysticHeroSkill>();
        lineTrigger = GameObject.Find("Player").GetComponent<LineTrigger>();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Vector3.Distance(transform.position, player.position) > 30f) // Adjust distance as needed
        {
            transform.position = player.position; // Instantly teleport to player
            currentState = State.follow;
        }

        if (currentState == State.follow) //&& Vector3.Distance(transform.position, player.position) <= 3f)
        {
            RepelHeroes();
        }

        HandleMouseInput();

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

        // Switch between states
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
            case State.death:
                //Death logic
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

                float detectionRadius = 0.5f;
                Collider2D hit = Physics2D.OverlapCircle(mousePosition, detectionRadius);

                if (hit != null)
                {
                    Debug.Log("Collider detected: " + hit.name);
                    if (hit.CompareTag("Enemy"))
                    {
                        if (currentEnemyHealth != null)
                        {
                            Debug.Log("Hiding arrow on previous enemy: " + currentEnemyHealth.name);
                            currentEnemyHealth.HideArrow();
                        }

                        Debug.Log("Enemy detected: " + hit.name);
                        focusEnemy = hit.transform;
                        enemyTransform = focusEnemy;
                        currentState = State.chase;

                        currentEnemyHealth = hit.GetComponent<EnemyHealth>();
                        if (currentEnemyHealth != null)
                        {
                            Debug.Log("Showing arrow on new enemy: " + currentEnemyHealth.name);
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

    void OnTriggerEnter2D(Collider2D other)
    {
        if (followOverrideTimer > 0) return;

        if (focusEnemy == null && other.CompareTag("Enemy"))
        {
            focusEnemy = other.transform;
            enemyTransform = focusEnemy;
            currentState = State.chase;
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (followOverrideTimer > 0) return;

        if (focusEnemy == null && other.CompareTag("Enemy"))
        {
            focusEnemy = other.transform;
            enemyTransform = focusEnemy;
            currentState = State.chase;
        }
    }

    public void FollowLogic()
    {

        if (!isFlippingOverridden)
        {
            FlipSprite(PlayerController.instance.transform);
        }

        float distancePlayer = Vector3.Distance(transform.position, player.position);

        if (distancePlayer > 2f)
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

    public void ChaseLogic()
    {
        if (focusEnemy != null)
        {
            float distanceToEnemy = Vector2.Distance(transform.position, focusEnemy.position);

            if (distanceToEnemy <= attackDistance)
            {
                currentState = State.attack;
                return;
            }

            Vector3 directionToEnemy = (focusEnemy.position - transform.position).normalized;
            animator.SetBool("isWalking", true);
            if (!isFlippingOverridden)
            {
                FlipSprite(focusEnemy);
            }
            transform.Translate(directionToEnemy * chaseSpeed * Time.deltaTime);
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

    public void AttackLogic()
    {
        if (focusEnemy != null)
        {
            float distanceToEnemy = Vector2.Distance(transform.position, focusEnemy.position);

            if (distanceToEnemy <= distanceToAttack && !isAttacking)
            {
                isAttacking = true;
                if (!isFlippingOverridden)
                {
                    FlipSprite(focusEnemy);
                }
                animator.SetTrigger("attack");
                StartCoroutine(AttackCooldown());
            }
            else if (distanceToEnemy > attackDistance)
            {
                currentState = State.chase;
            }
        }
        else
        {
            currentState = State.follow;
        }
    }

    private IEnumerator BraceForNextAttack()
    {
        yield return new WaitForSeconds(1f);
        isAttacking = false;
    }

    IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(attackCooldown);
        isAttacking = false;
    }

    public void Attack()
    {
        damageCollider.gameObject.SetActive(true);
        rb.velocity = Vector2.zero;
    }

    public void AttackEnd()
    {
        isAttacking = false;
        damageCollider.gameObject.SetActive(false);
    }

    public void FlipSprite(Transform flipTo)
    {
        if (isFlippingOverridden)
        {
            FlipSpriteOverride(overrideFlipTarget);
            return;
        }

        if (flipTo.transform.position.x < transform.position.x)
            transform.localScale = new Vector3(-1, 1, 1);
        else
            transform.localScale = new Vector3(1, 1, 1);
    }

    public void FlipSpriteOverride(Vector3 targetPosition)
    {
        if (targetPosition.x < transform.position.x)
            transform.localScale = new Vector3(-1, 1, 1); // Face left
        else
            transform.localScale = new Vector3(1, 1, 1); // Face right
    }

    public void SetFlipOverride(bool state, Vector3 target)
    {
        isFlippingOverridden = state;
        overrideFlipTarget = target;
    }
}
