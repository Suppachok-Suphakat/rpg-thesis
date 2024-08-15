using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordSkill : MonoBehaviour
{
    [SerializeField] private GameObject slashAnimPrefab;
    [SerializeField] private Transform slashAnimSpawnPoint;
    [SerializeField] private int staminaCost;

    private Transform weaponCollider;
    private Animator animator;
    private Charecter charecter;

    private GameObject slashAnim;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        charecter = FindObjectOfType<Charecter>();
    }

    private void Start()
    {
        weaponCollider = PlayerController.instance.GetSwordWeaponCollider();
    }

    private void Update()
    {
        
    }

    private IEnumerator ReduceWeaponChargeRoutine()
    {
        charecter.ReduceStamina(staminaCost);
        yield return new WaitForSeconds(1);
    }

    public void DoneAttackingAnimEvent()
    {
        weaponCollider.gameObject.SetActive(false);
    }
}
