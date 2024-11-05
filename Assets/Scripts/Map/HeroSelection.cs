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

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F) && !heroMenuInitiated && playerCheck)
        {
            PartnerSkillManager.Instance.TogglePartnerMenu();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PartnerSkillManager.Instance.ExitPartnerMenu();
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player" && !heroMenuInitiated)
        {
            //speechBubbleRenderer.enabled = true;

            playerCheck = true;
            canvas.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            //speechBubbleRenderer.enabled = false;

            playerCheck = false;
            canvas.SetActive(false);
        }
    }
}
