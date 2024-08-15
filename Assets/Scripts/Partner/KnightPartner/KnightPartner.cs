using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class KnightPartner : Partner
{
    //RaycastHit2D hit;
    public LayerMask Focus;

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
    void FollowLogic()
    {
        FlipSprite();
        float distancePlayer = Vector3.Distance(transform.position, player.position);

        if (distancePlayer > 2)
        {
            animator.SetBool("isWalking", true);
            Vector2 targetPosition = new Vector2(player.transform.position.x, player.transform.position.y); ;
            transform.position = Vector2.Lerp(transform.position, targetPosition, status.followSpeed * Time.deltaTime);
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

                transform.Translate(directionToEnemy * status.chaseSpeed * Time.deltaTime);
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
