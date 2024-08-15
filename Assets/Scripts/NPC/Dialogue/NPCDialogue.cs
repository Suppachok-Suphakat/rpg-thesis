using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCDialogue : MonoBehaviour
{
    public AdvancedDialogueSO[] conversation;

    private SpriteRenderer speechBubbleRenderer;

    private AdvancedDialogueManager advancedDialogueManager;

    private bool dialogueInitiated;

    // Start is called before the first frame update
    void Start()
    {
        advancedDialogueManager = GameObject.Find("DialogueManager").GetComponent<AdvancedDialogueManager>();
        speechBubbleRenderer = GetComponent<SpriteRenderer>();
        speechBubbleRenderer.enabled = false;
    }

    void Update()
    {
        if (dialogueInitiated)
        {
            FlipSprite();
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Player" && !dialogueInitiated)
        {
            speechBubbleRenderer.enabled = true;

            advancedDialogueManager.InitiateDialogue(this);
            dialogueInitiated = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            speechBubbleRenderer.enabled = false;

            advancedDialogueManager.TurnOffDialogue();
            dialogueInitiated = false;
        }
    }

    void FlipSprite()
    {
        if (PlayerController.instance.transform.position.x < transform.parent.position.x)
            transform.parent.localScale = new Vector3(-1, 1, 1);
        else
            transform.parent.localScale = new Vector3(1, 1, 1);
    }
}
