using System.Collections;
using UnityEngine;

public class EnemyPathfinding : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2f;

    private Rigidbody2D rb;
    private Vector2 moveDir;
    private Knockback knockback;
    private SpriteRenderer spriteRenderer;

    public float stopMovingTime;
    public bool isImmobilized = false;

    private EnemyAI enemyAI;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        knockback = GetComponent<Knockback>();
        rb = GetComponent<Rigidbody2D>();
        enemyAI = GetComponent<EnemyAI>();
    }


    private void FixedUpdate()
    {
        if (isImmobilized) { return; }
        if (knockback.GettingKnockedBack) { return; }

        rb.MovePosition(rb.position + moveDir * (moveSpeed * Time.fixedDeltaTime));

        //if (moveDir.x < 0)
        //    spriteRenderer.flipX = true;
        //else if (moveDir.x > 0)
        //    spriteRenderer.flipX = false;

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
        isImmobilized = true;
        moveDir = Vector2.zero;

        // Make Rigidbody2D Kinematic to prevent physics interactions
        rb.bodyType = RigidbodyType2D.Kinematic;

        StartCoroutine(StopMovingForSeconds());
    }

    IEnumerator StopMovingForSeconds()
    {
        yield return new WaitForSeconds(stopMovingTime);

        // Restore Rigidbody2D to Dynamic so it can move again
        rb.bodyType = RigidbodyType2D.Dynamic;

        isImmobilized = false;
    }
}