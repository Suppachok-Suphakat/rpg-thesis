using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class ItemSO : ScriptableObject
{
    public string itemName;
    public StatToChange statToChange = new StatToChange();
    public int amountToChangeStat;

    public AttributeToChange attributesToChange = new AttributeToChange();
    public int amountToChangeAttribute;

    public int itemPrice;

    public bool UseItem()
    {
        if(statToChange == StatToChange.health)
        {
            Charecter charecter = GameObject.Find("Player").GetComponent<Charecter>();
            if(charecter.hp.currVal == charecter.hp.maxVal)
            {
                return false;
            }
            else
            {
                charecter.Heal(amountToChangeStat);
                return true;
            }
        }

        if (statToChange == StatToChange.mana)
        {
            Charecter charecter = GameObject.Find("Player").GetComponent<Charecter>();
            if (charecter.mana.currVal == charecter.mana.maxVal)
            {
                return false;
            }
            else
            {
                charecter.RestoreMana(amountToChangeStat);
                return true;
            }
        }
        return false;
    }

    public bool BuyItem()
    {
        if (EconomyManager.Instance.currentGold >= itemPrice)
        {
            EconomyManager.Instance.DecreaseCurrentGold(itemPrice);
        }
        return false;
    }

    public enum StatToChange
    {
        none,
        health,
        mana,
        stamina
    };

    public enum AttributeToChange
    {
        none,
        strength,
        defense,
        agility
    };
}
