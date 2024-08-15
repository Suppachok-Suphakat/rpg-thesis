using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;

    public GameObject inventoryMenu;
    public GameObject equipmentMenu;
    public GameObject partnerMenu;

    public ItemSlot[] itemSlot;
    public EquipmentSlot[] equipmentSlot;
    public EquippedSlot[] equippedSlot;

    public ItemSO[] itemSOs;

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.I))
        {
            Inventory();
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            Equipment();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ExitInventory();
            ExitEquipment();
        }
    }

    void Inventory()
    {
        if (inventoryMenu.activeSelf)
        {
            PlayerController.instance.isMenuActive = false;
            Time.timeScale = 1;
            inventoryMenu.SetActive(false);
            equipmentMenu.SetActive(false);
        }
        else
        {
            PlayerController.instance.isMenuActive = true;
            Time.timeScale = 0;
            inventoryMenu.SetActive(true);
            equipmentMenu.SetActive(false);
            partnerMenu.SetActive(false);
        }
    }

    void Equipment()
    {
        if (equipmentMenu.activeSelf)
        {
            PlayerController.instance.isMenuActive = false;
            Time.timeScale = 1;
            inventoryMenu.SetActive(false);
            equipmentMenu.SetActive(false);
        }
        else
        {
            PlayerController.instance.isMenuActive = true;
            Time.timeScale = 0;
            equipmentMenu.SetActive(true);
            inventoryMenu.SetActive(false);
            partnerMenu.SetActive(false);
        }
    }

    void ExitInventory()
    {
        if (inventoryMenu.activeSelf)
        {
            PlayerController.instance.isMenuActive = false;
            Time.timeScale = 1;
            inventoryMenu.SetActive(false);
            equipmentMenu.SetActive(false);
        }
    }

    void ExitEquipment()
    {
        if (equipmentMenu.activeSelf)
        {
            PlayerController.instance.isMenuActive = false;
            Time.timeScale = 1;
            inventoryMenu.SetActive(false);
            equipmentMenu.SetActive(false);
        }
    }

    public bool UseItem(string itemName)
    {
        for (int i = 0; i < itemSOs.Length; i++)
        {
            if (itemSOs[i].itemName == itemName)
            {
                bool useable = itemSOs[i].UseItem();
                return useable;
            }
        }
        return false;
    }

    public int AddItem(string itemName, int quantity, Sprite itemSprite, string itemDescription, ItemType itemType, WeaponInfo weaponInfo)
    {
        if(itemType == ItemType.consumeable || itemType == ItemType.collectible)
        {
            for (int i = 0; i < itemSlot.Length; i++)
            {
                if (itemSlot[i].isFull == false && itemSlot[i].itemName == itemName || itemSlot[i].quantity == 0)
                {
                    int leftOverItems = itemSlot[i].AddItem(itemName, quantity, itemSprite, itemDescription, itemType);

                    if (leftOverItems > 0)
                    {
                        leftOverItems = AddItem(itemName, leftOverItems, itemSprite, itemDescription, itemType, weaponInfo);
                    }
                    return leftOverItems;
                }
            }
            return quantity;
        }
        else
        {
            for (int i = 0; i < equipmentSlot.Length; i++)
            {
                if (equipmentSlot[i].isFull == false && equipmentSlot[i].itemName == itemName || equipmentSlot[i].quantity == 0)
                {
                    int leftOverItems = equipmentSlot[i].AddItem(itemName, quantity, itemSprite, itemDescription, itemType, weaponInfo);

                    if (leftOverItems > 0)
                    {
                        leftOverItems = AddItem(itemName, leftOverItems, itemSprite, itemDescription, itemType, weaponInfo);
                    }
                    return leftOverItems;
                }
            }
            return quantity;
        }
    }


    public void DeselectAllSlots()
    {
        for (int i = 0; i < itemSlot.Length; i++)
        {
            itemSlot[i].selectedShader.SetActive(false);
            itemSlot[i].thisItemSelected = false;
        }

        for (int i = 0; i < equipmentSlot.Length; i++)
        {
            equipmentSlot[i].selectedShader.SetActive(false);
            equipmentSlot[i].thisItemSelected = false;
        }

        for (int i = 0; i < equippedSlot.Length; i++)
        {
            equippedSlot[i].selectedShader.SetActive(false);
            equippedSlot[i].thisItemSelected = false;
        }
    }
}

public enum ItemType
{
    consumeable,
    collectible,
    assaultWeapon,
    magicWeapon,
    rangeWeapon,
    head,
    body,
    shields,
    rings,
    relics,
    none
};