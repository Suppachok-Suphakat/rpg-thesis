using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Attacker : MonoBehaviour, IEnemy
{
    //public Transform player;

    [SerializeField] private float chaseSpeed = 2;
    public Transform damageCollider;

    private Animator myAnimator;
    private SpriteRenderer spriteRenderer;

    readonly int ATTACK_HASH = Animator.StringToHash("Attack");

    private void Awake()
    {
        myAnimator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Attack()
    {
        myAnimator.SetTrigger(ATTACK_HASH);
    }

    public void AttackAnimEvent()
    {
        damageCollider.gameObject.SetActive(true);
    }

    public void EndAttackAnimEvent()
    {
        damageCollider.gameObject.SetActive(false);
    }

    void FlipSprite()
    {
        // Flip the sprite based on the player's facing direction
        if (PlayerController.instance.transform.position.x < transform.position.x)
            transform.localScale = new Vector3(-1, 1, 1); // Flip the sprite to face left
        else
            transform.localScale = new Vector3(1, 1, 1); // Reset the scale to face right
    }
}
