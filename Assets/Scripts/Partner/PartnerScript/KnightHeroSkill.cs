using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KnightHeroSkill : MonoBehaviour
{
    [Header("Hero Skill")]
    [SerializeField] private GameObject barrier;
    public GameObject barrierCircleInstance;
    public GameObject skillDamage;

    [Header("Skill 1")]
    [SerializeField] int skill1CooldownTime;
    [SerializeField] int skill1MaxCooldownTime;
    [SerializeField] int skill1CurrentCooldownTime;
    [SerializeField] float skill1ActiveTime;
    [SerializeField] float currentSkill1ActiveTime;

    [Header("Skill 2")]
    [SerializeField] int skill2CooldownTime;
    [SerializeField] int skill2MaxCooldownTime;
    [SerializeField] int skill2CurrentCooldownTime;
    [SerializeField] float skill2ActiveTime;
    [SerializeField] float currentSkill2ActiveTime;

    private bool fusionActivated = false;

    [SerializeField] float cooldownRecoveryTimer = 1;
    [SerializeField] float cooldownRecoveryDelay = 0.1f;

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

    enum AbilityState
    {
        ready,
        active1,
        active2,
        fusion,
        cooldown
    }
    AbilityState state = AbilityState.cooldown;

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

        // Ensure these components are correctly set through the manager
        if (partnerSkillManager != null)
        {
            foreach (var partner in partnerSkillManager.partners)
            {
                if (partner.partnerObject == gameObject)
                {
                    statusComponent = partner.statusComponent;
                    statusComponent.gameObject.SetActive(false); // Ensure it's inactive initially

                    if (partner.skillBubble != null)
                    {
                        partner.skillBubble.SetActive(false); // Ensure it's inactive initially
                    }

                    break;
                }
            }
        }

        skill1CurrentCooldownTime = skill1CooldownTime;
        skill2CurrentCooldownTime = skill2CooldownTime;

        skill1Image.fillAmount = 0;
    }

    // Update is called once per frame
    void Update()
    {
        switch (state)
        {
            case AbilityState.ready:
                if (partnerSkillManager != null)
                {
                    foreach (var partner in partnerSkillManager.partners)
                    {
                        if (partner.partnerObject == gameObject)
                        {
                            partner.skillBubble.SetActive(true);
                            break;
                        }
                    }
                }
                gameObject.GetComponent<Animator>().ResetTrigger("skill");
                gameObject.GetComponent<Animator>().ResetTrigger("skill2");
                if (lineTrigger.currentTarget == this.transform)
                {
                    if (Input.GetKeyDown(KeyCode.Q))
                    {
                        SkillActivate();
                        state = AbilityState.active1;
                        skill1ActiveTime = currentSkill1ActiveTime;
                    }
                    else if (Input.GetKeyDown(KeyCode.E))
                    {
                        Skill2Activate();
                        statusComponent.Set(0, skill2MaxCooldownTime);
                        state = AbilityState.active2;
                        skill2ActiveTime = currentSkill2ActiveTime;
                    }
                }
                break;
            case AbilityState.active1:
                if (partnerSkillManager != null)
                {
                    foreach (var partner in partnerSkillManager.partners)
                    {
                        if (partner.partnerObject == gameObject)
                        {
                            partner.skillBubble.SetActive(false);
                            break;
                        }
                    }
                }

                if (skill1ActiveTime >= 0)
                {
                    skill1ActiveTime -= Time.deltaTime;
                }
                else
                {
                    if(barrierCircleInstance != null)
                    {
                        knightHeroAI.isUsingSkill = false;
                        knightHeroAI.currentState = KnightHeroAI.State.defence;
                        DestroyMagicCircle();
                    }


                    transform.parent.localScale = new Vector3(1f, 1f, 1f);

                    //if (weaponChangeInfo != null)
                    //{
                    //    toolbarSlot.weaponInfo = weaponChangeInfo;
                    //    toolbarSlot.slotSprite.GetComponent<Image>().sprite = weaponChangeSprite;
                    //}
                    //else
                    //{
                    //    toolbarSlot.weaponInfo = null;
                    //    toolbarSlot.slotSprite.GetComponent<Image>().sprite = weaponChangeSprite;
                    //}

                    //GameObject.Find("ActiveToolbar").GetComponent<ActiveToolbar>().ChangeActiveWeapon();

                    state = AbilityState.cooldown;
                    skill1CooldownTime = skill1CurrentCooldownTime;
                }
                break;
            case AbilityState.active2:
                if (skill2ActiveTime >= 0)
                {
                    skill2ActiveTime -= Time.deltaTime;
                }
                else
                {
                    //// Reset the weapon if necessary
                    //if (weaponChangeInfo != null)
                    //{
                    //    toolbarSlot.weaponInfo = weaponChangeInfo;
                    //    toolbarSlot.slotSprite.GetComponent<Image>().sprite = weaponChangeSprite;
                    //}
                    //else
                    //{
                    //    toolbarSlot.weaponInfo = null;
                    //    toolbarSlot.slotSprite.GetComponent<Image>().sprite = weaponChangeSprite;
                    //}

                    //GameObject.Find("ActiveToolbar").GetComponent<ActiveToolbar>().ChangeActiveWeapon();

                    state = AbilityState.cooldown;
                    skill2CooldownTime = skill2CurrentCooldownTime; // Reset skill2CooldownTime
                }
                break;
            case AbilityState.fusion:
                if (partnerSkillManager != null)
                {
                    foreach (var partner in partnerSkillManager.partners)
                    {
                        if (partner.partnerObject == gameObject)
                        {
                            partner.skillBubble.SetActive(false);
                            break;
                        }
                    }
                }
                break;
            case AbilityState.cooldown:
                if (skill1CooldownTime < skill1MaxCooldownTime)
                {
                    Skill1CooldownOverTime();
                }
                if (skill2CooldownTime < skill2MaxCooldownTime)
                {
                    Skill2CooldownOverTime();
                }
                if (skill1CooldownTime >= skill1MaxCooldownTime &&
                    skill2CooldownTime >= skill2MaxCooldownTime)
                {
                    state = AbilityState.ready;
                }
                UpdateCooldownUI(); // Update the UI for all skills
                break;
        }
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

    public void SkillActivate()
    {
        Debug.Log("Partner Skill Activate");
        gameObject.GetComponent<Animator>().SetTrigger("skill");
        knightHeroAI.SkillLogic();

        //if (toolbarSlot.weaponInfo != null)
        //{
        //    weaponChangeInfo = toolbarSlot.weaponInfo;
        //}
        //weaponChangeSprite = toolbarSlot.slotSprite.GetComponent<Image>().sprite;

        skill1CooldownTime = 0;  // Start cooldown
        UpdateCooldownUI();  // Update UI

        if (partnerSkillManager != null)
        {
            foreach (var partner in partnerSkillManager.partners)
            {
                if (partner.partnerObject == gameObject)
                {
                    partner.statusComponent.gameObject.SetActive(true);
                    partner.skillBubble.SetActive(true);
                    break;
                }
            }
        }
    }

    public void Skill2Activate()
    {
        Debug.Log("Partner Skill Activate");
        gameObject.GetComponent<Animator>().SetTrigger("skill2");

        //if (toolbarSlot.weaponInfo != null)
        //{
        //    weaponChangeInfo = toolbarSlot.weaponInfo;
        //}
        //weaponChangeSprite = toolbarSlot.slotSprite.GetComponent<Image>().sprite;

        skill2CooldownTime = 0;  // Start cooldown
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

    public void SpawnMagicCircle()
    {
        barrierCircleInstance = Instantiate(barrier, transform.position, Quaternion.identity);
    }

    public void UpdateMagicCirclePosition()
    {
        if (barrierCircleInstance != null)
        {
            Vector3 partnerPosition = transform.position;
            partnerPosition.z = 0f;

            barrierCircleInstance.transform.position = partnerPosition;
        }
    }

    public void DestroyMagicCircle()
    {
        Debug.Log("Destroy");
        Destroy(barrierCircleInstance);
    }

    public void OnSkillDamage()
    {
        skillDamage.SetActive(true);
    }

    public void OffSkillDamage()
    {
        skillDamage.SetActive(false);
    }

    private void UpdateCooldownUI()
    {
        // Update Skill 1 UI
        if (skill1CooldownTime < skill1MaxCooldownTime)
        {
            skill1Image.fillAmount = skill1CooldownTime / (float)skill1MaxCooldownTime;
        }
        else
        {
            skill1Image.fillAmount = 1;
        }

        // Update Skill 2 UI
        if (skill2CooldownTime < skill2MaxCooldownTime)
        {
            skill2Image.fillAmount = skill2CooldownTime / (float)skill2MaxCooldownTime;
        }
        else
        {
            skill2Image.fillAmount = 1;
        }
    }
}
