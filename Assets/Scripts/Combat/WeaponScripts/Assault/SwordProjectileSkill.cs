using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordProjectileSkill : MonoBehaviour, IWeapon
{
    [SerializeField] private GameObject slashAnimPrefab;
    [SerializeField] private Transform slashAnimSpawnPoint;
    //[SerializeField] private float swordAttackCD = .5f;
    [SerializeField] private WeaponInfo weaponInfo;
    [SerializeField] private int staminaCost;

    [SerializeField] private int damageAmount;

    [SerializeField] private Transform weaponCollider;
    private Animator animator;
    private Charecter charecter;

    private GameObject slashAnim;

    [Header("Weapon Ability")]
    [SerializeField] private GameObject swordProjectileSkillPrefab;
    [SerializeField] private Transform skillSpawnPoint;
    [SerializeField] private GameObject swordAbilityAnimPrefab;
    [SerializeField] private Transform swordAbilityAnimSpawnPoint;
    [SerializeField] private GameObject swordAbilityAnim;
    [SerializeField] float chargeTime;
    [SerializeField] float activeTime;
    [SerializeField] float currentChargeTime;
    [SerializeField] float currentActiveTime;

    public int chargeAmount;
    public int maxChargeAmount;

    [SerializeField] private GameObject sliderObject;
    [SerializeField] private StatusBar statusComponent;

    enum AbilityState
    {
        ready,
        active,
        charge
    }
    AbilityState state = AbilityState.charge;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        charecter = FindObjectOfType<Charecter>();
    }

    private void Start()
    {
        weaponCollider.gameObject.GetComponent<SwordDamageSouce>().damageAmount = this.damageAmount;
        slashAnimSpawnPoint = GameObject.Find("SlashSpawnPoint").transform;
        currentActiveTime = activeTime;

        sliderObject = GameObject.Find("MeleeWeaponSkillBar");
        statusComponent = sliderObject.GetComponent<StatusBar>();

        statusComponent.Set(weaponCollider.gameObject.GetComponent<SwordDamageSouce>().chargeAmount, maxChargeAmount);
    }

    private void OnDestroy()
    {
        weaponCollider.gameObject.GetComponent<SwordDamageSouce>().chargeAmount = 0;
        statusComponent.Set(weaponCollider.gameObject.GetComponent<SwordDamageSouce>().chargeAmount, maxChargeAmount);
    }

    private void Update()
    {
        MouseFollowWithOffset();

        switch (state)
        {
            case AbilityState.ready:
                if (Input.GetKeyDown(KeyCode.E))
                {
                    SkillActivate();
                    statusComponent.Set(weaponCollider.gameObject.GetComponent<SwordDamageSouce>().chargeAmount, maxChargeAmount);
                    state = AbilityState.active;
                    activeTime = currentActiveTime;
                }
                break;
            case AbilityState.active:
                if (activeTime > 0)
                {
                    weaponCollider.gameObject.GetComponent<SwordDamageSouce>().chargeAmount = 0;
                    activeTime -= Time.deltaTime;
                }
                else
                {
                    state = AbilityState.charge;
                    chargeTime = currentActiveTime;
                }
                break;
            case AbilityState.charge:
                animator.ResetTrigger("Skill");
                if (weaponCollider.gameObject.GetComponent<SwordDamageSouce>().chargeAmount < this.chargeAmount)
                {
                    statusComponent.Set(weaponCollider.gameObject.GetComponent<SwordDamageSouce>().chargeAmount, maxChargeAmount);
                    //Charging
                }
                else
                {
                    weaponCollider.gameObject.GetComponent<SwordDamageSouce>().chargeAmount = 0;
                    weaponCollider.gameObject.SetActive(false);
                    statusComponent.Set(maxChargeAmount, maxChargeAmount);
                    state = AbilityState.ready;
                }
                break;
        }
    }

    public WeaponInfo GetWeaponInfo()
    {
        return weaponInfo;
    }

    public void Attack()
    {
        if (charecter.stamina.currVal >= staminaCost && !PlayerController.instance.isMenuActive)
        {
            animator.SetTrigger("Attack");
            weaponCollider.gameObject.SetActive(true);
            slashAnim = Instantiate(slashAnimPrefab, slashAnimSpawnPoint.position, Quaternion.identity);
            slashAnim.transform.parent = this.transform.parent;
            weaponCollider.gameObject.GetComponent<DamageSouce>();
            StartCoroutine(ReduceStaminaRoutine());
        }
    }

    public void SkillActivate()
    {
        animator.SetTrigger("Attack");
        GameObject newArrow = Instantiate(swordProjectileSkillPrefab, skillSpawnPoint.position,
            ActiveWeapon.Instance.transform.rotation);
        newArrow.GetComponent<SkillProjectile>().UpdateProjectileRange(weaponInfo.weaponRange);
    }

    private IEnumerator ReduceStaminaRoutine()
    {
        charecter.ReduceStamina(staminaCost);
        yield return new WaitForSeconds(weaponInfo.weaponCooldown);
    }

    private IEnumerator SkillRoutine()
    {
        yield return new WaitForSeconds(5);
    }

    public void DoneAttackingAnimEvent()
    {
        weaponCollider.gameObject.SetActive(false);
    }

    public void SwingUpFlipAnimEvent()
    {
        slashAnim.gameObject.transform.rotation = Quaternion.Euler(-180, 0, 0);

        if (PlayerController.instance.FacingLeft)
        {
            slashAnim.GetComponent<SpriteRenderer>().flipX = true;
            slashAnim.GetComponent<SpriteRenderer>().flipY = true;
        }
    }

    public void SwingDownFlipAnimEvent()
    {
        slashAnim.gameObject.transform.rotation = Quaternion.Euler(0, 0, 0);

        if (PlayerController.instance.FacingLeft)
        {
            slashAnim.GetComponent<SpriteRenderer>().flipX = true;
            slashAnim.GetComponent<SpriteRenderer>().flipY = true;
        }
    }

    private void MouseFollowWithOffset()
    {
        Vector3 mousePos = Input.mousePosition;
        Vector3 playerScreenPoint = Camera.main.WorldToScreenPoint(PlayerController.instance.transform.position);

        float angle = Mathf.Atan2(mousePos.y, mousePos.x) * Mathf.Rad2Deg;

        if (mousePos.x < playerScreenPoint.x)
        {
            //ActiveWeapon.Instance.transform.rotation = Quaternion.Euler(0, -180, angle);
            //weaponCollider.transform.rotation = Quaternion.Euler(0, -180, 0);
        }
        else
        {
            //ActiveWeapon.Instance.transform.rotation = Quaternion.Euler(0, 0, angle);
            //weaponCollider.transform.rotation = Quaternion.Euler(0, 0, 0);
        }
    }
}
