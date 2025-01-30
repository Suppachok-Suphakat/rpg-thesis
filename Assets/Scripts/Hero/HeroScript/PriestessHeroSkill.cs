using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PriestessHeroSkill : MonoBehaviour
{
    [Header("Hero Skill")]
    public GameObject skillHeal;
    [SerializeField] private GameObject skill1Prefab;
    [SerializeField] private GameObject skill1AreaPreview;
    //[SerializeField] private float skill1ActiveTime = 5f;
    [SerializeField] private GameObject skill2Prefab;
    [SerializeField] private GameObject skill2AreaPreview;
    [SerializeField] private float skill2ActiveTime = 5f;

    private GameObject skill1PreviewInstance;
    private GameObject skill2PreviewInstance;

    private bool isSkill1Active = false;
    private bool isSkill2Active = false;

    [Header("Skill Settings")]
    [SerializeField] float skill1CooldownTime;
    [SerializeField] float skill1MaxCooldownTime;
    [SerializeField] float skill2CooldownTime;
    [SerializeField] float skill2MaxCooldownTime;

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

    public PriestessHeroAI priestessHeroAI;

    private LineTrigger lineTrigger;

    public ConversationManager conversationManager;

    private void Awake()
    {
        lineTrigger = GameObject.Find("Player").GetComponent<LineTrigger>();
        priestessHeroAI = GetComponent<PriestessHeroAI>();
    }

    // Start is called before the first frame update
    void Start()
    {
        partnerSkillManager = GameObject.Find("PartnerCanvas").GetComponent<PartnerSkillManager>();
        //skill1Image.fillAmount = 0;
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

    public void SkillHealOn()
    {
        skillHeal.SetActive(true);
    }

    public void SkillHealOff()
    {
        skillHeal.SetActive(false);
    }

    private void HandleSkill2()
    {
        if (lineTrigger.currentTarget == this.transform)
        {
            if (Input.GetKeyDown(KeyCode.Q) && !isAnySkillActive)
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
                    if (skill2PreviewInstance != null)
                    {
                        skill2PreviewInstance.SetActive(false); // Hide the preview when the button is released
                        Destroy(skill2PreviewInstance); // Optionally destroy it after use
                        skill2PreviewInstance = null;
                    }

                    if (skill2CooldownTime <= 0) // Cooldown completed
                    {
                        GameObject skillInstance = Instantiate(skill2Prefab, GetMouseWorldPosition(), Quaternion.identity);
                        isSkill2Active = true;
                        skill2CooldownTime = skill2MaxCooldownTime; // Reset cooldown

                        Destroy(skillInstance, skill2ActiveTime);
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

    private void ActivateSkill(GameObject skillPrefab)
    {
        Vector3 mousePosition = GetMouseWorldPosition();
        Instantiate(skillPrefab, mousePosition, Quaternion.identity);
        Debug.Log("Skill Activated");
    }

    public void Skill1CooldownOverTime()
    {
        if (cooldownRecoveryTimer <= 0)
        {
            skill1CooldownTime += 1;
            cooldownRecoveryTimer = cooldownRecoveryDelay;
        }
        else
        {
            cooldownRecoveryTimer -= Time.deltaTime;
        }
        skill1CooldownTime = Mathf.Min(skill1CooldownTime, skill1MaxCooldownTime);
    }

    public void Skill2CooldownOverTime()
    {
        if (cooldownRecoveryTimer <= 0)
        {
            skill2CooldownTime += 1;
            cooldownRecoveryTimer = cooldownRecoveryDelay;
        }
        else
        {
            cooldownRecoveryTimer -= Time.deltaTime;
        }
        skill2CooldownTime = Mathf.Min(skill2CooldownTime, skill2MaxCooldownTime);
    }

    public void Skill1Activate()
    {
        Debug.Log("Partner Skill Activate");
        conversationManager.ShowConversation("Be restored in sacred light!", priestessHeroAI.heroFaceSprite);
        gameObject.GetComponent<Animator>().SetTrigger("skillHeal");

        skill1CooldownTime = 0;  // Start cooldown
        UpdateCooldownUI();  // Update UI
    }

    public void Skill2Activate()
    {
        Debug.Log("Partner Skill 2 Activate");
        conversationManager.ShowConversation("Feel the warmth of the divine!", priestessHeroAI.heroFaceSprite);
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
        PlayerController.instance.GetComponent<Animator>().SetTrigger("priestessLink");

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
        Debug.Log("Unlink1");
        // Trigger return to normal state
        PlayerController.instance.GetComponent<Animator>().SetTrigger("priestessUnlink");

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
        PlayerController.instance.GetComponent<Animator>().ResetTrigger("priestessUnlink");
    }

    private void UpdateCooldownUI()
    {
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
