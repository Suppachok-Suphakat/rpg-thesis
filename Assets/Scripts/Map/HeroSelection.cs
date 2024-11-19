using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class HeroSelection : MonoBehaviour
{
    private bool heroMenuInitiated;
    private bool playerCheck;
    public GameObject canvas;

    void Update()
    {
        // Open the partner menu if F is pressed and we are in range of the trigger
        if (Input.GetKeyDown(KeyCode.F) && !heroMenuInitiated && playerCheck)
        {
            PartnerSkillManager.Instance.TogglePartnerMenu();  // Open partner menu
            heroMenuInitiated = true;  // Mark that the menu was initiated
        }

        // Close the partner menu if Escape is pressed
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PartnerSkillManager.Instance.ExitPartnerMenu();  // Exit the partner menu
            heroMenuInitiated = false;  // Reset menu initiated status
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && !heroMenuInitiated)
        {
            playerCheck = true;
            canvas.SetActive(true);  // Show the canvas (indicating interaction available)
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerCheck = false;
            canvas.SetActive(false);  // Hide the canvas (indicating interaction is no longer available)
        }
    }
}
