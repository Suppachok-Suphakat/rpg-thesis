using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TextCore.Text;
using Cinemachine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController instance;
    public CinemachineVirtualCamera virtualCamera;

    public bool FacingLeft { get { return facingLeft; } set { facingLeft = value; } }

    private bool facingLeft = false;

    private bool isWalkingBack;

    private PlayerControls playerControls;
    public Vector2 movement;

    public float moveSpeed = 5f;
    public float runSpeed = 8f;
    public float startingMoveSpeed;
    public float currentMoveSpeed;

    [SerializeField] private float dashSpeed = 4f;
    [SerializeField] private int dashStamina = 100;
    [SerializeField] public TrailRenderer trailRenderer;
    [SerializeField] private Transform swordWeaponCollider;
    [SerializeField] private Transform spearWeaponCollider;

    private Knockback knockback;

    private bool isDashing;

    private Vector2 input;

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;

    public LayerMask solidObjectsLayer;
    public LayerMask interactableLayer;
    public bool isMenuActive = false;

    private Character character;

    private void Awake()
    {
        instance = this;

        playerControls = new PlayerControls();

        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        animator = GetComponentInChildren<Animator>();

        rb = GetComponent<Rigidbody2D>();
        character = GetComponent<Character>();
        knockback = GetComponent<Knockback>();
    }

    private void Start()
    {
        playerControls.Combat.Dash.performed += _ => Dash();
        startingMoveSpeed = moveSpeed;
    }

    private void OnEnable()
    {
        playerControls.Enable();
    }

    private void Update()
    {
        if (!isMenuActive)
        {
            HandleUpdate();
        }
    }

    public void HandleUpdate()
    {
        PlayerInput();

        if (Input.GetKeyDown(KeyCode.E))
        {
            Interact();
        }
    }

    public Transform GetSwordWeaponCollider()
    {
        return swordWeaponCollider;
    }
    public Transform GetSpearWeaponCollider()
    {
        return spearWeaponCollider;
    }

    private void FixedUpdate()
    {
        AdjustPlayerFacingDirection();
        Move();
    }

    private void PlayerInput()
    {
        movement = playerControls.Movement.Move.ReadValue<Vector2>();
        animator.SetFloat("moveX", movement.x);
        animator.SetFloat("moveY", movement.y);

        // Check if the player is moving
        if (movement.magnitude > 0)
        {
            animator.SetBool("Idle", false); // Player is not idle
            CheckIfWalkingBack(); // Check if player is walking back
        }
        else
        {
            animator.SetBool("Idle", true); // Player is idle
            animator.SetBool("isWalkingBack", false); // Not walking back when idle
        }
    }

    private void CheckIfWalkingBack()
    {
        // Get the mouse position in world space and convert it to Vector2
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // Calculate the player's movement direction and the direction towards the mouse
        Vector2 playerPosition = transform.position;
        Vector2 moveDirection = movement.normalized;
        Vector2 mouseDirection = (mouseWorldPos - playerPosition).normalized;

        // Check if the player is moving away from the mouse position
        bool isMovingAway = Vector2.Dot(moveDirection, mouseDirection) < 0;

        // Set the animation parameter based on the direction check
        animator.SetBool("isWalkingBack", isMovingAway);
    }

    private void Move()
    {
        if (knockback.GettingKnockedBack || character.isDead) { return; }
        rb.MovePosition(rb.position + movement * (moveSpeed * Time.fixedDeltaTime));
    }

    public void Warp(Vector3 newPosition)
    {
        // Move player to the new position
        transform.position = newPosition;

        // Reset Cinemachine's position
        if (virtualCamera != null)
        {
            virtualCamera.OnTargetObjectWarped(transform, newPosition - transform.position);
        }

        Debug.Log("Player teleported, camera synced.");
    }

    void Interact()
    {
        var collider = Physics2D.OverlapCircle(transform.position, 1f, interactableLayer);
        if (collider != null)
        {
            collider.GetComponent<Interactable>()?.Interact();
        }
    }

    private void AdjustPlayerFacingDirection()
    {
        Vector3 mousePos = Input.mousePosition;
        Vector3 playerScreenPoint = Camera.main.WorldToScreenPoint(transform.position);

        if (mousePos.x < playerScreenPoint.x)
        {
            spriteRenderer.flipX = true;
            facingLeft = true;
        }
        else
        {
            spriteRenderer.flipX = false;
            facingLeft = false;
        }
    }

    private void Dash()
    {
        if (!isDashing && character.stamina.currVal >= dashStamina && !isMenuActive)
        {
            isDashing = true;
            moveSpeed *= dashSpeed;
            trailRenderer.emitting = true;
            StartCoroutine(EndDashRoutine());
        }
    }

    private IEnumerator EndDashRoutine()
    {
        float dashTime = .2f;
        float dashCD = .25f;
        character.ReduceStamina(dashStamina);
        yield return new WaitForSeconds(dashTime);
        if (character.isLeftShiftPressed)
        {
            moveSpeed = runSpeed;
        }
        else
        {
            moveSpeed = startingMoveSpeed;
        }
        trailRenderer.emitting = false;
        yield return new WaitForSeconds(dashCD);
        isDashing = false;
    }
}
