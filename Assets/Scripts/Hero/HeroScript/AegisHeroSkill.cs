using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AegisHeroSkill : MonoBehaviour
{
    [Header("Hero Skill")]
    [SerializeField] private GameObject skillPrefab;
    [SerializeField] private GameObject skill1AreaPreview;
    [SerializeField] private GameObject skill2Prefab;
    [SerializeField] private GameObject skill2AreaPreview;
    private GameObject skill1PreviewInstance;
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
    //[SerializeField] float cooldownRecoveryTimer = 1;
    //[SerializeField] float cooldownRecoveryDelay = 0.1f;
    private bool isAnySkillActive = false;

    private bool fusionActivated = false;

    [SerializeField] private StatusBar statusComponent;
    PartnerSkillManager partnerSkillManager;

    [Header("Skill Change")]
    [SerializeField] public ToolbarSlot toolbarSlot;
    [SerializeField] public WeaponInfo weaponInfo;
    [SerializeField] private GameObject weaponGO;
    [SerializeField] private GameObject weaponInstance;
    [SerializeField] private GameObject droneInstance;
    [SerializeField] private GameObject droneGO;
    [SerializeField] private Transform dronePoint;
    [SerializeField] public Sprite itemSprite;

    WeaponInfo weaponChangeInfo;
    Sprite weaponChangeSprite;
    public ActiveToolbar activeToolbar;

    [SerializeField] public Image skill1Image;
    [SerializeField] public Image skill2Image;

    public AegisHeroAI aegisHeroAI;

    private LineTrigger lineTrigger;

    public ConversationManager conversationManager;

    private void Awake()
    {
        lineTrigger = GameObject.Find("Player").GetComponent<LineTrigger>();
        aegisHeroAI = GetComponent<AegisHeroAI>();
    }

    // Start is called before the first frame update
    void Start()
    {
        partnerSkillManager = GameObject.Find("PartnerCanvas").GetComponent<PartnerSkillManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (fusionActivated)
        {

        }

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
                    //if (skill1PreviewInstance == null)
                    //{
                    //    skill1PreviewInstance = Instantiate(skill1AreaPreview);
                    //}
                    //skill1PreviewInstance.SetActive(true);

                    weaponInstance.GetComponent<EXSkillFollowMouse>().WeaponSkillActivate();
                    isAnySkillActive = true; // Lock activation for other skills
                }


                if (skill1CooldownTime <= 0) // Cooldown completed
                {
                    //GameObject skillInstance = Instantiate(skill2Prefab, GetMouseWorldPosition(), Quaternion.identity);
                    //ActivateSkill1(skillInstance);
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
            }

            if (Input.GetMouseButtonDown(0) && skill2PreviewInstance != null)
            {
                skill2PreviewInstance.SetActive(false); // Hide the preview
                Destroy(skill2PreviewInstance); // Optionally destroy it
                skill2PreviewInstance = null;

                if (skill2CooldownTime <= 0) // Cooldown completed
                {
                    ActivateSkill(skillPrefab);
                    isSkill2Active = true;
                    skill2CooldownTime = skill2MaxCooldownTime; // Reset cooldown
                }

                isAnySkillActive = false; // Release lock after using the skill
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

    private void ShowSkillPreview(GameObject skillPreview)
    {
        skillPreview.SetActive(true);
        // Optionally update the preview's position based on the mouse or player aim.
        skillPreview.transform.position = GetMouseWorldPosition();
    }

    private void HideSkillPreview(GameObject skillPreview)
    {
        skillPreview.SetActive(false);
    }

    private void ActivateSkill(GameObject skillPrefab)
    {
        Vector3 mousePosition = GetMouseWorldPosition();
        Instantiate(skillPrefab, mousePosition, Quaternion.identity);
        Debug.Log("Skill Activated");
    }

    private void SkillCooldown(ref float currentCooldown, float maxCooldown, ref bool isActive)
    {
        if (currentCooldown < maxCooldown)
        {
            currentCooldown += Time.deltaTime; // Smooth increment
        }
        else
        {
            isActive = false;
        }
    }

    public void SkillActivate()
    {
        Debug.Log("Partner Skill Activate");
        conversationManager.ShowConversation("Nature strikes with me!", aegisHeroAI.heroFaceSprite);
        gameObject.GetComponent<Animator>().SetTrigger("skill");

        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0f;
        Instantiate(skillPrefab, mousePosition, Quaternion.identity);

        skill1CooldownTime = 0;  // Start cooldown
        UpdateCooldownUI();  // Update UI
    }

    public void Skill2Activate()
    {
        Debug.Log("Partner Skill 2 Activate");
        conversationManager.ShowConversation("Caught you in my sights!", aegisHeroAI.heroFaceSprite);
        gameObject.GetComponent<Animator>().SetTrigger("skill");

        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0f;
        Instantiate(skill2Prefab, mousePosition, Quaternion.identity);

        skill2CooldownTime = 0;  // Start cooldown for Skill 2
        UpdateCooldownUI();  // Update UI
    }

    public void FusionActivate()
    {
        PlayerController.instance.GetComponent<Animator>().SetTrigger("ArcherFusion");

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

        droneInstance.SetActive(false);

        weaponInstance = Instantiate(weaponGO, dronePoint.position, Quaternion.identity);

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
        PlayerController.instance.GetComponent<Animator>().SetTrigger("ArcherFusionReturn");

        if (weaponInstance != null)
        {
            StartCoroutine(ReturnWeaponToDronePoint());
        }

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

        StartCoroutine(ResetFusionTrigger());

        fusionActivated = false;
    }

    IEnumerator ResetFusionTrigger()
    {
        yield return new WaitForSeconds(0.1f);
        PlayerController.instance.GetComponent<Animator>().ResetTrigger("ArcherFusionReturn");
    }


    private IEnumerator ReturnWeaponToDronePoint()
    {
        float duration = 0.5f; // Adjust speed as needed
        float elapsedTime = 0f;
        Vector3 startPosition = weaponInstance.transform.position;
        Vector3 targetPosition = dronePoint.position;

        while (elapsedTime < duration)
        {
            weaponInstance.transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure it reaches the exact position
        weaponInstance.transform.position = targetPosition;
        weaponInstance.transform.rotation = Quaternion.identity;

        // Parent it to the dronePoint to stop unintended movement
        weaponInstance.transform.SetParent(dronePoint);

        Destroy(weaponInstance);

        droneInstance.SetActive(true);
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
        mousePosition.z = 0;
        return mousePosition;
    }
}
