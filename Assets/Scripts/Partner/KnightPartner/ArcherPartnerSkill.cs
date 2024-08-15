using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArcherPartnerSkill : MonoBehaviour
{
    [Header("Partner Skill")]
    [SerializeField] private GameObject skillPrefab;
    [SerializeField] int cooldownTime;
    [SerializeField] int maxCooldownTime;

    [SerializeField] int currentCooldownTime;

    [SerializeField] float cooldownRecoveryTimer = 1;
    [SerializeField] float cooldownRecoveryDelay = 0.1f;

    [SerializeField] float skillActiveTime;
    [SerializeField] float currentSkillActiveTime;
    [SerializeField] float fusionActiveTime;
    [SerializeField] float currentFusionActiveTime;

    [SerializeField] private StatusBar statusComponent;
    PartnerSkillManager partnerSkillManager;

    [Header("Skill Change")]
    [SerializeField] public ToolbarSlot toolbarSlot;
    [SerializeField] public WeaponInfo weaponInfo;
    [SerializeField] public Sprite itemSprite;

    WeaponInfo weaponChangeInfo;
    Sprite weaponChangeSprite;

    enum AbilityState
    {
        ready,
        active,
        cooldown
    }
    AbilityState state = AbilityState.cooldown;

    private void Awake()
    {

    }

    // Start is called before the first frame update
    void Start()
    {
        partnerSkillManager = GameObject.Find("PartnerCanvas").GetComponent<PartnerSkillManager>();

        currentCooldownTime = cooldownTime;
        statusComponent = partnerSkillManager.archerMPSliderObject.GetComponent<StatusBar>();
        partnerSkillManager.bubbleObject.SetActive(false);
        statusComponent.Set(cooldownTime, maxCooldownTime);

        toolbarSlot = GameObject.Find("RangeToolbar").GetComponent<ToolbarSlot>();
    }

    // Update is called once per frame
    void Update()
    {
        switch (state)
        {
            case AbilityState.ready:
                partnerSkillManager.bubbleObject.SetActive(true);
                PlayerController.instance.GetComponent<Animator>().ResetTrigger("ArcherFusionReturn");
                gameObject.GetComponent<Animator>().ResetTrigger("Skill");
                if (Input.GetKeyDown(KeyCode.Q))
                {
                    SkillActivate();
                    statusComponent.Set(0, maxCooldownTime);
                    state = AbilityState.active;
                    skillActiveTime = currentSkillActiveTime;
                }
                else if (Input.GetKeyDown(KeyCode.F))
                {
                    FusionActivate();
                    statusComponent.Set(0, maxCooldownTime);
                    state = AbilityState.active;
                    fusionActiveTime = currentFusionActiveTime;
                }
                break;
            case AbilityState.active:
                partnerSkillManager.bubbleObject.SetActive(false);
                if (skillActiveTime >= 0 || fusionActiveTime >= 0)
                {
                    skillActiveTime -= Time.deltaTime;
                    fusionActiveTime -= Time.deltaTime;
                }
                else
                {
                    PlayerController.instance.GetComponent<Animator>().SetTrigger("ArcherFusionReturn");
                    transform.parent.localScale = new Vector3(1f, 1f, 1f);

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

                    GameObject.Find("ActiveToolbar").GetComponent<ActiveToolbar>().ChangeActiveWeapon();

                    state = AbilityState.cooldown;
                    cooldownTime = currentCooldownTime;
                }
                break;
            case AbilityState.cooldown:
                if (cooldownTime <= maxCooldownTime)
                {
                    CooldownOverTime();
                    statusComponent.Set(cooldownTime, maxCooldownTime);
                }
                else
                {
                    statusComponent.Set(maxCooldownTime, maxCooldownTime);
                    state = AbilityState.ready;
                }
                break;
        }
    }

    public void CooldownOverTime()
    {
        if (cooldownRecoveryTimer <= 0)
        {
            cooldownTime += 1;
            cooldownRecoveryTimer = cooldownRecoveryDelay;
        }
        else
        {
            cooldownRecoveryTimer -= Time.deltaTime;
        }
    }

    public void SkillActivate()
    {
        Debug.Log("Partner Skill Activate");
        gameObject.GetComponent<Animator>().SetTrigger("Skill");

        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0f;
        Instantiate(skillPrefab, mousePosition, Quaternion.identity);

        if (toolbarSlot.weaponInfo != null)
        {
            weaponChangeInfo = toolbarSlot.weaponInfo;
        }
        weaponChangeSprite = toolbarSlot.slotSprite.GetComponent<Image>().sprite;
    }

    public void FusionActivate()
    {
        PlayerController.instance.GetComponent<Animator>().SetTrigger("ArcherFusion");
        transform.parent.localScale = new Vector3(0f, 0f, 0f);

        if (toolbarSlot.weaponInfo != null)
        {
            weaponChangeInfo = toolbarSlot.weaponInfo;
        }
        weaponChangeSprite = toolbarSlot.slotSprite.GetComponent<Image>().sprite;

        toolbarSlot.weaponInfo = weaponInfo;
        toolbarSlot.slotSprite.GetComponent<Image>().sprite = itemSprite;
        GameObject.Find("ActiveToolbar").GetComponent<ActiveToolbar>().ChangeActiveWeapon();
    }
}
