using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ItemSlot : MonoBehaviour, IPointerClickHandler
{
    //====ITEM DATA====//
    public string itemName;
    public int quantity;
    public Sprite itemSprite;
    public bool isFull;
    public string itemDescription;
    public Sprite emptySprite;
    public ItemType itemType;

    [SerializeField] private int maxNumberOfItems;

    //====ITEM SLOT====//
    [SerializeField] private Image itemImage;
    [SerializeField] private TMP_Text quantityText;
    [SerializeField] private GameObject quantityTextGO;

    //====ITEM DESCRIPTION====//
    [SerializeField] private Image itemDescriptionImage;
    [SerializeField] private TMP_Text itemDescriptionNameText;
    [SerializeField] private TMP_Text itemDescriptionText;


    public GameObject selectedShader;
    public bool thisItemSelected;

    private InventoryManager inventoryManager;

    private SpriteRenderer sr;

    private void Start()
    {
        inventoryManager = GameObject.Find("InventoryCanvas").GetComponent<InventoryManager>();
        sr = GetComponent<SpriteRenderer>();
        //quantityText.text = "";
        quantityTextGO.SetActive(false);
        //quantityText.enabled = false;
    }

    private void Update()
    {
        //if(this.quantity == 0)
        //{
        //    quantityText.text = "";
        //}
    }

    public int AddItem(string itemName, int quantity, Sprite itemSprite, string itemDescription, ItemType itemType)
    {
        if (isFull)
        {
            return quantity;
        }

        //Update Item Type
        this.itemType = itemType;

        //Update Name
        this.itemName = itemName;

        //Update Image
        this.itemSprite = itemSprite;
        itemImage.sprite = itemSprite;

        //Update Description
        this.itemDescription = itemDescription;

        //Update Quantity
        this.quantity += quantity;

        if (this.quantity >= maxNumberOfItems)
        {
            quantityTextGO.SetActive(true);
            //quantityText.enabled = true;
            quantityText.text = $"{maxNumberOfItems}";
            isFull = true;

            //Return Leftovers
            int extraItems = this.quantity - maxNumberOfItems;
            this.quantity = maxNumberOfItems;
            return extraItems;
        }

        //Update QuantityText
        quantityTextGO.SetActive(true);
        //quantityText.enabled = true;
        quantityText.text = $"{this.quantity}";

        return 0;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(eventData.button == PointerEventData.InputButton.Left)
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
            bool useable = inventoryManager.UseItem(itemName);
            if (useable)
            {
                this.quantity -= 1;
                quantityText.text = $"{this.quantity}";
                if (this.quantity <= 0)
                {
                    EmptySlot();
                }
            }
        }
        else
        {
            inventoryManager.DeselectAllSlots();
            selectedShader.SetActive(true);
            thisItemSelected = true;
            itemDescriptionNameText.text = itemName;
            itemDescriptionText.text = itemDescription;
            itemDescriptionImage.sprite = itemSprite;
        }

        if(itemDescriptionImage.sprite == null)
        {
            itemDescriptionImage.sprite = emptySprite;
        }
    }

    private void EmptySlot()
    {
        quantityText.enabled = false;
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
        GameObject itemToDrop = new GameObject(itemName);
        Item newItem = itemToDrop.AddComponent<Item>();
        newItem.quantity = 1;
        newItem.itemName = itemName;
        newItem.sprite = itemSprite;
        newItem.itemDescription = itemDescription;

        //Pickup newPickup = itemToDrop.AddComponent<Pickup>();

        SpriteRenderer sr = itemToDrop.AddComponent<SpriteRenderer>();
        sr.sprite = itemSprite;
        sr.sortingOrder = 0;
        sr.sortingLayerName = "Default";

        itemToDrop.AddComponent<CapsuleCollider2D>();
        Rigidbody2D rb2D = itemToDrop.AddComponent<Rigidbody2D>();
        rb2D.gravityScale = 0f;

        itemToDrop.transform.position = GameObject.FindWithTag("Player").transform.position
            + new Vector3(1f, 0, 0);
        sr.sortingOrder = 100;
        //itemToDrop.transform.localScale = new Vector3(1, 1, 1);

        this.quantity -= 1;
        quantityText.text = $"{this.quantity}";
        if (this.quantity <= 0)
        {
            EmptySlot();
        }
    }
}
