using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroUnlock : MonoBehaviour
{
    private bool heroMenuInitiated;
    private bool playerCheck;
    public int heroNum;
    //public GameObject canvas;

    void Update()
    {
        // Open the partner menu if F is pressed and we are in range of the trigger
        if (Input.GetKeyDown(KeyCode.F) && playerCheck)
        {
            PartnerSkillManager.Instance.UnlockPartner(heroNum);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && !heroMenuInitiated)
        {
            playerCheck = true;
            //canvas.SetActive(true);  // Show the canvas (indicating interaction available)
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerCheck = false;
            //canvas.SetActive(false);  // Hide the canvas (indicating interaction is no longer available)
        }
    }
}
