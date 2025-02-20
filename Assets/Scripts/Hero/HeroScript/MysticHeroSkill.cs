using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MysticHeroSkill : MonoBehaviour
{
    [Header("Hero Skill")]
    public GameObject skill1Damage;
    public GameObject skill1Prefab;
    private GameObject skill1PreviewInstance;
    public float skill2Range;
    public LineRenderer lineRenderer;
    public GameObject skill2Prefab;
    public GameObject skill2Preview;
    public GameObject skill2AreaPreview;
    private GameObject skill2PreviewInstance;
    public GameObject skill1Collider;
    public GameObject skill2Collider;

    private bool isSkill2PreviewActive = false;
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

    public ConversationManager conversationManager;

    private void Awake()
    {
        lineTrigger = GameObject.Find("Player").GetComponent<LineTrigger>();
        rb = GetComponent<Rigidbody2D>();
        mysticHeroAI = GetComponent<MysticHeroAI>();
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
        if (isSkill1Active)
        {

        }

        if (fusionActivated)
        {

        }

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
                    //Enter a shadow state and wait for player to attack and teleport to the enemy that player just attack
                    Skill1Activate(); //skill 1
                    isReady = true;
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
        isWaitingForAttack = true;

        Color heroColor = GetComponent<SpriteRenderer>().color;
        heroColor.a = 0.5f;
        GetComponent<SpriteRenderer>().color = heroColor;

        conversationManager.ShowConversation("Waiting to strike...", mysticHeroAI.heroFaceSprite);
        UpdateCooldownUI();
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

        transform.position = targetEnemy.position;
        GetComponent<Animator>().SetTrigger("skill1");
        skill1CooldownTime = skill1MaxCooldownTime;
        skill1Collider.SetActive(true); // Activate skill1 collider

        if (mysticHeroAI != null)
        {
            mysticHeroAI.focusEnemy = targetEnemy;
            mysticHeroAI.enemyTransform = targetEnemy;
        }

        yield return new WaitForSeconds(0.5f);

        Color heroColor = GetComponent<SpriteRenderer>().color;
        heroColor.a = 1f;
        GetComponent<SpriteRenderer>().color = heroColor;

        isSkill1Active = false;
        isAnySkillActive = false;
        isReady = false;
        skill1Collider.SetActive(false); // Deactivate skill1 collider
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
        if (isSkill2Active) return;

        Debug.Log("Partner Skill Activate");

        skill2CooldownTime = skill2MaxCooldownTime;
        isSkill2Active = true;
        skill2Collider.SetActive(true); // Activate skill2 collider

        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;
        Vector3 direction = (mousePosition - transform.position).normalized;

        GetComponent<Animator>().SetTrigger("skill2");
        StartCoroutine(ChargeForward(direction));
        UpdateCooldownUI();
    }

    private IEnumerator ChargeForward(Vector3 direction)
    {
        float chargeTime = 0.3f;
        float elapsedTime = 0f;
        float chargeSpeed = 15f;

        while (elapsedTime < chargeTime)
        {
            transform.position += direction * chargeSpeed * Time.deltaTime;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        isSkill2Active = false;
        skill2Collider.SetActive(false); // Deactivate skill2 collider
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

        fusionActivated = false;
    }


    IEnumerator ResetFusionTrigger()
    {
        yield return new WaitForSeconds(0.1f);
        PlayerController.instance.GetComponent<Animator>().ResetTrigger("mysticUnlink");
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
