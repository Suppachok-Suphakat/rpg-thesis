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
    private Character character;

    readonly int ATTACK_HASH = Animator.StringToHash("Attack");

    private void Awake()
    {
        myAnimator = GetComponent<Animator>();
        character = FindObjectOfType<Character>();
    }

    private void Start()
    {
        
    }

    private void Update()
    {
        MouseFollowWithOffset();
    }


    public void Attack()
    {
        if (character.mana.currVal >= manaCost)
        {
            myAnimator.SetTrigger(ATTACK_HASH);
        }
    }

    private IEnumerator ReduceManaRoutine()
    {
        character.ReduceMana(manaCost);
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
        //Instantiate(magicSkill, mousePosition, Quaternion.identity);
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
