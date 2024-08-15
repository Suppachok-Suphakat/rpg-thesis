using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance;

    private TMP_Text goldText;
    public int currentGold = 0;

    //HUD
    public GameObject shopCanvas;
    public GameObject statusCanvas;
    public GameObject inventoryCanvas;
    public GameObject PartnerCanvas;

    //Player
    private PlayerController playerController;

    const string COIN_AMOUNT_TEXT = "Gold Amount Text";

    public ShopItemSO[] shopItemSOs;
    public ShopSlot[] shopSlot;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        playerController = GameObject.Find("Player").GetComponent<PlayerController>();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            ExitShop();
        }
    }

    void ExitShop()
    {
        if (shopCanvas.activeSelf)
        {
            PlayerController.instance.isMenuActive = false;
            Time.timeScale = 1;
            shopCanvas.SetActive(false);
        }
    }

    public bool BuyItem(string itemName)
    {
        for (int i = 0; i < shopItemSOs.Length; i++)
        {
            if (shopItemSOs[i].itemName == itemName)
            {
                bool buyable = shopItemSOs[i].BuyItem();
                return buyable;
            }
        }
        return false;
    }

    public void DeselectAllSlots()
    {
        for (int i = 0; i < shopSlot.Length; i++)
        {
            shopSlot[i].selectedShader.SetActive(false);
            shopSlot[i].thisItemSelected = false;
        }
    }

    public void UpdateCurrentGold(int pickupAmout)
    {
        currentGold += pickupAmout;

        if (goldText == null)
        {
            goldText = GameObject.Find(COIN_AMOUNT_TEXT).GetComponent<TMP_Text>();
        }

        goldText.text = currentGold.ToString("D4");
    }

    public void DecreaseCurrentGold(int itemPrice)
    {
        currentGold -= itemPrice;

        if (goldText == null)
        {
            goldText = GameObject.Find(COIN_AMOUNT_TEXT).GetComponent<TMP_Text>();
        }

        goldText.text = currentGold.ToString("D4");
    }
}
