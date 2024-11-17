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

        public Sprite portraitImage;
        public Sprite spriteImage;
    }

    public List<Partner> partners = new List<Partner>();
    public List<int> activePartners = new List<int>();
    private const int maxActivePartners = 3;

    private void Awake()
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

        // Animate button to move slightly outward to indicate selection
        StartCoroutine(MoveButton(partners[index].buttonTransform, Vector2.right * 30));
    }

    private void DeselectPartner(int index)
    {
        SetPartnerActive(index, false);
        activePartners.Remove(index);

        if (lineTrigger != null && lineTrigger.isInFusion)
        {
            Transform partnerTransform = partners[index].partnerObject.transform;
            lineTrigger.UnlinkHero(partnerTransform);
        }

        // Animate button back to its original position
        StartCoroutine(MoveButton(partners[index].buttonTransform, Vector2.left * 30));
    }

    private void SetPartnerActive(int index, bool isActive)
    {
        partners[index].partnerObject.SetActive(isActive);
        partners[index].statusBar.SetActive(isActive);
        //partners[index].selectedIndicator.SetActive(isActive);

        if (partners[index].statusComponent != null)
        {
            partners[index].statusComponent.gameObject.SetActive(isActive);
        }
    }

    private IEnumerator MoveButton(RectTransform button, Vector2 targetOffset)
    {
        if (button == null) yield break;

        Vector2 initialPosition = button.anchoredPosition;
        Vector2 targetPosition = initialPosition + targetOffset;
        float duration = 0.2f; // Adjust for faster/slower animation
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            button.anchoredPosition = Vector2.Lerp(initialPosition, targetPosition, elapsed / duration);
            yield return null;
        }

        button.anchoredPosition = targetPosition;
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
