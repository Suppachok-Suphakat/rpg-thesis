using System.Collections;
using UnityEngine;

public class ImmobilizingEffect : MonoBehaviour
{
    public float immobilizeDuration = 5f; // How long the enemy is immobilized

    private AttackerEnemyPathfinding attackerEnemyMovement;
    private EnemyPathfinding enemyMovement;

    void Start()
    {
        // Find movement components on the parent (the enemy)
        attackerEnemyMovement = GetComponentInParent<AttackerEnemyPathfinding>();
        enemyMovement = GetComponentInParent<EnemyPathfinding>();

        // Immobilize the enemy
        if (attackerEnemyMovement != null)
        {
            attackerEnemyMovement.isImmobilized = true;
        }
        if (enemyMovement != null)
        {
            enemyMovement.isImmobilized = true;
        }

        // Start the timer to re-enable movement and destroy this effect
        StartCoroutine(EndImmobilization());
    }

    private IEnumerator EndImmobilization()
    {
        // Wait for the immobilization duration
        yield return new WaitForSeconds(immobilizeDuration);

        // Re-enable movement
        if (attackerEnemyMovement != null)
        {
            attackerEnemyMovement.isImmobilized = false;
        }
        if (enemyMovement != null)
        {
            enemyMovement.isImmobilized = false;
        }

        // Destroy this GameObject
        Destroy(gameObject);
    }
}