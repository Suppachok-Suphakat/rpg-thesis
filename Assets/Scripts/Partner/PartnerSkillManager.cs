using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartnerSkillManager : MonoBehaviour
{
    public static PartnerSkillManager Instance;

    public GameObject partnerMenu;

    [System.Serializable]
    public class Partner
    {
        public string name;
        public GameObject partnerObject;
        public GameObject statusBar;
        public GameObject preview;
        public GameObject selectedIndicator;
        public StatusBar statusComponent;
        public GameObject skillBubble;
    }

    public List<Partner> partners = new List<Partner>();
    public List<int> activePartners = new List<int>();
    private const int maxActivePartners = 3;

    void Start()
    {
        Instance = this;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            TogglePartnerMenu();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ExitPartnerMenu();
        }
    }

    public void TogglePartnerMenu()
    {
        bool isActive = partnerMenu.activeSelf;
        partnerMenu.SetActive(!isActive);
        PlayerController.instance.isMenuActive = !isActive;
        Time.timeScale = isActive ? 1 : 0;
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

    public void SelectPartner(int index)
    {
        if (activePartners.Contains(index))
        {
            Debug.Log("Deselect");
            SetPartnerActive(index, false);
            activePartners.Remove(index);
        }
        else
        {
            if (activePartners.Count < maxActivePartners)
            {

                Debug.Log("Select");
                SetPartnerActive(index, true);
                activePartners.Add(index);
            }
            else
            {
                Debug.Log("Cannot activate more than " + maxActivePartners + " partners at once.");
            }
        }
    }

    private void SetPartnerActive(int index, bool isActive)
    {
        partners[index].partnerObject.SetActive(isActive);
        partners[index].statusBar.SetActive(isActive);
        partners[index].preview.SetActive(isActive);
        partners[index].selectedIndicator.SetActive(isActive);

        if (partners[index].statusComponent != null)
        {
            partners[index].statusComponent.gameObject.SetActive(isActive);
        }

        if (partners[index].skillBubble != null)
        {
            partners[index].skillBubble.SetActive(isActive);
        }
    }
}