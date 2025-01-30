using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunSkill : MonoBehaviour, IWeapon
{
    [SerializeField] private WeaponInfo weaponInfo;
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private Transform arrowSpawnPoint;
    [SerializeField] private int staminaCost;
    private Character character;

    readonly int FIRE_HASH = Animator.StringToHash("Fire");

    private Animator myAnimator;

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

    }

    public void Attack()
    {
        myAnimator.SetTrigger(FIRE_HASH);
        GameObject newArrow = Instantiate(arrowPrefab, arrowSpawnPoint.position,
            ActiveWeapon.Instance.transform.rotation);
        newArrow.GetComponent<Bullet>().UpdateProjectileRange(weaponInfo.weaponRange);
        StartCoroutine(ReduceStaminaRoutine());
    }

    private IEnumerator ReduceStaminaRoutine()
    {
        character.ReduceStamina(staminaCost);
        yield return new WaitForSeconds(0.5f);
    }

    public void SkillActivate()
    {
        //ShootSpread();
        StartCoroutine(ShootArrows());
    }

    private IEnumerator ShootArrows()
    {
        for (int i = 0; i < 3; i++)
        {
            yield return new WaitForSeconds(0.1f); // Adjust the delay between shots as needed

            GameObject newArrow = Instantiate(arrowPrefab, arrowSpawnPoint.position, ActiveWeapon.Instance.transform.rotation);
            newArrow.GetComponent<Bullet>().UpdateProjectileRange(weaponInfo.weaponRange);
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
