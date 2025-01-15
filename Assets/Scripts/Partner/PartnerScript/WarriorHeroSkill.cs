using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WarriorHeroSkill : MonoBehaviour
{
    [Header("Hero Skill")]
    public GameObject skill1Damage;
    public GameObject skill1Prefab; // Prefab for the skill projectile
    public GameObject skill1Preview; // Visual for the preview (e.g., an arrow)
    private GameObject skill1PreviewInstance;
    public float skill1Range;
    public LineRenderer lineRenderer; // Reference to the LineRenderer
    public GameObject skill2Prefab;
    public GameObject skill2AreaPreview;
    private GameObject skill2PreviewInstance;

    private bool isSkill1PreviewActive = false;
    private bool isSkill1Active = false;
    private bool isSkill2Active = false;

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
    [SerializeField] float cooldownRecoveryTimer = 1;
    [SerializeField] float cooldownRecoveryDelay = 0.1f;
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
    public WarriorHeroAI warriorHeroAI;

    public ConversationManager conversationManager;

    private void Awake()
    {
        lineTrigger = GameObject.Find("Player").GetComponent<LineTrigger>();
        rb = GetComponent<Rigidbody2D>();
        warriorHeroAI = GetComponent<WarriorHeroAI>();
    }

    // Start is called before the first frame update
    void Start()
    {
        partnerSkillManager = PartnerSkillManager.Instance;
        //lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        //skill1Image.fillAmount = 0;
    }

    // Update is called once per frame
    void Update()
    {
        HandleSkill1();
        HandleSkill2();
        UpdateCooldownUI();

        if (isSkill1PreviewActive) // Ensure this only runs during the preview
        {
            // Set the start of the line to the hero's position
            lineRenderer.SetPosition(0, transform.position);

            // Set the end of the line to the mouse position
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0; // Ensure it stays on the 2D plane

            Vector3 direction = (mousePosition - transform.position).normalized;
            // Calculate the fixed endpoint based on the skill range
            Vector3 fixedEndpoint = transform.position + direction * skill1Range;
            lineRenderer.SetPosition(1, fixedEndpoint);
        }
    }

    private void HandleSkill1()
    {
        if (lineTrigger.currentTarget == this.transform)
        {
            if (Input.GetKeyDown(KeyCode.E) && !isSkill1Active) // Activate preview on first press
            {
                if (!isSkill1PreviewActive)
                {
                    EnableLineRenderer();
                    ActivateSkill1Preview();
                }
                else
                {
                    DisableLineRenderer();
                    Skill1Activate();
                }
            }

            if (isSkill1PreviewActive)
            {
                UpdateSkill1Preview(); // Update preview position and rotation
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

    private void ActivateSkill1Preview()
    {
        skill1PreviewInstance = Instantiate(skill1Preview, transform.position, Quaternion.identity);
        skill1PreviewInstance.transform.SetParent(transform); // Keep it relative to the hero
        isSkill1PreviewActive = true;
    }

    private void UpdateSkill1Preview()
    {
        if (skill1PreviewInstance != null)
        {
            Vector3 mousePosition = GetMouseWorldPosition();
            Vector3 direction = (mousePosition - transform.position).normalized;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            skill1PreviewInstance.transform.rotation = Quaternion.Euler(0, 0, angle);
        }
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

    private void HandleSkill2()
    {
        if (lineTrigger.currentTarget == this.transform)
        {
            if (Input.GetKeyDown(KeyCode.Q) && !isAnySkillActive) // Block if another skill is active
            {
                if (skill2CooldownTime <= 0 && !isSkill2Active) // Cooldown completed
                {
                    if (skill2PreviewInstance == null)
                    {
                        skill2PreviewInstance = Instantiate(skill2AreaPreview);
                    }
                    skill2PreviewInstance.SetActive(true);
                    isAnySkillActive = true; // Lock activation for other skills
                }
            }

            if (skill2PreviewInstance != null)
            {
                skill2PreviewInstance.transform.position = GetMouseWorldPosition();
                if (Input.GetMouseButtonDown(0))
                {
                    skill2PreviewInstance.SetActive(false); // Hide the preview
                    Destroy(skill2PreviewInstance); // Optionally destroy it
                    skill2PreviewInstance = null;

                    if (skill2CooldownTime <= 0) // Cooldown completed
                    {
                        GameObject skillInstance = Instantiate(skill2Prefab, GetMouseWorldPosition(), Quaternion.identity);
                        ActivateSkill2(skillInstance);
                        Skill2Activate();
                        isSkill2Active = true;
                        skill2CooldownTime = skill2MaxCooldownTime; // Reset cooldown
                    }

                    isAnySkillActive = false; // Release lock after using the skill
                }
            }
        }

        // Update cooldown over time
        if (skill2CooldownTime > 0)
        {
            skill2CooldownTime -= Time.deltaTime;
        }
        else
        {
            isSkill2Active = false; // Skill is ready again
        }
    }

    private void ActivateSkill2(GameObject skillInstance)
    {
        // Start a coroutine to handle the skill duration and exit animation
        StartCoroutine(HandleSkill2Exit(skillInstance));
    }

    private IEnumerator HandleSkill2Exit(GameObject skillInstance)
    {
        // Wait for the active duration
        yield return new WaitForSeconds(skill2ActiveTime);

        // Trigger the exit animation
        Animator animator = skillInstance.GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("Exit");

            // Wait for the animation to complete
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            yield return new WaitForSeconds(stateInfo.length);
        }

        // Destroy the skill instance
        Destroy(skillInstance);
    }

    public void Skill1Activate()
    {
        if (skill1PreviewInstance != null)
        {
            Vector3 mousePosition = GetMouseWorldPosition();
            Vector3 direction = (mousePosition - transform.position).normalized;

            // Calculate the rotation for the skill projectile
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.Euler(0, 0, angle);

            // Instantiate the projectile at the hero's position
            GameObject newArrow = Instantiate(skill1Prefab, transform.position, rotation);

            // Flip the projectile if the mouse is on the left side of the screen
            if (mousePosition.x < transform.position.x)
            {
                Vector3 scale = newArrow.transform.localScale;
                scale.y *= -1; // Flip Y-axis
                newArrow.transform.localScale = scale;
            }

            // Set projectile range and activate it
            newArrow.GetComponent<ThruProjectile>().UpdateProjectileRange(skill1Range);

            // Destroy the preview and disable the line renderer
            Destroy(skill1PreviewInstance);
            DisableLineRenderer();
            isSkill1PreviewActive = false;

            // Show dialogue and play animation
            conversationManager.ShowConversation("I’ll clear a path!", warriorHeroAI.heroFaceSprite);
            gameObject.GetComponent<Animator>().SetTrigger("skill1");

            // Start cooldown
            isSkill1Active = true;
            skill1CooldownTime = skill1MaxCooldownTime;
            UpdateCooldownUI(); // Update UI
        }
    }

    public void Skill2Activate()
    {
        Debug.Log("Partner Skill Activate");
        conversationManager.ShowConversation("Shield up!", warriorHeroAI.heroFaceSprite);
        gameObject.GetComponent<Animator>().SetTrigger("skill2");
        //knightHeroAI.SkillLogic();

        skill1CooldownTime = 0;  // Start cooldown
        UpdateCooldownUI();  // Update UI
    }

    public void FusionActivate()
    {
        PlayerController.instance.GetComponent<Animator>().SetTrigger("warriorLink");

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
        PlayerController.instance.GetComponent<Animator>().SetTrigger("warriorUnlink");

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

        StartCoroutine(ResetFusionTrigger());

        fusionActivated = false;
    }

    IEnumerator ResetFusionTrigger()
    {
        yield return new WaitForSeconds(0.1f);
        PlayerController.instance.GetComponent<Animator>().ResetTrigger("KnightFusionReturn");
    }

    public void OnSkillDamage()
    {
        skill1Damage.SetActive(true);
    }

    public void OffSkillDamage()
    {
        skill1Damage.SetActive(false);
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
}
