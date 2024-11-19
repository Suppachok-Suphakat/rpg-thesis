using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArcherHeroSkill : MonoBehaviour
{
    [Header("Partner Skill")]
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
    [SerializeField] float cooldownRecoveryTimer = 1;
    [SerializeField] float cooldownRecoveryDelay = 0.1f;

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

    public ArcherHeroAI archerHeroAI;

    private LineTrigger lineTrigger;

    public ConversationManager conversationManager;

    enum AbilityState
    {
        ready,
        active1,
        active2,
        fusion,
        cooldown
    }
    AbilityState state = AbilityState.cooldown;

    enum SkillState
    {
        Ready,
        Active,
        Cooldown
    }
    SkillState skill1State = SkillState.Cooldown;
    SkillState skill2State = SkillState.Cooldown;

    private void Awake()
    {
        lineTrigger = GameObject.Find("Player").GetComponent<LineTrigger>();
        archerHeroAI = GetComponent<ArcherHeroAI>();
    }

    // Start is called before the first frame update
    void Start()
    {
        partnerSkillManager = GameObject.Find("PartnerCanvas").GetComponent<PartnerSkillManager>();

        //skill1Image.fillAmount = 0;
        //skill2Image.fillAmount = 0;
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
        if (Input.GetKey(KeyCode.Q))
        {
            if (skill1CooldownTime <= 0 && !isSkill1Active) // Cooldown completed
            {
                if (skill1PreviewInstance == null)
                {
                    skill1PreviewInstance = Instantiate(skill1AreaPreview);
                }
                skill1PreviewInstance.SetActive(true);
                skill1PreviewInstance.transform.position = GetMouseWorldPosition(); // Follow the mouse position
            }
        }

        if (Input.GetKeyUp(KeyCode.Q))
        {
            if (skill1PreviewInstance != null)
            {
                skill1PreviewInstance.SetActive(false); // Hide the preview when the button is released
                Destroy(skill1PreviewInstance); // Optionally destroy it after use
                skill1PreviewInstance = null;
            }

            if (skill1CooldownTime <= 0) // Cooldown completed
            {
                ActivateSkill(skillPrefab);
                isSkill1Active = true;
                skill1CooldownTime = skill1MaxCooldownTime; // Reset cooldown
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
        if (Input.GetKey(KeyCode.E))
        {
            if (skill2CooldownTime <= 0 && !isSkill2Active) // Cooldown completed
            {
                if (skill2PreviewInstance == null)
                {
                    skill2PreviewInstance = Instantiate(skill2AreaPreview);
                }
                skill2PreviewInstance.SetActive(true);
                skill2PreviewInstance.transform.position = GetMouseWorldPosition(); // Follow the mouse position
            }
        }

        if (Input.GetKeyUp(KeyCode.E))
        {
            if (skill2PreviewInstance != null)
            {
                skill2PreviewInstance.SetActive(false); // Hide the preview when the button is released
                Destroy(skill2PreviewInstance); // Optionally destroy it after use
                skill2PreviewInstance = null;
            }

            if (skill2CooldownTime <= 0) // Cooldown completed
            {
                ActivateSkill(skill2Prefab);
                isSkill2Active = true;
                skill2CooldownTime = skill2MaxCooldownTime; // Reset cooldown
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
        conversationManager.ShowConversation("Nature strikes with me!", archerHeroAI.heroFaceSprite);
        gameObject.GetComponent<Animator>().SetTrigger("skill");

        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0f;
        Instantiate(skillPrefab, mousePosition, Quaternion.identity);

        //if (toolbarSlot.weaponInfo != null)
        //{
        //    weaponChangeInfo = toolbarSlot.weaponInfo;
        //}
        //weaponChangeSprite = toolbarSlot.slotSprite.GetComponent<Image>().sprite;

        skill1CooldownTime = 0;  // Start cooldown
        UpdateCooldownUI();  // Update UI
    }

    public void Skill2Activate()
    {
        Debug.Log("Partner Skill 2 Activate");
        conversationManager.ShowConversation("Caught you in my sights!", archerHeroAI.heroFaceSprite);
        gameObject.GetComponent<Animator>().SetTrigger("skill");

        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0f;
        Instantiate(skill2Prefab, mousePosition, Quaternion.identity);

        //if (toolbarSlot.weaponInfo != null)
        //{
        //    weaponChangeInfo = toolbarSlot.weaponInfo;
        //}
        //weaponChangeSprite = toolbarSlot.slotSprite.GetComponent<Image>().sprite;

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
        PlayerController.instance.GetComponent<Animator>().SetTrigger("ArcherFusionReturn");

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
        PlayerController.instance.GetComponent<Animator>().ResetTrigger("ArcherFusionReturn");
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
