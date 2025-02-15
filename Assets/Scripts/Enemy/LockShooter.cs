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

    [Header("Attack Settings")]
    [SerializeField] private float attackDelay = 1f; // Delay before attacking, adjustable in the Inspector

    private Animator animator;
    private bool isFirstShot = true;  // Flag to track the first shot
    private bool isSearchingForPlayer = false; // Flag to track if enemy is searching for a better spot

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
        StartCoroutine(AttackWithDelay()); // Start the coroutine to handle the attack delay
    }

    private IEnumerator AttackWithDelay()
    {
        // Wait for the specified delay
        yield return new WaitForSeconds(attackDelay);

        // Once the delay is over, proceed with the attack
        if (isFirstShot)
        {
            animator.SetTrigger("Attack"); // Trigger the first attack animation
        }
        else
        {
            animator.SetTrigger("AttackSecond"); // Trigger the second attack animation
        }
        isFirstShot = !isFirstShot;  // Toggle the shot flag for the next attack
    }

    // This method will be called from an Animation Event
    public void Shoot()
    {
        if (!TargetConeOfInfluence(out float startAngle, out float currentAngle, out float angleStep, out bool canShoot) || !canShoot)
        {
            // If we can't shoot, stop attacking and search for a better spot
            if (!isSearchingForPlayer)
            {
                StopAttackAndSearch();
            }
            return;
        }

        // Continue shooting if the player is in range
        FireShot(ref currentAngle, ref angleStep);
    }

    private void FireShot(ref float currentAngle, ref float angleStep)
    {
        for (int j = 0; j < projectilesPerBurst; j++)
        {
            // Before firing each shot, check if the player is still in range
            if (!TargetConeOfInfluence(out _, out _, out _, out bool canShootInRange) || !canShootInRange)
            {
                Debug.Log("Player is out of range for this shot. Bullet will not fire.");
                continue; // Skip this shot if the player is out of range
            }

            Quaternion rotation = Quaternion.Euler(0, 0, currentAngle);
            GameObject newBullet = Instantiate(bulletPrefab, bulletSpawnpoint.position, rotation);

            // Calculate direction to the player
            Vector2 direction = (PlayerController.instance.transform.position - bulletSpawnpoint.position).normalized;

            if (newBullet.TryGetComponent(out EnemyBullet projectile))
            {
                projectile.UpdateMoveSpeed(bulletMoveSpeed);
                projectile.SetDirection(direction); // Set the bullet direction
            }

            currentAngle += angleStep;
        }
    }

    private void StopAttackAndSearch()
    {
        // Stop attacking
        animator.SetTrigger("Idle"); // Transition to idle or searching state in your animator
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
}
