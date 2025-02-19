using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PhantomHeroSkill : MonoBehaviour
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
    public PhantomHeroAI phantomHeroAI;

    private bool isWaitingForAttack = false;
    private Transform targetEnemy; // The enemy to teleport to

    public ConversationManager conversationManager;

    private void Awake()
    {
        lineTrigger = GameObject.Find("Player").GetComponent<LineTrigger>();
        rb = GetComponent<Rigidbody2D>();
        phantomHeroAI = GetComponent<PhantomHeroAI>();
    }

    // Start is called before the first frame update
    void Start()
    {
        partnerSkillManager = PartnerSkillManager.Instance;
        //lineRenderer = GetComponent<LineRenderer>();
        //lineRenderer.positionCount = 2;
        //skill1Image.fillAmount = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (isSkill1Active)
        {

        }

        if (fusionActivated)
        {

        }

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
                    //Enter a shadow state and wait for player to attack and teleport to the enemy that player just attack
                    isAnySkillActive = true; //cannot use skill 2 or any skill
                }
                Skill1Activate(); //skill 1
                isAnySkillActive = false; // this mean can now use skill do this after attacked enemy
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
                        Skill2Activate();
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

    public void Skill1Activate()
    {
        if (isSkill1Active) return; // Prevent activation while skill is running

        isSkill1Active = true;
        isWaitingForAttack = true; // Start waiting for an attack
        skill1CooldownTime = skill1MaxCooldownTime;

        // Enter Shadow State (Lower opacity)
        Color heroColor = GetComponent<SpriteRenderer>().color;
        heroColor.a = 0.5f; // Lower opacity
        GetComponent<SpriteRenderer>().color = heroColor;

        // Show message
        conversationManager.ShowConversation("Waiting to strike...", phantomHeroAI.heroFaceSprite);

        // Start cooldown
        isSkill1Active = true;
        skill1CooldownTime = skill1MaxCooldownTime;
        UpdateCooldownUI(); // Update UI
    }

    // This should be called when the player attacks an enemy
    public void OnPlayerAttack(Transform enemy)
    {
        if (isWaitingForAttack && enemy != null)
        {
            isWaitingForAttack = false;
            targetEnemy = enemy;
            StartCoroutine(ExecuteSkill1());
        }
    }

    private IEnumerator ExecuteSkill1()
    {
        if (targetEnemy == null) yield break;

        // Teleport to enemy
        transform.position = targetEnemy.position;

        // Play skill animation
        GetComponent<Animator>().SetTrigger("skill1");

        // Wait for animation to complete
        yield return new WaitForSeconds(0.5f); // Adjust timing if necessary

        // Exit Shadow State (Restore opacity)
        Color heroColor = GetComponent<SpriteRenderer>().color;
        heroColor.a = 1f; // Restore full opacity
        GetComponent<SpriteRenderer>().color = heroColor;

        isSkill1Active = false;
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
        PlayerController.instance.GetComponent<Animator>().SetTrigger("phantomLink");

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
        PlayerController.instance.GetComponent<Animator>().SetTrigger("phantomUnlink");

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

        // Reset opacity (Exit Shadow State)
        Color heroColor = GetComponent<SpriteRenderer>().color;
        heroColor.a = 1f; // Restore full opacity
        GetComponent<SpriteRenderer>().color = heroColor;

        // Destroy skill preview if active
        if (skill1PreviewInstance != null)
        {
            Destroy(skill1PreviewInstance);
            skill1PreviewInstance = null;
        }

        StartCoroutine(ResetFusionTrigger());

        fusionActivated = false;
    }


    IEnumerator ResetFusionTrigger()
    {
        yield return new WaitForSeconds(0.1f);
        PlayerController.instance.GetComponent<Animator>().ResetTrigger("phantomUnlink");
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
