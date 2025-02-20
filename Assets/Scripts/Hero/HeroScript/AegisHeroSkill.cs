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
    [SerializeField] public GameObject weaponInstance;
    [SerializeField] public GameObject droneInstance;
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
                    weaponInstance.GetComponent<EXSkillFollowMouse>().WeaponSkillActivate();
                    conversationManager.ShowConversation("Thunder strike", aegisHeroAI.heroFaceSprite);
                    gameObject.GetComponent<Animator>().SetTrigger("skill1");
                    isAnySkillActive = true; // Lock activation for other skills
                }


                if (skill1CooldownTime <= 0) // Cooldown completed
                {
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
                    weaponInstance.GetComponent<EXSkillFollowMouse>().TriggerTurret();
                    isSkill2Active = true;
                    skill2CooldownTime = skill2MaxCooldownTime; // Reset cooldown
                }

                conversationManager.ShowConversation("Turret deploy", aegisHeroAI.heroFaceSprite);
                gameObject.GetComponent<Animator>().SetTrigger("skill2");

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

    public void FusionActivate()
    {
        PlayerController.instance.GetComponent<Animator>().SetTrigger("aegisLink");

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
        PlayerController.instance.GetComponent<Animator>().SetTrigger("aegisUnlink");

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

        fusionActivated = false;
    }

    IEnumerator ResetFusionTrigger()
    {
        yield return new WaitForSeconds(0.1f);
        PlayerController.instance.GetComponent<Animator>().ResetTrigger("aegisUnlink");
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
