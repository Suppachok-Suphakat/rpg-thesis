using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MysticHeroSkill : MonoBehaviour
{
    [Header("Skill 01")]
    [SerializeField] private GameObject weaponGO;
    [SerializeField] public GameObject weaponInstance;
    [SerializeField] private Transform swordPoint;
    [Header("Skill 02")]
    public float skill2Range;
    public LineRenderer lineRenderer;
    public GameObject skill2Prefab;
    public GameObject skill2Preview;
    public GameObject skill2AreaPreview;
    public GameObject skill2PreviewInstance;
    Vector3 mousePosition;
    Vector3 direction;

    public bool isSkill2PreviewActive = false;
    private bool isSkill1Active = false;
    private bool isSkill2Active = false;
    public bool isReady = false;

    [Header("Skill Settings")]
    [SerializeField] float skill1CooldownTime;
    [SerializeField] float skill1MaxCooldownTime;
    [SerializeField] float skill2CooldownTime;
    [SerializeField] float skill2MaxCooldownTime;

    [Header("Skill 1")]
    [SerializeField] int skill1CurrentCooldownTime;
    [SerializeField] float skill1ActiveTime;
    [SerializeField] float currentSkill1ActiveTime;

    [Header("Skill 2")]
    [SerializeField] int skill2CurrentCooldownTime;
    [SerializeField] float skill2ActiveTime;
    [SerializeField] float currentSkill2ActiveTime;

    [Header("Skill")]
    //[SerializeField] float cooldownRecoveryTimer = 1;
    //[SerializeField] float cooldownRecoveryDelay = 0.1f;
    private bool isAnySkillActive = false;

    private bool fusionActivated = false;

    [SerializeField] private StatusBar statusComponent;
    PartnerSkillManager partnerSkillManager;

    [Header("Skill Change")]
    [SerializeField] public ToolbarSlot toolbarSlot;
    [SerializeField] public WeaponInfo weaponInfo;
    [SerializeField] public Sprite itemSprite;

    WeaponInfo weaponChangeInfo;
    Sprite weaponChangeSprite;
    public ActiveToolbar activeToolbar;

    [SerializeField] public Image skill1Image;
    [SerializeField] public Image skill2Image;

    private LineTrigger lineTrigger;
    private Rigidbody2D rb;
    public MysticHeroAI mysticHeroAI;

    private bool isWaitingForAttack = false;
    private Transform targetEnemy; // The enemy to teleport to

    private Animator animator;

    public ConversationManager conversationManager;

    private void Awake()
    {
        lineTrigger = GameObject.Find("Player").GetComponent<LineTrigger>();
        rb = GetComponent<Rigidbody2D>();
        mysticHeroAI = GetComponent<MysticHeroAI>();
        animator = GetComponent<Animator>();
    }

    // Start is called before the first frame update
    void Start()
    {
        partnerSkillManager = PartnerSkillManager.Instance;
        //lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        //skill1Image.fillAmount = 0;
        DisableLineRenderer();
    }

    // Update is called once per frame
    void Update()
    {
        HandleSkill1();
        HandleSkill2();
        UpdateCooldownUI();

        if (isSkill2PreviewActive) // Ensure this only runs during the preview
        {
            // Set the start of the line to the hero's position
            lineRenderer.SetPosition(0, transform.position);

            // Set the end of the line to the mouse position
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0; // Ensure it stays on the 2D plane

            Vector3 direction = (mousePosition - transform.position).normalized;
            // Calculate the fixed endpoint based on the skill range
            Vector3 fixedEndpoint = transform.position + direction * skill2Range;
            lineRenderer.SetPosition(1, fixedEndpoint);
        }
    }

    private void HandleSkill1()
    {
        if (lineTrigger.currentTarget == this.transform)
        {
            if (Input.GetKeyDown(KeyCode.E) && !isAnySkillActive) // Activate preview on first press
            {
                if (skill1CooldownTime <= 0)
                {
                    Skill1Activate();
                    isAnySkillActive = true; //cannot use skill 2 or any skill
                }
            }
        }

        // Cooldown handling
        if (skill1CooldownTime > 0)
        {
            skill1CooldownTime -= Time.deltaTime;
        }
        else
        {
            isSkill1Active = false; // Skill is ready again
        }
    }

    public void Skill1Activate()
    {
        if (isSkill1Active) return;

        isSkill1Active = true;
        isAnySkillActive = true; // Prevent other skills

        animator.SetTrigger("skill1");

        // Start the coroutine and wait before proceeding
        StartCoroutine(ActivateSkill1AfterAnimation());
    }

    private IEnumerator ActivateSkill1AfterAnimation()
    {
        yield return StartCoroutine(WaitForAnimation("Mystic_Skill_01")); // Wait for animation to finish

        // Now instantiate the weapon after the animation completes
        weaponInstance = Instantiate(weaponGO, swordPoint.position, Quaternion.identity);
        StartCoroutine(DeactivateSkill1AfterTime(5f)); // Wait for destruction

        conversationManager.ShowConversation("Spirit Sword!", mysticHeroAI.heroFaceSprite);
        UpdateCooldownUI();
    }

    private IEnumerator WaitForAnimation(string animationName)
    {
        // Ensure the animation is playing before checking its progress
        while (!animator.GetCurrentAnimatorStateInfo(0).IsName(animationName))
        {
            yield return null;
        }

        // Wait for the animation to finish
        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.5f)
        {
            yield return null;
        }
    }

    private IEnumerator DeactivateSkill1AfterTime(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (weaponInstance != null)
        {
            Destroy(weaponInstance);
        }

        isSkill1Active = false;
        isAnySkillActive = false;
    }

    private void HandleSkill2()
    {
        if (lineTrigger.currentTarget == this.transform)
        {
            if (Input.GetKeyDown(KeyCode.Q) && !isAnySkillActive) // Activate preview on first press
            {
                if (skill2CooldownTime <= 0 && !isSkill2PreviewActive)
                {
                    EnableLineRenderer();
                    ActivateSkill2Preview();
                    isAnySkillActive = true;
                }
            }

            if (lineRenderer.enabled)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    DisableLineRenderer();
                    Skill2Activate();
                    isAnySkillActive = false;
                }
            }

            if (isSkill2PreviewActive)
            {
                UpdateSkill2Preview(); // Update preview position and rotation
            }
        }

        // Cooldown handling
        if (skill2CooldownTime > 0)
        {
            skill2CooldownTime -= Time.deltaTime;
        }
        else
        {
            isSkill2Active = false; // Skill is ready again
        }
    }

    private void ActivateSkill2Preview()
    {
        skill2PreviewInstance = Instantiate(skill2Preview, transform.position, Quaternion.identity);
        skill2PreviewInstance.transform.SetParent(transform); // Keep it relative to the hero
        isSkill2PreviewActive = true;
    }

    private void UpdateSkill2Preview()
    {
        if (skill2PreviewInstance != null)
        {
            Vector3 mousePosition = GetMouseWorldPosition();
            Vector3 direction = (mousePosition - transform.position).normalized;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            skill2PreviewInstance.transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    public void Skill2Activate()
    {
        if (skill2PreviewInstance != null)
        {
            // Show dialogue and play animation
            conversationManager.ShowConversation("Spirit of dragon!", mysticHeroAI.heroFaceSprite);
            animator.SetTrigger("skill2");
            StartCoroutine(ActivateSkill2AfterAnimation());

            mousePosition = GetMouseWorldPosition();
            direction = (mousePosition - transform.position).normalized;

            mysticHeroAI.SetFlipOverride(true, mousePosition);
            mysticHeroAI.FlipSpriteOverride(mousePosition); // <-- Explicitly call flip
        }
    }

    private IEnumerator ActivateSkill2AfterAnimation()
    {
        yield return StartCoroutine(WaitForAnimation("Mystic_Skill_02")); // Wait for animation to finish

        // Calculate the rotation for the skill projectile
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0, 0, angle);

        // Instantiate the projectile at the hero's position
        GameObject newArrow = Instantiate(skill2Prefab, transform.position, rotation);

        // Flip the projectile if the mouse is on the left side of the screen
        if (mousePosition.x < transform.position.x)
        {
            Vector3 scale = newArrow.transform.localScale;
            scale.y *= -1; // Flip Y-axis
            newArrow.transform.localScale = scale;
        }

        // Set projectile range and activate it
        newArrow.GetComponent<MysticSkillProjectile>().UpdateProjectileRange(skill2Range);

        // Destroy the preview and disable the line renderer
        Destroy(skill2PreviewInstance);
        DisableLineRenderer();
        isSkill2PreviewActive = false;

        // Start cooldown
        isSkill2Active = true;
        skill2CooldownTime = skill2MaxCooldownTime;
        UpdateCooldownUI(); // Update UI

        // Reset flip override after a delay
        StartCoroutine(ResetFlipAfterDelay(0.45f)); // Adjust delay as needed
    }

    private IEnumerator ResetFlipAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        mysticHeroAI.SetFlipOverride(false, Vector3.zero);
    }

    void EnableLineRenderer()
    {
        lineRenderer.enabled = true; // Enable the LineRenderer
    }

    void DisableLineRenderer()
    {
        lineRenderer.enabled = false; // Disable the LineRenderer
    }

    void ClearLineRenderer()
    {
        lineRenderer.positionCount = 0; // Clear all positions
    }

    public void FusionActivate()
    {
        PlayerController.instance.GetComponent<Animator>().SetTrigger("mysticLink");

        if (toolbarSlot != null)
        {
            toolbarSlot.gameObject.SetActive(true); // Ensure toolbarSlot is active
            StartCoroutine(EnsureUIUpdate()); // Ensure UI is updated
        }

        if (toolbarSlot.weaponInfo != null)
        {
            weaponChangeInfo = toolbarSlot.weaponInfo;
        }
        weaponChangeSprite = toolbarSlot.slotSprite.GetComponent<Image>().sprite;

        toolbarSlot.weaponInfo = weaponInfo;
        toolbarSlot.slotSprite.GetComponent<Image>().sprite = itemSprite;
        activeToolbar.ChangeActiveWeapon();

        fusionActivated = true;
    }

    public IEnumerator EnsureUIUpdate()
    {
        yield return null; // Wait for a frame
        if (toolbarSlot != null && toolbarSlot.gameObject.activeInHierarchy)
        {
            var imageComponent = toolbarSlot.slotSprite.GetComponent<Image>();
            if (imageComponent != null)
            {
                imageComponent.sprite = itemSprite;
            }
        }
    }

    public void DeFusionActivate()
    {
        // Trigger return to normal state
        PlayerController.instance.GetComponent<Animator>().SetTrigger("mysticUnlink");

        // Handle weapon change back
        if (weaponChangeInfo != null)
        {
            toolbarSlot.weaponInfo = weaponChangeInfo;
            toolbarSlot.slotSprite.GetComponent<Image>().sprite = weaponChangeSprite;
        }
        else
        {
            toolbarSlot.weaponInfo = null;
            toolbarSlot.slotSprite.GetComponent<Image>().sprite = weaponChangeSprite;
        }

        activeToolbar.ChangeActiveWeapon();

        // Stop Skill 1
        isSkill1Active = false;
        isWaitingForAttack = false;
        skill1CooldownTime = 0; // Reset cooldown
        targetEnemy = null; // Clear the target

        fusionActivated = false;
    }


    IEnumerator ResetFusionTrigger()
    {
        yield return new WaitForSeconds(0.1f);
        PlayerController.instance.GetComponent<Animator>().ResetTrigger("mysticUnlink");
    }

    private void UpdateCooldownUI()
    {
        // Update skill cooldown UI
        skill1Image.fillAmount = Mathf.Clamp01(1 - (skill1CooldownTime / skill1MaxCooldownTime));
        skill2Image.fillAmount = Mathf.Clamp01(1 - (skill2CooldownTime / skill2MaxCooldownTime));
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0; // Make sure it's on the same Z plane
        return mousePosition;
    }

    void NoYellowKEKW()
    {
        if (isSkill1Active)

        if (fusionActivated)

        if (isWaitingForAttack)

        if (isSkill2Active)

        if(targetEnemy == null)
        {

        }
    }
}
