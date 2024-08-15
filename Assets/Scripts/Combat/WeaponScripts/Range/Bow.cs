using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class Bow : MonoBehaviour, IWeapon
{
    [SerializeField] private WeaponInfo weaponInfo;
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private Transform arrowSpawnPoint;
    [SerializeField] private int staminaCost;
    private Charecter charecter;

    readonly int FIRE_HASH = Animator.StringToHash("Fire");

    private Animator myAnimator;

    [Header("Weapon Skill")]
    [SerializeField] private GameObject arrowSkillPrefab;
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

        sliderObject = GameObject.Find("RangeWeaponSkillBar");
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
        myAnimator.SetTrigger(FIRE_HASH);
        GameObject newArrow = Instantiate(arrowPrefab, arrowSpawnPoint.position, 
            ActiveWeapon.Instance.transform.rotation);
        newArrow.GetComponent<Arrow>().UpdateProjectileRange(weaponInfo.weaponRange);
        StartCoroutine(ReduceStaminaRoutine());
    }

    private IEnumerator ReduceStaminaRoutine()
    {
        charecter.ReduceStamina(staminaCost);
        yield return new WaitForSeconds(0.5f);
    }

    public void SkillActivate()
    {
        ShootSpread();
        //StartCoroutine(ShootArrows());
    }

    private IEnumerator ShootArrows()
    {
        for (int i = 0; i < 3; i++)
        {
            yield return new WaitForSeconds(0.1f); // Adjust the delay between shots as needed

            GameObject newArrow = Instantiate(arrowPrefab, arrowSpawnPoint.position, ActiveWeapon.Instance.transform.rotation);
            newArrow.GetComponent<Arrow>().UpdateProjectileRange(weaponInfo.weaponRange);
        }
    }

    private void ShootSpread()
    {
        // Calculate the angle between arrows
        float angleStep = 15f; // Adjust the angle step as needed
        float startAngle = -angleStep; // Start with a negative angle to spread evenly

        // Instantiate three arrows with different angles
        for (int i = 0; i < 3; i++)
        {
            float angle = startAngle + i * angleStep; // Calculate the angle for this arrow
            Quaternion rotation = ActiveWeapon.Instance.transform.rotation; // Use the rotation of ActiveWeapon.Instance.transform

            // Rotate the quaternion to the desired angle around the Z axis
            rotation *= Quaternion.Euler(0f, 0f, angle);

            GameObject newArrow = Instantiate(arrowPrefab, arrowSpawnPoint.position, rotation);
            newArrow.GetComponent<Arrow>().UpdateProjectileRange(weaponInfo.weaponRange);
        }
    }

    public WeaponInfo GetWeaponInfo()
    {
        return weaponInfo;
    }
}
