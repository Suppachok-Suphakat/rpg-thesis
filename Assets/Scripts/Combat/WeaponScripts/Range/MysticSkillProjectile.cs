using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MysticSkillProjectile : MonoBehaviour
{
    public int damage;

    [SerializeField] private float moveSpeed = 22f;
    [SerializeField] private GameObject particleOnHitPrefabVFX;
    [SerializeField] private bool isEnemyProjectile = false;
    [SerializeField] private float projectileRange = 10f;
    [SerializeField] private float pullRadius = 3f; // Radius of pull effect
    [SerializeField] private float pullForce = 5f; // Strength of the pull force
    [SerializeField] private float pullInterval = 0.1f; // How often to pull enemies

    private Vector3 startPosition;
    private bool isPulling = true;

    private void Start()
    {
        startPosition = transform.position;
        StartCoroutine(PullEnemiesCoroutine());
    }

    private void Update()
    {
        if (isEnemyProjectile)
        {

        }

        MoveProjectile();
        DetectFireDistance();
    }

    public void UpdateProjectileRange(float projectileRange)
    {
        this.projectileRange = projectileRange;
    }

    private void MoveProjectile()
    {
        transform.Translate(Vector3.right * Time.deltaTime * moveSpeed);
    }

    private void DetectFireDistance()
    {
        if (Vector3.Distance(transform.position, startPosition) > projectileRange)
        {
            DestroyProjectile();
        }
    }

    private void DestroyProjectile()
    {
        isPulling = false; // Stop pulling enemies
        Destroy(gameObject);
    }

    private IEnumerator PullEnemiesCoroutine()
    {
        while (isPulling)
        {
            Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, pullRadius, LayerMask.GetMask("Enemy"));

            foreach (Collider2D enemy in enemies)
            {
                Rigidbody2D enemyRb = enemy.GetComponent<Rigidbody2D>();

                EnemyPathfinding enemyPathfinding = enemy.GetComponent<EnemyPathfinding>();
                AttackerEnemyPathfinding attackerPathfinding = enemy.GetComponent<AttackerEnemyPathfinding>();
                LockShooterEnemyAI lockShooterEnemyAI = enemy.GetComponent<LockShooterEnemyAI>();

                if (enemyRb != null)
                {
                    // Disable pathfinding to avoid movement conflicts
                    if (enemyPathfinding != null) enemyPathfinding.isImmobilized = true;
                    if (attackerPathfinding != null) attackerPathfinding.isImmobilized = true;
                    if (lockShooterEnemyAI != null) lockShooterEnemyAI.isImmobilized = true;

                    // Move enemy smoothly towards the projectile
                    Vector2 directionToProjectile = (transform.position - enemy.transform.position).normalized;
                    Vector2 targetPosition = (Vector2)enemy.transform.position + directionToProjectile * pullForce * Time.deltaTime;

                    enemyRb.MovePosition(Vector2.Lerp(enemyRb.position, targetPosition, 0.5f));
                }
            }

            yield return new WaitForSeconds(pullInterval);
        }

        // Re-enable pathfinding for all affected enemies after the projectile disappears
        foreach (Collider2D enemy in Physics2D.OverlapCircleAll(transform.position, pullRadius, LayerMask.GetMask("Enemy")))
        {
            EnemyPathfinding enemyPathfinding = enemy.GetComponent<EnemyPathfinding>();
            AttackerEnemyPathfinding attackerPathfinding = enemy.GetComponent<AttackerEnemyPathfinding>();
            LockShooterEnemyAI lockShooterEnemyAI = enemy.GetComponent<LockShooterEnemyAI>();

            if (enemyPathfinding != null) enemyPathfinding.isImmobilized = false;
            if (attackerPathfinding != null) attackerPathfinding.isImmobilized = false;
            if (lockShooterEnemyAI != null) lockShooterEnemyAI.isImmobilized = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                // Deal damage or apply effects if needed
            }
        }
    }

    public void UpdateMoveSpeed(float moveSpeed)
    {
        this.moveSpeed = moveSpeed;
    }
}
