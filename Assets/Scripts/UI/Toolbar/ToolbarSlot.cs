using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToolbarSlot : MonoBehaviour
{
    [SerializeField] public WeaponInfo weaponInfo;

    public GameObject slotSprite;

    [SerializeField] StatusBar chargeBar;

    [SerializeField] private ItemType itemType = new ItemType();

    private void Start()
    {
        Image imageComponent = slotSprite.GetComponent<Image>();
        if (imageComponent != null && weaponInfo != null)
        {
            imageComponent.sprite = weaponInfo.weaponSprite;
        }
    }

    public WeaponInfo GetWeaponInfo()
    {
        return weaponInfo;
    }
}
