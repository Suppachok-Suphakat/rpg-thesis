using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KnightHeroSkill : MonoBehaviour
{
    [Header("Hero Skill")]
    public GameObject skill1Damage;
    [SerializeField] private GameObject skill2Prefab;
    [SerializeField] private GameObject skill2AreaPreview;
    private GameObject skill2PreviewInstance;

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
    public KnightHeroAI knightHeroAI;

    public ConversationManager conversationManager;

    private void Awake()
    {
        lineTrigger = GameObject.Find("Player").GetComponent<LineTrigger>();
        rb = GetComponent<Rigidbody2D>();
        knightHeroAI = GetComponent<KnightHeroAI>();
    }

    // Start is called before the first frame update
    void Start()
    {
        partnerSkillManager = PartnerSkillManager.Instance;
        //skill1Image.fillAmount = 0;
    }

    // Update is called once per frame
    void Update()
    {
        HandleSkill1();
        HandleSkill2();
        UpdateCooldownUI();
    }

    private void HandleSkill1()
    {
        if (lineTrigger.currentTarget == this.transform)
        {
            if (Input.GetKeyDown(KeyCode.E) && !isAnySkillActive) // Block if another skill is active
            {
                if (skill1CooldownTime <= 0 && !isSkill1Active) // Cooldown completed
                {
                    isAnySkillActive = true; // Lock activation for other skills
                }

                if (skill1CooldownTime <= 0) // Cooldown completed
                {
                    Skill1Activate();
                    isSkill1Active = true;
                    skill1CooldownTime = skill1MaxCooldownTime; // Reset cooldown
                }

                isAnySkillActive = false; // Release lock after using the skill
            }
        }

        // Update cooldown over time
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
        Debug.Log("Partner Skill Activate");
        conversationManager.ShowConversation("I’ll clear a path!", knightHeroAI.heroFaceSprite);
        gameObject.GetComponent<Animator>().SetTrigger("skill2");

        skill2CooldownTime = 0;  // Start cooldown
        UpdateCooldownUI();  // Update UI
    }

    public void Skill2Activate()
    {
        Debug.Log("Partner Skill Activate");
        conversationManager.ShowConversation("Shield up!", knightHeroAI.heroFaceSprite);
        gameObject.GetComponent<Animator>().SetTrigger("skill");
        //knightHeroAI.SkillLogic();

        skill1CooldownTime = 0;  // Start cooldown
        UpdateCooldownUI();  // Update UI
    }

    public void FusionActivate()
    {
        PlayerController.instance.GetComponent<Animator>().SetTrigger("KnightFusion");

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
        PlayerController.instance.GetComponent<Animator>().SetTrigger("KnightFusionReturn");

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
