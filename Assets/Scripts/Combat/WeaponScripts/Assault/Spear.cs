using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class Spear : MonoBehaviour, IWeapon
{
    [SerializeField] private WeaponInfo weaponInfo;
    [SerializeField] private int staminaCost;

    [SerializeField] private int damageAmount;

    [SerializeField] private Transform weaponCollider;
    private Animator animator;
    private Character character;
    private SpriteRenderer spriteRenderer;

    public float rotation;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        character = FindObjectOfType<Character>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        //weaponCollider = PlayerController.instance.GetSpearWeaponCollider();
        weaponCollider.gameObject.GetComponent<SwordDamageSouce>().damageAmount = this.damageAmount;
        //slashAnimSpawnPoint = GameObject.Find("SlashSpawnPoint").transform;
        weaponCollider.gameObject.GetComponent<SwordDamageSouce>().chargeAmount = 0;
    }

    private void OnDestroy()
    {

    }

    private void Update()
    {
        MouseFollowWithOffset();
    }

    public WeaponInfo GetWeaponInfo()
    {
        return weaponInfo;
    }

    public void Attack()
    {
        if (character.stamina.currVal >= staminaCost && !PlayerController.instance.isMenuActive)
        {
            animator.SetTrigger("Attack");
            weaponCollider.gameObject.SetActive(true);
            weaponCollider.gameObject.GetComponent<DamageSouce>();
            StartCoroutine(ReduceStaminaRoutine());
        }
    }

    private IEnumerator ReduceStaminaRoutine()
    {
        character.ReduceStamina(staminaCost);
        yield return new WaitForSeconds(1f);
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
