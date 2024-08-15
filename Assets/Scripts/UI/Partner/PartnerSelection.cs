using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartnerSelection : MonoBehaviour
{
    public GameObject partnerImage;
    public GameObject partnerSelected;

    public void KnightPartner()
    {
        if (!partnerImage.activeSelf)
        {
            partnerImage.SetActive(true);
            partnerSelected.SetActive(true);
        }
    }

    public void OtherPartner()
    {
        if (partnerImage.activeSelf)
        {
            partnerImage.SetActive(false);
            partnerSelected.SetActive(false);
        }
    }
}
