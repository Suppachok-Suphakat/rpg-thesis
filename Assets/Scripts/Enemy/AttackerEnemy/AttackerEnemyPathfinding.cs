using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackerEnemyPathfinding : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2f;

    private Rigidbody2D rb;
    private Vector2 moveDir;
    private Knockback knockback;
    private SpriteRenderer spriteRenderer;

    private AttackerEnemyAI attackerEnemyAI;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        knockback = GetComponent<Knockback>();
        rb = GetComponent<Rigidbody2D>();
        attackerEnemyAI = GetComponent<AttackerEnemyAI>();
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
