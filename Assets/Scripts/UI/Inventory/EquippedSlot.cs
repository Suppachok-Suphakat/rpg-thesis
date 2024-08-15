using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System;

public class EquippedSlot : MonoBehaviour,IPointerClickHandler
{
    //SLOT APPEARANCE//
    [SerializeField] public Image slotImage;

    [SerializeField] private TMP_Text slotName;
    [SerializeField] private string thisSlotName;

    //SLOT DATA//
    [SerializeField] private ItemType itemType = new ItemType();

    private Sprite itemSprite;
    private string itemName;
    public string itemDescription;

    [SerializeField] private TMP_Text itemDescriptionNameText;
    [SerializeField] private TMP_Text itemDescriptionText;

    //OTHER VARIABLE//
    private bool slotInUse;
    [SerializeField] public GameObject selectedShader;
    [SerializeField] public bool thisItemSelected;
    [SerializeField] public Sprite emptySprite;

    [SerializeField] public ToolbarSlot toolbarSlot;

    private InventoryManager inventoryManager;
    private EquipmentSOLibrary equipmentSOLibrary;

    //TOOLBAR WEAPON//
    [SerializeField] private WeaponInfo weaponInfo;
    [SerializeField] private GameObject weaponPrefab;
    [SerializeField] private WeaponInfo noneWeaponInfo;

    public WeaponInfo GetWeaponInfo()
    {
        return weaponInfo;
    }

    private void Start()
    {
        inventoryManager = GameObject.Find("InventoryCanvas").GetComponent<InventoryManager>();
        equipmentSOLibrary = GameObject.Find("InventoryCanvas").GetComponent<EquipmentSOLibrary>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        //ON LEFT CLICK
        if(eventData.button == PointerEventData.InputButton.Left)
        {
            OnLeftClick();
        }
        //ON RIGHT CLICK
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            OnRightClick();
        }
    }
    private void OnLeftClick()
    {
        if(thisItemSelected && slotInUse)
        {
            UnEquipGear();
        }
        else
        {
            inventoryManager.DeselectAllSlots();
            selectedShader.SetActive(true);
            thisItemSelected = true;
            itemDescriptionNameText.text = itemName;
            itemDescriptionText.text = itemDescription;

            if (!slotInUse)
            {
                GameObject.Find("StatManager").GetComponent<PlayerStats>().TurnOffPreviewStats();
            }

            for (int i = 0; i < equipmentSOLibrary.equipmentSO.Length; i++)
            {
                if (equipmentSOLibrary.equipmentSO[i].itemName == this.itemName)
                {
                    equipmentSOLibrary.equipmentSO[i].PreviewEquipment();
                }
            }
        }
    }


    private void OnRightClick()
    {
        UnEquipGear();
    }


    public void EquipGear(Sprite itemSprite, string itemName, string itemDescription, WeaponInfo weaponInfo)
    {
        if(slotInUse)
        {
            UnEquipGear();
        }

        //UPDATE IMAGE
        this.itemSprite = itemSprite;
        slotImage.sprite = this.itemSprite;
        slotName.text = itemName;

        //UPDATE DATA
        this.itemName = itemName;
        this.itemDescription = itemDescription;

        this.weaponInfo = weaponInfo;

        if(toolbarSlot != null)
        {
            toolbarSlot.weaponInfo = weaponInfo;
            toolbarSlot.slotSprite.GetComponent<Image>().sprite = slotImage.sprite;
        }

        GameObject.Find("ActiveToolbar").GetComponent<ActiveToolbar>().ChangeActiveWeapon();

        //UPDATE PLAYER STAT
        for (int i = 0; i < equipmentSOLibrary.equipmentSO.Length; i++)
        {
            if (equipmentSOLibrary.equipmentSO[i].itemName == this.itemName)
            {
                equipmentSOLibrary.equipmentSO[i].EquipItem();
            }
        }

        slotInUse = true;
    }

    private void UnEquipGear()
    {
        inventoryManager.DeselectAllSlots();

        string itemNameToUnequip = this.itemName;
        string itemDescriptionToUnequip = this.itemDescription;
        Sprite itemSpriteToUnequip = this.itemSprite;

        inventoryManager.AddItem(itemNameToUnequip, 1, itemSpriteToUnequip, itemDescriptionToUnequip, itemType, weaponInfo);

        this.itemSprite = emptySprite;
        slotImage.sprite = this.emptySprite;
        slotName.text = thisSlotName;

        this.itemName = "";
        this.itemDescription = "";
        this.weaponInfo = null;

        if (toolbarSlot != null)
        {
            toolbarSlot.weaponInfo = null;
            toolbarSlot.slotSprite.GetComponent<Image>().sprite = slotImage.sprite;
        }
        GameObject.Find("ActiveToolbar").GetComponent<ActiveToolbar>().ChangeActiveWeapon();

        //UPDATE PLAYER STAT
        for (int i = 0; i < equipmentSOLibrary.equipmentSO.Length; i++)
        {
            if (equipmentSOLibrary.equipmentSO[i].itemName == itemNameToUnequip)
            {
                equipmentSOLibrary.equipmentSO[i].UnEquipItem();
            }
        }

        slotInUse = false;
        GameObject.Find("StatManager").GetComponent<PlayerStats>().TurnOffPreviewStats();
    }
}
