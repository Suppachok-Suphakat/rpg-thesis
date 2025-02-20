using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class EXSwordFollowMouse : MonoBehaviour, IWeapon
{
    public int damage;
    [SerializeField] private float moveSpeed = 22f;
    [SerializeField] private GameObject particleOnHitPrefabVFX;
    [SerializeField] private float projectileRange = 10f;
    [SerializeField] private WeaponInfo weaponInfo;
    private Animator animator;

    private Vector3 startPosition;
    private Vector3 initialDirection;

    private Dictionary<GameObject, float> damageCooldowns = new Dictionary<GameObject, float>();
    [SerializeField] private float damageCooldown = 1.0f; // Delay before hitting the same enemy again

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
                if (CanDamage(other.gameObject))
                {
                    enemyHealth.TakeDamage(damage);
                    Instantiate(particleOnHitPrefabVFX, transform.position, transform.rotation);
                    damageCooldowns[other.gameObject] = Time.time + damageCooldown;
                }
            }
            else if (!other.isTrigger && indestructible)
            {
                Instantiate(particleOnHitPrefabVFX, transform.position, transform.rotation);
            }
        }
    }

    private bool CanDamage(GameObject enemy)
    {
        if (!damageCooldowns.ContainsKey(enemy))
        {
            return true; // If enemy was never hit before, allow damage
        }

        if (Time.time >= damageCooldowns[enemy])
        {
            return true; // If cooldown has passed, allow damage
        }

        return false; // Otherwise, prevent continuous damage
    }

    private void MoveProjectileFollowMouse()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0; // Ensure the same z-coordinate as your 2D scene

        // Calculate direction from the projectile to the mouse position
        Vector3 direction = (mousePosition - transform.position).normalized;

        // Rotate the projectile to face the direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        // Move the projectile forward in the direction it's facing
        transform.Translate(Vector3.right * moveSpeed * Time.deltaTime);
    }
}
