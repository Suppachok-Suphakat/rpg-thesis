using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spear : MonoBehaviour, IWeapon
{
    [SerializeField] private WeaponInfo weaponInfo;
    [SerializeField] private int staminaCost;

    [SerializeField] private int damageAmount;

    [SerializeField] private Transform weaponCollider;
    private Animator animator;
    private Charecter charecter;
    private SpriteRenderer spriteRenderer;

    //private GameObject slashAnim;

    [Header("Weapon Skill")]
    [SerializeField] float chargeTime;
    [SerializeField] float activeTime;
    [SerializeField] float currentChargeTime;
    [SerializeField] float currentActiveTime;

    public int chargeAmount;
    public int maxChargeAmount;

    [SerializeField] private GameObject sliderObject;
    [SerializeField] private StatusBar statusComponent;

    public float rotation;

    enum SkillState
    {
        ready,
        active,
        charge
    }
    SkillState state = SkillState.charge;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        charecter = FindObjectOfType<Charecter>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        //weaponCollider = PlayerController.instance.GetSpearWeaponCollider();
        weaponCollider.gameObject.GetComponent<SwordDamageSouce>().damageAmount = this.damageAmount;
        //slashAnimSpawnPoint = GameObject.Find("SlashSpawnPoint").transform;
        weaponCollider.gameObject.GetComponent<SwordDamageSouce>().chargeAmount = 0;
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
            case SkillState.ready:
                if (Input.GetKeyDown(KeyCode.E))
                {
                    SkillActivate();
                    statusComponent.Set(weaponCollider.gameObject.GetComponent<SwordDamageSouce>().chargeAmount, maxChargeAmount);
                    state = SkillState.active;
                    activeTime = currentActiveTime;
                }
                break;
            case SkillState.active:
                if (activeTime > 0)
                {
                    weaponCollider.gameObject.GetComponent<SwordDamageSouce>().chargeAmount = 0;
                    activeTime -= Time.deltaTime;
                }
                else
                {
                    state = SkillState.charge;
                    chargeTime = currentActiveTime;
                }
                break;
            case SkillState.charge:
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
                    state = SkillState.ready;
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
            weaponCollider.gameObject.GetComponent<DamageSouce>();
            StartCoroutine(ReduceStaminaRoutine());
        }
    }

    public void SkillActivate()
    {
        animator.SetTrigger("Skill");
        weaponCollider.gameObject.SetActive(true);
    }

    private IEnumerator ReduceStaminaRoutine()
    {
        charecter.ReduceStamina(staminaCost);
        yield return new WaitForSeconds(1f);
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
        //slashAnim.gameObject.transform.rotation = Quaternion.Euler(-180, 0, 0);

        if (PlayerController.instance.FacingLeft)
        {
            //slashAnim.GetComponent<SpriteRenderer>().flipX = true;
        }
    }

    public void SwingDownFlipAnimEvent()
    {
        //slashAnim.gameObject.transform.rotation = Quaternion.Euler(0, 0, 0);

        if (PlayerController.instance.FacingLeft)
        {
            //slashAnim.GetComponent<SpriteRenderer>().flipX = true;
        }
    }

    private void MouseFollowWithOffset()
    {
        Vector3 mousePos = Input.mousePosition;
        Vector3 playerScreenPoint = Camera.main.WorldToScreenPoint(PlayerController.instance.transform.position);

        float angle = Mathf.Atan2(mousePos.y, mousePos.x) * Mathf.Rad2Deg;

        if (mousePos.x < playerScreenPoint.x)
        {
            
        }
        else
        {
            
        }
    }
}
