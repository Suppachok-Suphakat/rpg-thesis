using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossEnemy : MonoBehaviour, BEnemy
{
    [SerializeField] private float chaseSpeed = 2;
    public Transform damageCollider;
    public Transform damageCollider2;

    private Animator myAnimator;
    private SpriteRenderer spriteRenderer;

    readonly int ATTACK_HASH = Animator.StringToHash("Attack");
    readonly int ATTACK2_HASH = Animator.StringToHash("Attack2");
    readonly int RANGEATTACK_HASH = Animator.StringToHash("RangeAttack");

    [Header("RangeAttack")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletMoveSpeed;
    [SerializeField] public int burstCount;
    [SerializeField] public int projectilesPerBurst;
    [SerializeField][Range(0, 359)] private float angleSpread;
    [SerializeField] private float startingDistance = 0.1f;
    [SerializeField] public float timeBetweenBursts;
    [SerializeField] private float restTime = 1f;

    private bool isShooting = false;

    private void Awake()
    {
        myAnimator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Attack()
    {
        myAnimator.SetTrigger(ATTACK_HASH);
    }

    public void Attack2()
    {
        myAnimator.SetTrigger(ATTACK2_HASH);
    }


    public void AttackAnimEvent()
    {
        damageCollider.gameObject.SetActive(true);
    }

    public void EndAttackAnimEvent()
    {
        damageCollider.gameObject.SetActive(false);
    }

    public void Attack2AnimEvent()
    {
        damageCollider2.gameObject.SetActive(true);
    }

    public void EndAttack2AnimEvent()
    {
        damageCollider2.gameObject.SetActive(false);
    }

    public void RangeAttack()
    {
        if (!isShooting)
        {
            StartCoroutine(ShootRoutine());
        }
    }

    private IEnumerator ShootRoutine()
    {
        isShooting = true;

        float startAngle, currentAngle, angleStep;

        TargetConeOfInfluence(out startAngle, out currentAngle, out angleStep);

        for (int i = 0; i < burstCount; i++)
        {
            myAnimator.SetTrigger(RANGEATTACK_HASH);
            for (int j = 0; j < projectilesPerBurst; j++)
            {
                Vector2 pos = FindBulletSpawnPos(currentAngle);

                GameObject newBullet = Instantiate(bulletPrefab, pos, Quaternion.identity);
                newBullet.transform.right = newBullet.transform.position - transform.position;


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
}
