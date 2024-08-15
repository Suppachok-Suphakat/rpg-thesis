using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class EquipmentSO : ScriptableObject
{
    public string itemName;
    public string itemDescription;

    [Header("IncreaseStats")]
    public int vitality, radiance, eclipse, armor, strength, dexterity, intelligence;

    //[SerializeField] public Sprite itemSprite;
    //[SerializeField] public Sprite itemSprite;
    [Header("WeaponInfo")]
    public WeaponInfo weaponInfo;

    public void PreviewEquipment()
    {
        GameObject.Find("StatManager").GetComponent<PlayerStats>().
            PreviewEquipmentStats(vitality, radiance, eclipse, armor, 
            strength, dexterity, intelligence, itemName, itemDescription);
    }

    public void EquipItem()
    {
        PlayerStats playerStats = GameObject.Find("StatManager").GetComponent<PlayerStats>();
        playerStats.vitality += vitality;
        Charecter.instance.hp.maxVal += vitality;
        Charecter.instance.hp.currVal += vitality;
        playerStats.radiance += radiance;
        playerStats.eclipse += eclipse;
        playerStats.armor += armor;

        playerStats.strength += strength;
        playerStats.dexterity += dexterity;
        playerStats.intelligence += intelligence;

        playerStats.UpdateEquipmentStats();
    }

    public void UnEquipItem()
    {
        PlayerStats playerStats = GameObject.Find("StatManager").GetComponent<PlayerStats>();
        playerStats.vitality -= vitality;
        Charecter.instance.hp.maxVal -= vitality;
        Charecter.instance.hp.currVal -= vitality;
        playerStats.radiance -= radiance;
        playerStats.eclipse -= eclipse;
        playerStats.armor -= armor;

        playerStats.strength -= strength;
        playerStats.dexterity -= dexterity;
        playerStats.intelligence -= intelligence;

        playerStats.UpdateEquipmentStats();
    }
}
