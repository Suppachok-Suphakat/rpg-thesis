using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretShooter : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletMoveSpeed = 5f;
    [SerializeField] private float animationDelay = 0.5f;
    [SerializeField] private float fireRate = 1.5f;
    [SerializeField] private Transform[] bulletSpawnpoints;
    [SerializeField] private Animator animator;
    [SerializeField] private float exitTime = 10f; // Adjust based on exit animation length
    [SerializeField] private float exitAnimationDuration = 1.0f; // Adjust based on exit animation length

    private bool isShooting = false;
    private Transform targetEnemy;
    private bool isExiting = false;

    private Vector2[] shootDirections = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
    private string[] animationTriggers = { "ShootUp", "ShootDown", "ShootLeft", "ShootRight" };

    private void Start()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (animator == null)
        {
            Debug.LogError("Animator component is missing on " + gameObject.name);
        }

        if (bulletSpawnpoints.Length != 4)
        {
            Debug.LogError("TurretShooter requires exactly 4 bullet spawn points!");
        }

        StartCoroutine(SelfDestructRoutine(exitTime)); // Start countdown for destruction
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy") && targetEnemy == null)
        {
            targetEnemy = other.transform;
            StartShooting();
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Enemy") && targetEnemy == null)
        {
            targetEnemy = other.transform;
            StartShooting();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Enemy") && other.transform == targetEnemy)
        {
            targetEnemy = null;
            isShooting = false;
        }
    }

    private void StartShooting()
    {
        if (!isShooting)
        {
            isShooting = true;
            StartCoroutine(ShootRoutine());
        }
    }

    private IEnumerator ShootRoutine()
    {
        while (targetEnemy != null)
        {
            yield return new WaitForSeconds(animationDelay);

            if (targetEnemy != null)
            {
                Vector2 direction = GetShootDirection(targetEnemy.position);
                int spawnIndex = GetDirectionIndex(direction);
                Shoot(direction, spawnIndex);
            }

            yield return new WaitForSeconds(fireRate);
        }

        isShooting = false;
    }

    private void Shoot(Vector2 direction, int spawnIndex)
    {
        if (spawnIndex < 0 || spawnIndex >= bulletSpawnpoints.Length) return;

        Transform spawnPoint = bulletSpawnpoints[spawnIndex];

        GameObject newBullet = Instantiate(bulletPrefab, spawnPoint.position, Quaternion.identity);
        Rigidbody2D rb = newBullet.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.velocity = direction * bulletMoveSpeed;
        }

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        newBullet.transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private Vector2 GetShootDirection(Vector3 enemyPosition)
    {
        Vector2 direction = (enemyPosition - transform.position).normalized;

        float maxDot = -1f;
        int bestIndex = 0;

        for (int i = 0; i < shootDirections.Length; i++)
        {
            float dot = Vector2.Dot(direction, shootDirections[i]);
            if (dot > maxDot)
            {
                maxDot = dot;
                bestIndex = i;
            }
        }

        TriggerAnimation(bestIndex);
        return shootDirections[bestIndex];
    }

    private int GetDirectionIndex(Vector2 direction)
    {
        float maxDot = -1f;
        int bestIndex = 0;

        for (int i = 0; i < shootDirections.Length; i++)
        {
            float dot = Vector2.Dot(direction, shootDirections[i]);
            if (dot > maxDot)
            {
                maxDot = dot;
                bestIndex = i;
            }
        }

        return bestIndex;
    }

    private void TriggerAnimation(int index)
    {
        if (animator != null)
        {
            animator.SetTrigger(animationTriggers[index]);
        }
        else
        {
            Debug.LogError("Animator is NULL in TurretShooter!");
        }
    }

    private IEnumerator SelfDestructRoutine(float timeBeforeExit)
    {
        yield return new WaitForSeconds(timeBeforeExit);

        if (!isExiting)
        {
            isExiting = true;
            animator.SetTrigger("Exit"); // Play exit animation
            yield return new WaitForSeconds(exitAnimationDuration);
            Destroy(gameObject);
        }
    }
}
