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
            Character character = GameObject.Find("Player").GetComponent<Character>();
            if(character.hp.currVal == character.hp.maxVal)
            {
                return false;
            }
            else
            {
                character.Heal(amountToChangeStat);
                return true;
            }
        }

        if (statToChange == StatToChange.mana)
        {
            Character character = GameObject.Find("Player").GetComponent<Character>();
            if (character.mana.currVal == character.mana.maxVal)
            {
                return false;
            }
            else
            {
                character.RestoreMana(amountToChangeStat);
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
