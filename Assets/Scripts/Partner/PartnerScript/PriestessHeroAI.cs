using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PriestessHeroAI : MonoBehaviour
{
    [Header("Status")]
    public float attackDistance = 5f;
    public float distanceToAttack = 1f;
    public int followSpeed = 2;

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


    //RaycastHit2D hit;
    public LayerMask focus;

    //==Combat==//
    private Knockback knockback;
    private Flash flash;

    private Animator animator;
    private Rigidbody2D rb;

    [SerializeField] private GameObject magicPrefab; // The magic effect to instantiate
    public bool isAttacking = false;

    public Transform enemyTransform;
    public Transform focusEnemy;

    [SerializeField] private float cooldownTime;
    private float attackCooldown;
    [SerializeField] private bool isCooldown;

    PriestessHeroSkill skill;

    [SerializeField] private EnemyHealth currentEnemyHealth;

    [SerializeField] private float slideDuration = 0.5f;
    private bool isSliding = false;

    private LineTrigger lineTrigger;

    [SerializeField] public GameObject skillBar;
    [SerializeField] public GameObject weaponBar;

    public PriestessHeroSkill priestHeroSkill;

    private void Awake()
    {
        flash = GetComponent<Flash>();
        knockback = GetComponent<Knockback>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();

        attackCooldown = cooldownTime;
        skill = gameObject.GetComponent<PriestessHeroSkill>();

        priestHeroSkill = GetComponent<PriestessHeroSkill>();
        lineTrigger = GameObject.Find("Player").GetComponent<LineTrigger>();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
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
        }

        switch (currentState)
        {
            case State.follow:
                FollowLogic();
                break;
            case State.chase:
                //
                break;
            case State.attack:
                AttackLogic();
                break;
            case State.retreat:
                //
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

    void OnTriggerEnter2D(Collider2D other)
    {
        if (currentState == State.skill) return;

        if (other.CompareTag("Enemy"))
        {
            focusEnemy = other.transform;
            enemyTransform = focusEnemy;

            currentState = State.follow;
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (currentState == State.skill) return;

        if (other.CompareTag("Enemy"))
        {
            focusEnemy = other.transform;
            enemyTransform = focusEnemy;

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
        if (enemyTransform != null)
        {
            // Instantiate magic effect at enemy location
            Vector3 magicPosition = enemyTransform.position;
            Instantiate(magicPrefab, magicPosition, Quaternion.identity);
        }
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
