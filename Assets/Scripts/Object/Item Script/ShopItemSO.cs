using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class ShopItemSO : ScriptableObject
{
    public string itemName;
    public string itemDescription;
    public int itemQuantity;
    public Sprite itemSprite;
    public ItemType itemType;
    public WeaponInfo weaponInfo;

    public int itemPrice;

    public bool BuyItem()
    {
        if (EconomyManager.Instance.currentGold >= itemPrice)
        {
            EconomyManager.Instance.DecreaseCurrentGold(itemPrice);
        }
        return false;
    }
}
