using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartnerSkillManager : MonoBehaviour
{
    public static PartnerSkillManager Instance;

    public GameObject inventoryMenu;
    public GameObject equipmentMenu;

    public GameObject hpSliderObject;
    public GameObject knightMPSliderObject;
    public GameObject archerMPSliderObject;
    public GameObject bubbleObject;

    public GameObject partnerMenu;

    [Header("Knight Partner")]
    public GameObject knightPartnerStatusBar;
    public GameObject knightPartnerPreview;
    public GameObject knightPartner;
    public GameObject knightSelected;

    [Header("Archer Partner")]
    public GameObject archerPartnerStatusBar;
    public GameObject archerPartnerPreview;
    public GameObject archerPartner;
    public GameObject archerSelected;

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
        //partnerStatusBar.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            PartnerMenu();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ExitPartnerMenu();
        }
    }

    void PartnerMenu()
    {
        if (partnerMenu.activeSelf)
        {
            PlayerController.instance.isMenuActive = false;
            Time.timeScale = 1;
            partnerMenu.SetActive(false);
        }
        else
        {
            PlayerController.instance.isMenuActive = true;
            Time.timeScale = 0;
            partnerMenu.SetActive(true);
            inventoryMenu.SetActive(false);
            equipmentMenu.SetActive(false);
        }
    }

    void ExitPartnerMenu()
    {
        if (partnerMenu.activeSelf)
        {
            PlayerController.instance.isMenuActive = false;
            Time.timeScale = 1;
            partnerMenu.SetActive(false);
        }
    }

    public void KnightPartner()
    {
        if (!knightPartner.activeSelf)
        {
            if (archerPartner.activeSelf)
            {
                archerPartner.SetActive(false);
                archerPartnerStatusBar.SetActive(false);
                archerPartnerPreview.SetActive(false);
                archerSelected.SetActive(false);
            }

            knightPartner.SetActive(true);
            knightPartnerStatusBar.SetActive(true);
            knightPartnerPreview.SetActive(true);
            knightSelected.SetActive(true);
        }
    }

    public void ArcherPartner()
    {
        if (!archerPartner.activeSelf)
        {
            if (knightPartner.activeSelf)
            {
                knightPartner.SetActive(false);
                knightPartnerStatusBar.SetActive(false);
                knightPartnerPreview.SetActive(false);
                knightSelected.SetActive(false);
            }

            archerPartner.SetActive(true);
            archerPartnerStatusBar.SetActive(true);
            archerPartnerPreview.SetActive(true);
            archerSelected.SetActive(true);
        }
    }

    public void OtherPartner()
    {
        if (knightPartner.activeSelf)
        {
            knightPartner.SetActive(false);
            knightPartnerStatusBar.SetActive(false);
            knightPartnerPreview.SetActive(false);
            knightSelected.SetActive(false);
        }
        else if(archerPartner.activeSelf)
        {
            archerPartner.SetActive(false);
            archerPartnerStatusBar.SetActive(false);
            archerPartnerPreview.SetActive(false);
            archerSelected.SetActive(false);
        }
    }
}
