using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.U2D;

public class ShopSlot : MonoBehaviour, IPointerClickHandler
{
    //====ITEM DATA====//
    public string itemName;
    public int quantity;
    public Sprite itemSprite;
    public bool isFull;
    public string itemDescription;
    public Sprite emptySprite;
    public WeaponInfo weaponInfo;
    public ItemType itemType;
    public int itemPrice;

    [SerializeField] private int maxNumberOfItems;

    //====ITEM SLOT====//
    [SerializeField] private Image itemImage;
    [SerializeField] private TMP_Text quantityText;
    [SerializeField] private GameObject quantityTextGO;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private GameObject priceTextGO;

    //====ITEM DESCRIPTION====//
    [SerializeField] private Image itemDescriptionImage;
    [SerializeField] private TMP_Text itemDescriptionNameText;
    [SerializeField] private TMP_Text itemDescriptionText;


    public GameObject selectedShader;
    public bool thisItemSelected;

    private EconomyManager economyManager;
    private InventoryManager inventoryManager;

    private SpriteRenderer sr;

    public ShopItemSO shopItem;

    private void Start()
    {
        economyManager = GameObject.Find("EconomyCanvas").GetComponent<EconomyManager>();
        inventoryManager = GameObject.Find("InventoryCanvas").GetComponent<InventoryManager>();

        sr = GetComponent<SpriteRenderer>();
        quantityTextGO.SetActive(false);

        if (shopItem != null)
        {
            //Update Item Type
            this.itemType = shopItem.itemType;

            //Update Weapon Info
            this.weaponInfo = shopItem.weaponInfo;

            //Update Name
            this.itemName = shopItem.itemName;

            //Update Image
            this.itemSprite = shopItem.itemSprite;
            itemImage.sprite = shopItem.itemSprite;

            //Update Description
            this.itemDescription = shopItem.itemDescription;

            //Update Quantity
            this.quantity = shopItem.itemQuantity;

            //Update Price
            this.itemPrice = shopItem.itemPrice;

            if (this.quantity >= maxNumberOfItems)
            {
                quantityTextGO.SetActive(true);
                //quantityText.enabled = true;
                quantityText.text = $"{maxNumberOfItems}";
                isFull = true;

                //Return Leftovers
                int extraItems = this.quantity - maxNumberOfItems;
                this.quantity = maxNumberOfItems;
            }

            //Update QuantityText
            quantityTextGO.SetActive(true);
            //quantityText.enabled = true;
            quantityText.text = $"{this.quantity}";
        }
    }

    private void Update()
    {
        //if(this.quantity == 0)
        //{
        //    quantityText.text = "";
        //}
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            OnLeftClick();
        }
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            //OnRightClick();
        }
    }

    public void OnLeftClick()
    {
        if (thisItemSelected)
        {
            if(shopItem != null && EconomyManager.Instance.currentGold >= itemPrice)
            {
                inventoryManager.AddItem(itemName, 1, itemSprite, itemDescription, itemType, weaponInfo);
                this.quantity -= 1;
                quantityText.text = $"{this.quantity}";
                EconomyManager.Instance.DecreaseCurrentGold(itemPrice);
                if (this.quantity <= 0)
                {
                    EmptySlot();
                }
            }
        }
        else
        {
            economyManager.DeselectAllSlots();
            selectedShader.SetActive(true);
            thisItemSelected = true;
            if(shopItem != null)
            {
                itemDescriptionNameText.text = itemName;
                itemDescriptionText.text = itemDescription;
                itemDescriptionImage.sprite = itemSprite;
                priceText.text = itemPrice.ToString();
            }
            else
            {
                itemDescriptionNameText.text = "";
                itemDescriptionText.text = "";
                itemDescriptionImage.sprite = emptySprite;
                priceText.text = "";
            }
        }

        if (itemDescriptionImage.sprite == null)
        {
            itemDescriptionImage.sprite = emptySprite;
        }
    }

    private void EmptySlot()
    {
        quantityText.enabled = false;
        priceText.enabled = false;
        itemImage.sprite = emptySprite;

        itemDescriptionNameText.text = "";
        itemDescriptionText.text = "";
        itemDescriptionImage.sprite = emptySprite;

        //Update Name
        this.itemName = "";

        //Update Image
        this.itemSprite = emptySprite;
        itemImage.sprite = emptySprite;

        //Update Description
        this.itemDescription = "";
    }

    public void OnRightClick()
    {
        
    }
}
