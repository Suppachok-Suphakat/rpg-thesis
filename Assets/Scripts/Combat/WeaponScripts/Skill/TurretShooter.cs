using System.Collections;
using UnityEngine;

public class TurretShooter : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletMoveSpeed = 5f;
    [SerializeField] private Transform[] bulletSpawnpoints;
    [SerializeField] private Animator animator;
    [SerializeField] private float fireRate = 1.0f; // Fire every second
    [SerializeField] private float exitTime = 10f;
    [SerializeField] private float exitAnimationDuration = 1.0f;

    private Transform targetEnemy;
    private bool isExiting = false;
    private int currentDirectionIndex = 0;

    private Vector2[] shootDirections = {
        Vector2.up, Vector2.down, Vector2.left, Vector2.right,
        new Vector2(1, 1).normalized,  new Vector2(1, -1).normalized,
        new Vector2(-1, 1).normalized, new Vector2(-1, -1).normalized
    };

    private string[] animationTriggers = {
        "ShootUp", "ShootDown", "ShootLeft", "ShootRight",
        "ShootUpRight", "ShootDownRight", "ShootUpLeft", "ShootDownLeft"
    };

    private void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (animator == null)
            Debug.LogError("Animator component is missing on " + gameObject.name);

        if (bulletSpawnpoints.Length != 8)
            Debug.LogError("TurretShooter requires exactly 8 bullet spawn points!");

        Invoke(nameof(StartSelfDestruct), exitTime);
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
            StopShooting();
        }
    }

    private void StartShooting()
    {
        if (targetEnemy != null)
        {
            InvokeRepeating(nameof(ShootBullet), 0f, fireRate);
        }
    }

    private void StopShooting()
    {
        CancelInvoke(nameof(ShootBullet));
    }

    private void ShootBullet()
    {
        if (targetEnemy == null)
        {
            StopShooting();
            return;
        }

        Vector2 direction = GetShootDirection(targetEnemy.position);
        TriggerAnimation(currentDirectionIndex);
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

        currentDirectionIndex = bestIndex;
        return shootDirections[bestIndex];
    }

    private void TriggerAnimation(int index)
    {
        if (animator != null)
        {
            animator.SetTrigger(animationTriggers[index]);
        }
    }

    // **CALL THIS METHOD IN THE ANIMATION EVENT**
    public void ShootFromAnimation()
    {
        int spawnIndex = currentDirectionIndex;
        if (spawnIndex < 0 || spawnIndex >= bulletSpawnpoints.Length) return;

        Transform spawnPoint = bulletSpawnpoints[spawnIndex];
        GameObject newBullet = Instantiate(bulletPrefab, spawnPoint.position, Quaternion.identity);
        Rigidbody2D rb = newBullet.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.velocity = shootDirections[spawnIndex] * bulletMoveSpeed;
        }

        float angle = Mathf.Atan2(shootDirections[spawnIndex].y, shootDirections[spawnIndex].x) * Mathf.Rad2Deg;
        newBullet.transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void StartSelfDestruct()
    {
        if (!isExiting)
        {
            isExiting = true;
            animator.SetTrigger("Exit");
            Invoke(nameof(DestroyTurret), exitAnimationDuration);
        }
    }

    private void DestroyTurret()
    {
        Destroy(gameObject);
    }
}

