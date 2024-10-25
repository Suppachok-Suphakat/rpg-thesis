using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats instance;

    public int vitality, radiance, eclipse, strength, dexterity, intelligence;
    public int hp, mp, armor, stamina;

    [SerializeField] public TMP_Text vitalityText, radianceText, eclipseText,
        strengthText, dexterityText, intelligenceText;

    [SerializeField] public TMP_Text hpText, mpText, staminaText, armorText;

    [SerializeField] public TMP_Text vitalityPreText, radiancePreText, eclipsePreText, 
        armorPreText, strengthPreText, dexterityPreText, intelligencePreText;

    [Header("PreviewInfo")]
    //[SerializeField] public Image previewInfo;
    [SerializeField] private TMP_Text itemDescriptionNameText;
    [SerializeField] private TMP_Text itemDescriptionText;

    [SerializeField] public GameObject selectedItemIncreaseStats;
    [SerializeField] public GameObject selectedItemRequiredStats;
    //[SerializeField] public GameObject selectedItemImage;

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        UpdateEquipmentStats();
        TurnOffPreviewStats();
    }

    public void UpdateEquipmentStats()
    {
        vitalityText.text = vitality.ToString();
        radianceText.text = radiance.ToString();
        eclipseText.text = eclipse.ToString();

        strengthText.text = strength.ToString();
        dexterityText.text = dexterity.ToString();
        intelligenceText.text = intelligence.ToString();

        hpText.text = Character.instance.hp.currVal.ToString() + "/ " + Character.instance.hp.maxVal.ToString();
        mpText.text = Character.instance.mana.currVal.ToString() + "/ " + Character.instance.mana.maxVal.ToString();
        staminaText.text = Character.instance.stamina.currVal.ToString() + "/ " + Character.instance.stamina.maxVal.ToString();
        armorText.text = armor.ToString();
    }

    public void PreviewEquipmentStats(int vitality, int radiance, int eclipse, int armor, 
        int strength,int dexterity, int intelligence, string itemName, string itemDescription)
    {
        vitalityPreText.text = vitality.ToString();
        radiancePreText.text = radiance.ToString();
        eclipsePreText.text = eclipse.ToString();

        armorPreText.text = armor.ToString();

        //previewImage.sprite = itemSprite;

        strengthPreText.text = strength.ToString();
        dexterityPreText.text = dexterity.ToString();
        intelligencePreText.text = intelligence.ToString();

        itemDescriptionNameText.text = itemName;
        itemDescriptionText.text = itemDescription;

        selectedItemIncreaseStats.SetActive(true);
        selectedItemRequiredStats.SetActive(true);
    }

    public void TurnOffPreviewStats()
    {
        itemDescriptionNameText.text = "";
        itemDescriptionText.text = "";
        selectedItemIncreaseStats.SetActive(false);
        selectedItemRequiredStats.SetActive(false);
    }
}
