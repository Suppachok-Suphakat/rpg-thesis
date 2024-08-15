using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossEnemyPathfinding : MonoBehaviour
{
    [SerializeField] public float moveSpeed = 2f;

    private Rigidbody2D rb;
    private Vector2 moveDir;
    private Knockback knockback;
    private SpriteRenderer spriteRenderer;

    private BossEnemyAI bossEnemyAI;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        knockback = GetComponent<Knockback>();
        rb = GetComponent<Rigidbody2D>();
        bossEnemyAI = GetComponent<BossEnemyAI>();
    }


    private void FixedUpdate()
    {
        if (knockback.GettingKnockedBack) { return; }

        rb.MovePosition(rb.position + moveDir * (moveSpeed * Time.fixedDeltaTime));

        if (moveDir.x < 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
            //spriteRenderer.flipX = false;
        }
        else if (moveDir.x > 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
            //spriteRenderer.flipX = true;
        }
    }

    public void MoveTo(Vector2 targetPosition)
    {
        moveDir = targetPosition;
    }

    public void StopMoving()
    {
        moveDir = Vector3.zero;
    }
}
