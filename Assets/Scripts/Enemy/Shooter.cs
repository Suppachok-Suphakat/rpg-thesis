using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooter : MonoBehaviour, IEnemy
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletMoveSpeed;
    [SerializeField] private int burstCount;
    [SerializeField] private int projectilesPerBurst;
    [SerializeField][Range(0, 359)] private float angleSpread;
    [SerializeField] private float startingDistance = 0.1f;
    [SerializeField] private float timeBetweenBursts;
    [SerializeField] private float restTime = 1f;
    [SerializeField] private float animationDelay = 0.5f; // Delay before shooting for animation sync
    [SerializeField] private Transform bulletSpawnpoint; // Delay before shooting for animation sync

    private bool isShooting = false;

    public void Attack()
    {
        if (!isShooting)
        {
            StartCoroutine(ShootRoutine());
        }
    }

    private IEnumerator ShootRoutine()
    {
        isShooting = true;

        // Flip to face the player at the start of shooting
        FlipSprite(PlayerController.instance.transform);

        float startAngle, currentAngle, angleStep;

        TargetConeOfInfluence(out startAngle, out currentAngle, out angleStep);

        for (int i = 0; i < burstCount; i++)
        {
            yield return new WaitForSeconds(animationDelay); // Add delay for animation

            for (int j = 0; j < projectilesPerBurst; j++)
            {
                //Vector2 pos = FindBulletSpawnPos(currentAngle);

                //GameObject newBullet = Instantiate(bulletPrefab, pos, Quaternion.identity);
                //newBullet.transform.right = newBullet.transform.position - transform.position;

                Quaternion rotation = Quaternion.Euler(0, 0, currentAngle);
                GameObject newBullet = Instantiate(bulletPrefab, bulletSpawnpoint.position, rotation);

                if (newBullet.TryGetComponent(out Projectile projectile))
                {
                    projectile.UpdateMoveSpeed(bulletMoveSpeed);
                }

                currentAngle += angleStep;
            }

            currentAngle = startAngle;

            yield return new WaitForSeconds(timeBetweenBursts);
            TargetConeOfInfluence(out startAngle, out currentAngle, out angleStep);
        }

        yield return new WaitForSeconds(restTime);
        isShooting = false;
    }

    private void TargetConeOfInfluence(out float startAngle, out float currentAngle, out float angleStep)
    {
        Vector2 targetDirection = PlayerController.instance.transform.position - transform.position;
        float targetAngle = Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg;
        startAngle = targetAngle;
        float endAngle = targetAngle;
        currentAngle = targetAngle;
        float halfAngleSpread = 0f;
        angleStep = 0;
        if (angleSpread != 0)
        {
            angleStep = angleSpread / (projectilesPerBurst - 1);
            halfAngleSpread = angleSpread / 2f;
            startAngle = targetAngle - halfAngleSpread;
            endAngle = targetAngle + halfAngleSpread;
            currentAngle = startAngle;
        }
    }

    private Vector2 FindBulletSpawnPos(float currentAngle)
    {
        float x = transform.position.x + startingDistance * Mathf.Cos(currentAngle * Mathf.Deg2Rad);
        float y = transform.position.y + startingDistance * Mathf.Sin(currentAngle * Mathf.Deg2Rad);

        Vector2 pos = new Vector2(x, y);

        return pos;
    }

    private void FlipSprite(Transform flipTo)
    {
        if (flipTo.position.x < transform.position.x)
            transform.localScale = new Vector3(1, 1, 1);
        else
            transform.localScale = new Vector3(-1, 1, 1);
    }

    public void MagicAttackEvent()
    {
        if (!isShooting)
        {
            StartCoroutine(MagicRoutine());
        }
    }

    private IEnumerator MagicRoutine()
    {
        isShooting = true;

        // Flip to face the player at the start of shooting
        FlipSprite(PlayerController.instance.transform);

        yield return new WaitForSeconds(animationDelay); // Add delay for animation

        GameObject newMagic = Instantiate(bulletPrefab, PlayerController.instance.transform.position, Quaternion.identity);

        yield return new WaitForSeconds(restTime);
        isShooting = false;
    }
}

