using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartnerSkillManager : MonoBehaviour
{
    public static PartnerSkillManager Instance;

    public GameObject partnerMenu;
    public Transform statusBarContainer;

    public Image[] portraitBoxes; // Assign three UI Images in the Inspector for portraits
    public Image[] spriteBoxes; // Assign three UI Images in the Inspector for sprites

    public LineTrigger lineTrigger;

    [System.Serializable]
    public class Partner
    {
        public string name;
        public GameObject partnerObject;
        public GameObject statusBar;
        public GameObject selectedIndicator;
        public StatusBar statusComponent;
        public RectTransform buttonTransform; // Reference to the button's RectTransform
        public bool isSelected; // Track if the partner is selected
        public GameObject skillInfoUI;

        public Sprite portraitImage;
        public Sprite spriteImage;
    }
    private Partner hoveredPartner = null;

    public List<Partner> partners = new List<Partner>();
    public List<int> activePartners = new List<int>();
    private const int maxActivePartners = 3;

    private Dictionary<int, float> cooldownTimers = new Dictionary<int, float>();
    public float selectionCooldown = 1.0f; // Cooldown time in seconds

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F) && hoveredPartner != null)
        {
            // Toggle skill info panel
            if (hoveredPartner.skillInfoUI.activeSelf)
            {
                HideSkillInfo(hoveredPartner);
            }
            else
            {
                ShowSkillInfo(hoveredPartner);
            }
        }
    }

    public void OnPartnerButtonHoverEnter(int index)
    {
        if (index >= 0 && index < partners.Count)
        {
            hoveredPartner = partners[index];
        }
    }

    public void OnPartnerButtonHoverExit()
    {
        if (hoveredPartner != null)
        {
            HideSkillInfo(hoveredPartner);
        }

        hoveredPartner = null;
    }

    private void ShowSkillInfo(Partner partner)
    {
        // Hide all other skill info UIs
        foreach (var p in partners)
        {
            p.skillInfoUI.SetActive(false);
        }

        // Show the selected partner's skill info UI
        partner.skillInfoUI.SetActive(true);
    }

    private void HideSkillInfo(Partner partner)
    {
        partner.skillInfoUI.SetActive(false);
    }

    public void TogglePartnerMenu()
    {
        bool isActive = partnerMenu.activeSelf;
        partnerMenu.SetActive(!isActive);
        PlayerController.instance.isMenuActive = !isActive;

        // Apply saved button states when menu is opened
        if (!isActive)
        {
            ApplyPartnerButtonStates();
        }
    }

    public void ExitPartnerMenu()
    {
        if (partnerMenu.activeSelf)
        {
            PlayerController.instance.isMenuActive = false;
            partnerMenu.SetActive(false);
        }
    }

    public void SelectPartner(int index)
    {
        // Check if the partner is on cooldown
        if (cooldownTimers.ContainsKey(index) && Time.time < cooldownTimers[index])
        {
            Debug.Log($"Partner {partners[index].name} is on cooldown.");
            return;
        }

        // Update cooldown timer
        cooldownTimers[index] = Time.time + selectionCooldown;

        if (activePartners.Contains(index))
        {
            DeselectPartner(index);
        }
        else
        {
            if (activePartners.Count < maxActivePartners)
            {
                ActivatePartner(index);
            }
            else
            {
                Debug.Log("Cannot activate more than " + maxActivePartners + " partners at once.");
            }
        }

        UpdatePortraitAndSpriteBoxes();
    }

    private void ActivatePartner(int index)
    {
        SetPartnerActive(index, true);
        activePartners.Add(index);
        partners[index].isSelected = true; // Mark partner as selected

        // Trigger move animation
        Animator buttonAnimator = partners[index].buttonTransform.GetComponent<Animator>();
        if (buttonAnimator != null)
        {
            buttonAnimator.SetBool("IsSelected", true);
        }
    }

    private void DeselectPartner(int index)
    {
        SetPartnerActive(index, false);
        activePartners.Remove(index);
        partners[index].isSelected = false; // Mark partner as deselected

        // Trigger move back animation
        Animator buttonAnimator = partners[index].buttonTransform.GetComponent<Animator>();
        if (buttonAnimator != null)
        {
            buttonAnimator.SetBool("IsSelected", false);
        }
    }

    private void SetPartnerActive(int index, bool isActive)
    {
        partners[index].partnerObject.SetActive(isActive);
        partners[index].statusBar.SetActive(isActive);

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

    private void ApplyPartnerButtonStates()
    {
        foreach (var partner in partners)
        {
            Animator buttonAnimator = partner.buttonTransform.GetComponent<Animator>();
            if (buttonAnimator != null)
            {
                buttonAnimator.SetBool("IsSelected", partner.isSelected);
            }
        }
    }
}
