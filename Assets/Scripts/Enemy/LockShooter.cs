using System.Collections;
using UnityEngine;

public class LockShooter : MonoBehaviour, IEnemy
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletMoveSpeed;
    [SerializeField] private int burstCount;
    [SerializeField] private int projectilesPerBurst;
    [SerializeField] private Transform bulletSpawnpoint;
    [SerializeField] private float coneAngle = 45f;
    [SerializeField] public bool canShoot;
    [SerializeField] public bool hasAttackFinished = false;
    [SerializeField] public bool isShooting = false;

    private bool isAttacking = false;
    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    public bool CanShootPlayer()
    {
        return TargetConeOfInfluence(out _, out _, out _, out bool canShoot) && canShoot;
    }

    public void Attack()
    {
        if (!isShooting)
        {
            isShooting = true; // Mark as shooting
            hasAttackFinished = false;
            FlipSprite(PlayerController.instance.transform);
            animator.SetTrigger("Attack"); // Ensure the attack animation is triggered
        }
    }

    public void OnAttackAnimationEnd()
    {
        isShooting = false; // Mark shooting as finished
        hasAttackFinished = true;
    }

    public bool IsAttacking()
    {
        return isAttacking;
    }

    // This method will be called from an Animation Event
    public void Shoot()
    {
        FlipSprite(PlayerController.instance.transform);
        float startAngle, currentAngle, angleStep;
        if (!TargetConeOfInfluence(out startAngle, out currentAngle, out angleStep, out bool canShoot))
        {
            // If player is out of range, shoot at their last known position
            Vector2 lastKnownDirection = (PlayerController.instance.transform.position - transform.position).normalized;
            FireBullet(lastKnownDirection);
            return;
        }

        StartCoroutine(ShootBurst(startAngle, angleStep));
    }

    private IEnumerator ShootBurst(float startAngle, float angleStep)
    {
        for (int i = 0; i < burstCount; i++)
        {
            float angle = startAngle;
            for (int j = 0; j < projectilesPerBurst; j++)
            {
                Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
                FireBullet(direction);
                angle += angleStep;
            }

            yield return new WaitForSeconds(0.2f); // Small delay between bursts
        }
    }

    private void FireBullet(Vector2 direction)
    {
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnpoint.position, Quaternion.identity);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = direction * bulletMoveSpeed;
        }
    }


    private bool TargetConeOfInfluence(out float startAngle, out float currentAngle, out float angleStep, out bool canShoot)
    {
        Vector2 targetDirection = PlayerController.instance.transform.position - transform.position;
        float targetAngle = Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg;

        bool isFacingLeft = transform.localScale.x > 0;
        float forwardAngle = isFacingLeft ? 180f : 0f;
        float halfAngleLimit = coneAngle / 2f;

        float minAngle = forwardAngle - halfAngleLimit;
        float maxAngle = forwardAngle + halfAngleLimit;

        // Determine whether the player is within the shooting cone
        canShoot = targetAngle >= minAngle && targetAngle <= maxAngle;

        if (!canShoot)
        {
            startAngle = currentAngle = angleStep = 0;
            return false; // Exit early if not within range
        }

        targetAngle = Mathf.Clamp(targetAngle, minAngle, maxAngle);

        float halfAngleSpread = coneAngle / 2f;
        startAngle = targetAngle - halfAngleSpread;
        float endAngle = targetAngle + halfAngleSpread;
        angleStep = projectilesPerBurst > 1 ? (endAngle - startAngle) / (projectilesPerBurst - 1) : 0;
        currentAngle = startAngle;

        return true;
    }

    private void FlipSprite(Transform flipTo)
    {
        if (flipTo.position.x < transform.position.x)
            transform.localScale = new Vector3(1, 1, 1);
        else
            transform.localScale = new Vector3(-1, 1, 1);
    }
}