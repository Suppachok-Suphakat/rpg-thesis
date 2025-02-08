using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class EXSkillFollowMouse : MonoBehaviour, IWeapon
{
    public int damage;

    [SerializeField] private float moveSpeed = 22f;
    [SerializeField] private GameObject particleOnHitPrefabVFX;
    [SerializeField] private bool isEnemyProjectile = false;
    [SerializeField] private float projectileRange = 10f;
    [SerializeField] private WeaponInfo weaponInfo;
    private Animator animator;
    [SerializeField] private GameObject damageCollider;

    private Vector3 startPosition;

    private Vector3 initialDirection;

    private void Start()
    {
        startPosition = transform.position;
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        MoveProjectileFollowMouse();
    }


    public WeaponInfo GetWeaponInfo()
    {
        return weaponInfo;
    }

    public void Attack()
    {
        
    }

    public void UpdateProjectileRange(float projectileRange)
    {
        this.projectileRange = projectileRange;
    }

    public void UpdateMoveSpeed(float moveSpeed)
    {
        this.moveSpeed = moveSpeed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        EnemyHealth enemyHealth = other.gameObject.GetComponent<EnemyHealth>();
        Indestructible indestructible = other.gameObject.GetComponent<Indestructible>();

        if (!other.isTrigger && (enemyHealth || indestructible))
        {
            if (enemyHealth)
            {
                enemyHealth.TakeDamage(damage);
                Instantiate(particleOnHitPrefabVFX, transform.position, transform.rotation);
            }
            else if (!other.isTrigger && indestructible)
            {
                Instantiate(particleOnHitPrefabVFX, transform.position, transform.rotation);
            }
        }
    }

    private void MoveProjectileFollowMouse()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0; // Keep it 2D

        // Determine direction
        Vector3 direction = (mousePosition - transform.position).normalized;

        // Flip the sprite only on the X-axis
        if (direction.x > 0)
        {
            transform.localScale = new Vector3(1, 1, 1); // Facing right
        }
        else if (direction.x < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1); // Facing left
        }

        // Move toward the mouse
        transform.position = Vector3.MoveTowards(transform.position, mousePosition, moveSpeed * Time.deltaTime);
    }

    public void WeaponSkillActivate()
    {
        animator.SetTrigger("skill01");
    }

    public void DamageColliderOn()
    {
        damageCollider.SetActive(true);
    }

    public void DamageColliderOff()
    {
        damageCollider.SetActive(false);
    }
}
