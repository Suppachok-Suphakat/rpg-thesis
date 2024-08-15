using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EXSkillFollowMouse : MonoBehaviour
{
    public int damage;

    [SerializeField] private float moveSpeed = 22f;
    [SerializeField] private GameObject particleOnHitPrefabVFX;
    [SerializeField] private bool isEnemyProjectile = false;
    [SerializeField] private float projectileRange = 10f;

    private Vector3 startPosition;

    [SerializeField] private GameObject sliderObject;
    [SerializeField] private SkillStatusBar statusComponent;
    public int chargeAmount;

    private Vector3 initialDirection;

    private void Start()
    {
        startPosition = transform.position;
        sliderObject = GameObject.Find("RangeWeaponSkillBar");
        statusComponent = sliderObject.GetComponent<SkillStatusBar>();
    }

    private void Update()
    {
        MoveProjectileFollowMouse();
        DetectFireDistance();
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
        Charecter player = other.gameObject.GetComponent<Charecter>();
        Barrier barrier = other.gameObject.GetComponent<Barrier>();

        if (!other.isTrigger && (enemyHealth || indestructible || player))
        {
            if ((player && isEnemyProjectile) || (enemyHealth && !isEnemyProjectile))
            {
                player?.TakeDamage(damage, transform);
                Instantiate(particleOnHitPrefabVFX, transform.position, transform.rotation);
            }
            else if (!other.isTrigger && indestructible)
            {
                Instantiate(particleOnHitPrefabVFX, transform.position, transform.rotation);
            }
        }
        else if (barrier)
        {
            Instantiate(particleOnHitPrefabVFX, transform.position, transform.rotation);
            Destroy(gameObject);
        }
    }

    private void DetectFireDistance()
    {
        if (Vector3.Distance(transform.position, startPosition) > projectileRange)
        {
            Destroy(gameObject);
        }
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
