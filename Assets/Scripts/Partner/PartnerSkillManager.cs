using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartnerSkillManager : MonoBehaviour
{
    public static PartnerSkillManager Instance;

    public GameObject partnerMenu;
    public Transform statusBarContainer; // Drag your StatusBarContainer (VerticalLayoutGroup) here in the Inspector.
    public Transform previewContainer; // Drag your PreviewContainer (HorizontalLayoutGroup) here in the Inspector.

    [System.Serializable]
    public class Partner
    {
        public string name;
        public GameObject partnerObject;
        public GameObject statusBar;
        public GameObject preview;
        public GameObject selectedIndicator;
        public StatusBar statusComponent;
    }

    public List<Partner> partners = new List<Partner>();
    public List<int> activePartners = new List<int>();
    private const int maxActivePartners = 3;

    void Start()
    {
        Instance = this;
    }

    public void TogglePartnerMenu()
    {
        bool isActive = partnerMenu.activeSelf;
        partnerMenu.SetActive(!isActive);
        PlayerController.instance.isMenuActive = !isActive;
        Time.timeScale = isActive ? 1 : 0;
    }

    public void ExitPartnerMenu()
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

                // Move the selected partner's status bar to the end of the status bar container
                partners[index].statusBar.transform.SetParent(null); // Temporarily detach
                partners[index].statusBar.transform.SetParent(statusBarContainer); // Reattach at end

                // Move the selected partner's preview to the end of the preview container
                partners[index].preview.transform.SetParent(null); // Temporarily detach
                partners[index].preview.transform.SetParent(previewContainer); // Reattach at end
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
    }
}
