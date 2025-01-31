using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class SkillAvatar : MonoBehaviour
{
    [SerializeField] private GameObject damageCollider;

    private Animator animator;
    private SpriteRenderer spriteRenderer;

    [SerializeField] private float attackCooldownTime;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        attackCooldownTime = 1f;
    }

    private void Update()
    {
        MouseFollowWithOffset();

        if (attackCooldownTime <= 1f)
        {
            attackCooldownTime += Time.deltaTime;
        }

        if (Input.GetMouseButtonDown(0) && attackCooldownTime >= 1f)
        {
            Attack();
        }
    }

    public void Attack()
    {
        if (!PlayerController.instance.isMenuActive)
        {
            animator.SetTrigger("attack");
            damageCollider.gameObject.SetActive(true);
            attackCooldownTime = 0;
        }
    }

    public void DoneAttackingAnimEvent()
    {
        damageCollider.gameObject.SetActive(false);
    }

    private void MouseFollowWithOffset()
    {
        Vector3 mousePos = Input.mousePosition;
        Vector3 playerScreenPoint = Camera.main.WorldToScreenPoint(transform.position);

        if (mousePos.x < playerScreenPoint.x)
        {
            //spriteRenderer.flipX = true;
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else
        {
            //spriteRenderer.flipX = false;
            transform.localScale = new Vector3(1, 1, 1);
        }
    }
}
