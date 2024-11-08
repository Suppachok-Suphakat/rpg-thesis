using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartnerSkillManager : MonoBehaviour
{
    public static PartnerSkillManager Instance;

    public GameObject partnerMenu;
    public Transform statusBarContainer; // Drag your StatusBarContainer (VerticalLayoutGroup) here in the Inspector.
    //public Transform previewContainer; // Drag your PreviewContainer (HorizontalLayoutGroup) here in the Inspector.

    // Arrays to hold the portrait and sprite boxes for the selected partners
    public Image[] portraitBoxes; // Assign three UI Images in the Inspector for portraits
    public Image[] spriteBoxes; // Assign three UI Images in the Inspector for sprites

    public LineTrigger lineTrigger;

    [System.Serializable]
    public class Partner
    {
        public string name;
        public GameObject partnerObject;
        public GameObject statusBar;
        public GameObject preview;
        public GameObject selectedIndicator;
        public StatusBar statusComponent;

        public Sprite portraitImage;
        public Sprite spriteImage;
    }

    public List<Partner> partners = new List<Partner>();
    public List<int> activePartners = new List<int>();
    private const int maxActivePartners = 3;

    private void Awake()
    {
        Debug.Log("PartnerSkillManager instance set");
        Instance = this;
    }

    void Start()
    {

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
        Debug.Log("Testing SelectPartner: Hero index " + index + " clicked");

        if (activePartners.Contains(index))
        {
            Debug.Log("Deselect");
            if (lineTrigger != null && lineTrigger.isInFusion)
            {
                Transform partnerTransform = partners[index].partnerObject.transform;
                lineTrigger.UnlinkHero(partnerTransform);
            }

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
                partners[index].statusBar.transform.SetParent(null);
                partners[index].statusBar.transform.SetParent(statusBarContainer);

                // Move the selected partner's preview to the end of the preview container
                //partners[index].preview.transform.SetParent(null);
                //partners[index].preview.transform.SetParent(previewContainer);
            }
            else
            {
                Debug.Log("Cannot activate more than " + maxActivePartners + " partners at once.");
            }
        }

        UpdatePortraitAndSpriteBoxes();
    }

    private void SetPartnerActive(int index, bool isActive)
    {
        partners[index].partnerObject.SetActive(isActive);
        partners[index].statusBar.SetActive(isActive);
        //partners[index].preview.SetActive(isActive);
        partners[index].selectedIndicator.SetActive(isActive);

        if (partners[index].statusComponent != null)
        {
            partners[index].statusComponent.gameObject.SetActive(isActive);
        }
    }

    private void UpdatePortraitAndSpriteBoxes()
    {
        // Clear all portrait and sprite boxes
        for (int i = 0; i < maxActivePartners; i++)
        {
            portraitBoxes[i].sprite = null;
            portraitBoxes[i].enabled = false;
            spriteBoxes[i].sprite = null;
            spriteBoxes[i].enabled = false;
        }

        // Set images for each active partner in order
        for (int i = 0; i < activePartners.Count; i++)
        {
            int partnerIndex = activePartners[i];
            if (portraitBoxes[i] != null && partners[partnerIndex].portraitImage != null)
            {
                portraitBoxes[i].sprite = partners[partnerIndex].portraitImage;
                portraitBoxes[i].enabled = true;
            }

            if (spriteBoxes[i] != null && partners[partnerIndex].spriteImage != null)
            {
                spriteBoxes[i].sprite = partners[partnerIndex].spriteImage;
                spriteBoxes[i].enabled = true;
            }
        }
    }
}
