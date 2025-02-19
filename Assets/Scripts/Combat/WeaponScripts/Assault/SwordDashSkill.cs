using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SwordDashSkill : MonoBehaviour, IWeapon
{
    [SerializeField] private GameObject slashAnimPrefab;
    [SerializeField] private Transform slashAnimSpawnPoint;
    [SerializeField] private WeaponInfo weaponInfo;
    [SerializeField] private int staminaCost;

    [SerializeField] private int damageAmount;

    [SerializeField] private Transform weaponCollider;
    private Animator animator;
    private Character character;

    private GameObject slashAnim;

    private PlayerController playerController;
    private PhantomHeroSkill phantomHeroSkill;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        character = FindObjectOfType<Character>();
        playerController = FindObjectOfType<PlayerController>();
        phantomHeroSkill = FindObjectOfType<PhantomHeroSkill>();
    }

    private void Start()
    {
        weaponCollider.gameObject.GetComponent<SwordDamageSouce>().damageAmount = this.damageAmount;
        slashAnimSpawnPoint = GameObject.Find("SlashSpawnPoint").transform;
    }

    private void Update()
    {
        MouseFollowWithOffset();
    }

    public WeaponInfo GetWeaponInfo()
    {
        return weaponInfo;
    }

    public void Attack()
    {
        if (character.stamina.currVal >= staminaCost && !PlayerController.instance.isMenuActive)
        {
            StartCoroutine(SwordDashAttackRoutine());
        }
    }

    private IEnumerator SwordDashAttackRoutine()
    {
        // Start Dash
        playerController.SkillDash();

        // Perform Attack
        animator.SetTrigger("Attack");
        weaponCollider.gameObject.SetActive(true);
        slashAnim = Instantiate(slashAnimPrefab, slashAnimSpawnPoint.position, Quaternion.identity);
        slashAnim.transform.parent = this.transform.parent;
        weaponCollider.gameObject.GetComponent<DamageSouce>();

        // Find the closest enemy
        Transform enemyTarget = FindClosestEnemy();
        if (enemyTarget != null)
        {
            phantomHeroSkill.OnPlayerAttack(enemyTarget); // Call OnPlayerAttack with the enemy
        }
        // Wait for dash duration before attacking
        yield return new WaitForSeconds(0.4f);
    }

    private IEnumerator ReduceStaminaRoutine()
    {
        character.ReduceStamina(staminaCost);
        yield return new WaitForSeconds(1f);
    }

    private IEnumerator SkillRoutine()
    {
        yield return new WaitForSeconds(5);
    }

    public void DoneAttackingAnimEvent()
    {
        weaponCollider.gameObject.SetActive(false);
    }

    public void SwingUpFlipAnimEvent()
    {
        slashAnim.gameObject.transform.rotation = Quaternion.Euler(-180, 0, 0);

        if (PlayerController.instance.FacingLeft)
        {
            slashAnim.GetComponent<SpriteRenderer>().flipX = true;
        }
    }

    public void SwingDownFlipAnimEvent()
    {
        slashAnim.gameObject.transform.rotation = Quaternion.Euler(0, 0, 0);

        if (PlayerController.instance.FacingLeft)
        {
            slashAnim.GetComponent<SpriteRenderer>().flipX = true;
        }
    }

    private void MouseFollowWithOffset()
    {
        Vector3 mousePos = Input.mousePosition;
        Vector3 playerScreenPoint = Camera.main.WorldToScreenPoint(PlayerController.instance.transform.position);

        float angle = Mathf.Atan2(mousePos.y, mousePos.x) * Mathf.Rad2Deg;

        if (mousePos.x < playerScreenPoint.x)
        {
            ActiveWeapon.Instance.transform.rotation = Quaternion.Euler(0, -180, angle);
        }
        else
        {
            ActiveWeapon.Instance.transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    private Transform FindClosestEnemy()
    {
        float closestDistance = Mathf.Infinity;
        Transform closestEnemy = null;
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy"); // Assuming enemies have the tag "Enemy"

        foreach (GameObject enemy in enemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemy = enemy.transform;
            }
        }

        return closestEnemy;
    }
}
