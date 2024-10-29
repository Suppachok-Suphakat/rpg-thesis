using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class KnightHeroAI : MonoBehaviour
{
    public float attackDistance = 5;
    public float distanceToAttack = 1;
    public float distanceToDefence = 5;
    public int followSpeed = 2;
    public int chaseSpeed = 2;

    public enum State
    {
        follow = 0,
        chase = 1,
        attack = 2,
        skill = 3,
        defence = 4,
        death = 5,
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

    KnightHeroSkill skill;
    public bool isUsingSkill = false;

    [SerializeField] private EnemyHealth currentEnemyHealth;

    [SerializeField] private float slideDuration = 0.5f;  // Duration for sliding past obstacles
    private bool isSliding = false;

    private LineTrigger lineTrigger;

    [SerializeField] public GameObject skillBar;
    [SerializeField] public GameObject selectedIndicator;
    [SerializeField] public GameObject skillbutton1;
    [SerializeField] public GameObject skillbutton2;

    public KnightHeroSkill knightHeroSkill;

    private void Awake()
    {
        player = GameObject.FindWithTag("Player").transform;

        flash = GetComponent<Flash>();
        knockback = GetComponent<Knockback>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        attackCooldown = cooldownTime;
        skill = gameObject.GetComponent<KnightHeroSkill>();

        knightHeroSkill = GetComponent<KnightHeroSkill>();
        lineTrigger = GameObject.Find("Player").GetComponent<LineTrigger>();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (currentState == State.follow && Vector3.Distance(transform.position, player.position) <= 2f)
        {
            RepelHeroes();
        }

        if (currentState == State.skill)
        {
            SkillLogic();
            return;
        }

        HandleMouseInput();

        if (Input.GetKeyDown(KeyCode.R))
        {
            currentState = State.follow;
            focusEnemy = null;
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
            case State.defence:
                DefenceLogic();
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

    void FollowLogic()
    {
        FlipSprite(PlayerController.instance.transform);
        float distancePlayer = Vector3.Distance(transform.position, player.position);

        if (distancePlayer > 2f)
        {
            animator.SetBool("isDefencing", false);
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

    void ChaseLogic()
    {
        if (isUsingSkill) return;
        if (currentState == State.skill) return;

        if (focusEnemy != null)
        {
            float distanceToEnemy = Vector2.Distance(transform.position, focusEnemy.position);
            float distanceToDefence = Vector2.Distance(player.transform.position, focusEnemy.position);
            float distanceToPlayer = Vector2.Distance(player.transform.position, transform.position);

            if (distanceToEnemy <= attackDistance)
            {
                currentState = State.defence;
            }
            else
            {
                if(distanceToPlayer <= distanceToDefence)
                {
                    Vector3 directionToEnemy = (focusEnemy.position - transform.position).normalized;
                    animator.SetBool("isDefencing", false);
                    animator.SetBool("isWalking", true);
                    FlipSprite(focusEnemy);
                    transform.Translate(directionToEnemy * chaseSpeed * Time.deltaTime);
                }
                else 
                {
                    currentState = State.defence;
                }
            }
        }
        else
        {
            currentState = State.follow;
        }
    }

    public void DefenceLogic()
    {
        if (isUsingSkill) return;
        if (currentState == State.skill) return;

        rb.bodyType = RigidbodyType2D.Dynamic;
        if (focusEnemy != null)
        {
            AggroEnemy();
            float distanceToEnemy = Vector2.Distance(transform.position, focusEnemy.position);

            if (distanceToEnemy <= distanceToAttack)
            {
                AttackLogic();
            }
            else
            {
                animator.SetBool("isWalking", false);
                animator.SetBool("isDefencing", true);
            }

            if (distanceToEnemy >= distanceToAttack)
            {
                currentState = State.chase;
            }
        }
        else
        {
            currentState = State.follow;
        }
    }

    void AggroEnemy()
    {
        AttackerEnemyAI enemyAI = enemyTransform.GetComponent<AttackerEnemyAI>();
        if (enemyAI != null)
        {
            enemyAI.currentTarget = this.transform;
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
        if (focusEnemy != null && !isAttacking)
        {
            float distanceToEnemy = Vector2.Distance(transform.position, focusEnemy.position);

            if (distanceToEnemy <= distanceToAttack)
            {
                isAttacking = true;
                animator.SetTrigger("attack");
                StartCoroutine(AttackCooldown());
            }
            else
            {
                currentState = State.defence;
            }
        }
        else
        {
            //currentState = State.follow;
        }
    }

    private IEnumerator BraceForNextAttack()
    {
        yield return new WaitForSeconds(1f);
        isAttacking = false;

        currentState = State.defence;
    }

    IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(attackCooldown);
        isAttacking = false;
        currentState = State.defence;
    }

    // Cooldown after defending
    private IEnumerator DefenceCooldown()
    {
        yield return new WaitForSeconds(1.5f);
        currentState = State.chase;
    }

    public void SkillLogic()
    {
        isUsingSkill = true;
        currentState = State.skill;
        rb.bodyType = RigidbodyType2D.Kinematic;
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

    void FlipSprite(Transform flipTo)
    {
        if (flipTo.transform.position.x < transform.position.x)
            transform.localScale = new Vector3(-1, 1, 1);
        else
            transform.localScale = new Vector3(1, 1, 1);
    }
}