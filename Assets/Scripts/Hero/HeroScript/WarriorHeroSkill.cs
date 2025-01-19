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

    [SerializeField] private float duration = 1f;
    [SerializeField] private AnimationCurve animCurve;
    [SerializeField] private float heightY = 3f;
    [SerializeField] private GameObject splatterProjectileShadow;
    [SerializeField] private GameObject landingShadowPrefab; // Reference to the landing shadow prefab
    [SerializeField] private Vector3 maxShadowScale = new Vector3(3f, 3f, 1f); // Maximum size for the shadow

    private GameObject landingShadowInstance;
    private Vector3 targetPosition;
    private Vector3 ToLandPosition;
    [SerializeField] private GameObject shadow;

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
            if (Input.GetKeyDown(KeyCode.E) && !isAnySkillActive) // Activate preview on first press
            {
                if (skill1CooldownTime <= 0 && !isSkill1PreviewActive)
                {
                    EnableLineRenderer();
                    ActivateSkill1Preview();
                    isAnySkillActive = true;
                }
            }

            if (lineRenderer.enabled)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    DisableLineRenderer();
                    Skill1Activate();
                    isAnySkillActive = false;
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
            if (Input.GetKeyDown(KeyCode.Q) && !isAnySkillActive)
            {
                if (skill2CooldownTime <= 0 && !isSkill2Active)
                {
                    if (skill2PreviewInstance == null)
                    {
                        shadow.SetActive(false);
                        skill2PreviewInstance = Instantiate(skill2AreaPreview);
                    }
                    skill2PreviewInstance.SetActive(true);
                    isAnySkillActive = true;
                }
            }

            if (skill2PreviewInstance != null)
            {
                Vector3 mousePosition = GetMouseWorldPosition();
                skill2PreviewInstance.transform.position = mousePosition;

                if (Input.GetMouseButtonDown(0))
                {
                    if (skill2PreviewInstance != null)
                    {
                        Destroy(skill2PreviewInstance);
                        skill2PreviewInstance = null;
                    }

                    if (skill2CooldownTime <= 0)
                    {
                        isSkill2Active = true;
                        skill2CooldownTime = skill2MaxCooldownTime; // Reset cooldown
                        StartCoroutine(JumpToPosition(mousePosition));
                    }
                }
            }
        }

        if (skill2CooldownTime > 0)
        {
            skill2CooldownTime -= Time.deltaTime;
        }
        else
        {
            isSkill2Active = false;
        }
    }

    private IEnumerator WaitForSeconds(float seconds)
    {
        // First jump up
        yield return new WaitForSeconds(seconds); // Adjust time as needed for the jump-up animation
    }

    private IEnumerator JumpToPosition(Vector3 targetPosition)
    {
        conversationManager.ShowConversation("Fire kick!", warriorHeroAI.heroFaceSprite);
        gameObject.GetComponent<Animator>().SetTrigger("skill2");

        //Vector3 mousePosition = GetMouseWorldPosition();
        //targetPosition = mousePosition;
        ToLandPosition = targetPosition;

        GameObject splatterShadow = Instantiate(splatterProjectileShadow, transform.position + new Vector3(0, -0.3f, 0), Quaternion.identity);
        landingShadowInstance = Instantiate(landingShadowPrefab, targetPosition, Quaternion.identity);
        landingShadowInstance.transform.localScale = Vector3.zero; // Start with zero scale

        StartCoroutine(ProjectileCurveRoutine(transform.position, targetPosition));
        StartCoroutine(MoveSplatterShadowRoutine(splatterShadow, splatterShadow.transform.position, targetPosition));

        //transform.position = targetPosition;
        yield return null;
    }

    private IEnumerator ProjectileCurveRoutine(Vector3 startPosition, Vector3 endPosition)
    {
        float timePassed = 0f;

        while (timePassed < duration)
        {
            timePassed += Time.deltaTime;
            float linearT = timePassed / duration;
            float heightT = animCurve.Evaluate(linearT);
            float height = Mathf.Lerp(0f, heightY, heightT);

            // Update projectile position
            transform.position = Vector2.Lerp(startPosition, endPosition, linearT) + new Vector2(0f, height);

            // Scale the landing shadow proportionally to time
            if (landingShadowInstance != null)
            {
                landingShadowInstance.transform.localScale = Vector3.Lerp(Vector3.zero, maxShadowScale, linearT);
                gameObject.GetComponent<Animator>().SetTrigger("skill2Finish");
            }

            yield return null;
        }

        // Instantiate the splatter at the exact position of the landing shadow
        GameObject skillInstance = Instantiate(skill2Prefab, landingShadowInstance.transform.position, Quaternion.identity);

        Destroy(landingShadowInstance); // Remove shadow upon landing
        isAnySkillActive = false;
        shadow.SetActive(true);
        Destroy(skillInstance, skill2ActiveTime);
    }

    private IEnumerator MoveSplatterShadowRoutine(GameObject splatterShadow, Vector3 startPosition, Vector3 endPosition)
    {
        float timePassed = 0f;

        while (timePassed < duration)
        {
            timePassed += Time.deltaTime;
            float linearT = timePassed / duration;
            splatterShadow.transform.position = Vector2.Lerp(startPosition, endPosition, linearT);
            yield return null;
        }

        Destroy(splatterShadow);
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
            conversationManager.ShowConversation("Fire fist!", warriorHeroAI.heroFaceSprite);
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

        skill2CooldownTime = skill2MaxCooldownTime; // Reset cooldown

        isSkill2Active = true;
        skill2CooldownTime = skill2MaxCooldownTime; // Reset cooldown

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
