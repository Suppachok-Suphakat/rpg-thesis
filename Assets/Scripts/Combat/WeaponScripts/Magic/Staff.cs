using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Staff : MonoBehaviour, IWeapon
{
    [SerializeField] private WeaponInfo weaponInfo;
    [SerializeField] private GameObject magic;
    [SerializeField] private Transform magicLaserSpawnPoint;
    [SerializeField] private int manaCost;

    private Animator myAnimator;
    private Charecter charecter;

    readonly int ATTACK_HASH = Animator.StringToHash("Attack");

    [Header("Weapon Skill")]
    [SerializeField] private GameObject magicSkill;
    [SerializeField] float chargeTime;
    [SerializeField] float activeTime;
    [SerializeField] float currentChargeTime;
    [SerializeField] float currentActiveTime;

    public int chargeAmount;
    public int maxChargeAmount;

    [SerializeField] private GameObject sliderObject;
    [SerializeField] private SkillStatusBar statusComponent;

    enum SkillState
    {
        ready,
        active,
        charge
    }
    SkillState state = SkillState.charge;

    private void Awake()
    {
        myAnimator = GetComponent<Animator>();
        charecter = FindObjectOfType<Charecter>();
    }

    private void Start()
    {
        currentActiveTime = activeTime;

        sliderObject = GameObject.Find("MagicWeaponSkillBar");
        statusComponent = sliderObject.GetComponent<SkillStatusBar>();

        statusComponent.chargeAmount = 0;
        statusComponent.maxChargeAmount = this.maxChargeAmount;

        statusComponent.Set(this.chargeAmount, this.maxChargeAmount);
    }

    private void OnDestroy()
    {
        statusComponent.chargeAmount = 0;
        statusComponent.Set(statusComponent.chargeAmount, statusComponent.maxChargeAmount);
    }

    private void Update()
    {
        MouseFollowWithOffset();

        switch (state)
        {
            case SkillState.ready:
                if (Input.GetKeyDown(KeyCode.E))
                {
                    SkillActivate();
                    statusComponent.chargeAmount = 0;
                    statusComponent.Set(statusComponent.chargeAmount, maxChargeAmount);
                    state = SkillState.active;
                    activeTime = currentActiveTime;
                }
                break;
            case SkillState.active:
                if (activeTime > 0)
                {
                    activeTime -= Time.deltaTime;
                }
                else
                {
                    statusComponent.chargeAmount = 0;
                    state = SkillState.charge;
                    chargeTime = currentActiveTime;
                }
                break;
            case SkillState.charge:
                //animator.ResetTrigger("Skill");
                if (statusComponent.chargeAmount < this.chargeAmount)
                {
                    //Charging
                    Debug.Log("Charging");
                    statusComponent.Set(statusComponent.chargeAmount, maxChargeAmount);
                }
                else
                {
                    Debug.Log("MaxCharging");
                    statusComponent.Set(maxChargeAmount, maxChargeAmount);
                    state = SkillState.ready;
                }
                break;
        }
    }


    public void Attack()
    {
        if (charecter.mana.currVal >= manaCost)
        {
            myAnimator.SetTrigger(ATTACK_HASH);
            StartCoroutine(ReduceManaRoutine());
        }
    }

    public void SkillActivate()
    {
        Debug.Log("Magic Skill");
        SpawnStaffSkillAnimEvent();
    }

    private IEnumerator ReduceManaRoutine()
    {
        charecter.ReduceMana(manaCost);
        yield return new WaitForSeconds(weaponInfo.weaponCooldown);
    }

    public void SpawnStaffProjectileAnimEvent()
    {
        GameObject newLaser = Instantiate(magic, magicLaserSpawnPoint.position, Quaternion.identity);
        newLaser.GetComponent<MagicLaser>().UpdateLaserRange(weaponInfo.weaponRange);
    }

    public void SpawnStaffMagicAnimEvent()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0f;
        Instantiate(magic, mousePosition, Quaternion.identity);
    }

    public void SpawnStaffSkillAnimEvent()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0f;
        Instantiate(magicSkill, mousePosition, Quaternion.identity);
    }

    public WeaponInfo GetWeaponInfo()
    {
        return weaponInfo;
    }

    private void MouseFollowWithOffset()
    {
        Vector3 mousePos = Input.mousePosition;
        Vector3 playerScreenPoint = Camera.main.WorldToScreenPoint(PlayerController.instance.transform.position);

        float angle = Mathf.Atan2(mousePos.y, mousePos.x) * Mathf.Rad2Deg;

        if (mousePos.x < playerScreenPoint.x)
        {
            ActiveWeapon.Instance.transform.rotation = Quaternion.Euler(0, -180, angle);
        }
        else
        {
            ActiveWeapon.Instance.transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }
}
