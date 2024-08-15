using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AdvancedDialogueManager : MonoBehaviour
{
    //NPC
    private AdvancedDialogueSO currentConversation;
    private int stepNum;
    private bool dialogueActivated;

    //UI
    public GameObject dialogueCanvas;
    public TMP_Text actor;
    public Image portrait;
    public TMP_Text dialogueText;
    public GameObject shopMenuCanvas;

    private string currentSpeaker;
    private Sprite currentPortrait;

    public ActorSO[] actorSO;

    //Animation
    public GameObject dialogueBox;
    [SerializeField] Animator animator;

    //Button
    public GameObject[] optionButton;
    public TMP_Text[] optionButtonText;
    public GameObject optionPanel;

    //Type
    [SerializeField]
    private float typingSpeed;
    private Coroutine typeWriterRoutine;
    private bool canContinueText = true;
    private bool shouldSkipDialogue;

    //HUD
    public GameObject statusCanvas;
    public GameObject inventoryCanvas;
    public GameObject PartnerCanvas;

    //Player
    private PlayerController playerController;

    // Start is called before the first frame update
    void Start()
    {
        playerController = GameObject.Find("Player").GetComponent<PlayerController>();

        animator = dialogueBox.GetComponent<Animator>();

        optionButtonText = new TMP_Text[optionButton.Length];

        for (int i = 0; i < optionButton.Length; i++)
            optionButtonText[i] = optionButton[i].GetComponentInChildren<TMP_Text>();

        for (int i = 0; i < optionButton.Length; i++)
            optionButton[i].SetActive(false);


        dialogueCanvas.SetActive(false);
        optionPanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (dialogueActivated && Input.GetButtonDown("Interact") && canContinueText)
        {
            statusCanvas.SetActive(false);
            inventoryCanvas.SetActive(false);
            PartnerCanvas.SetActive(false);

            playerController.enabled = false;

            if (stepNum >= currentConversation.actors.Length)
            {
                TurnOffDialogue();
            }
            else
            {
                PlayDialogue();
            }
        }

        if (Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            shouldSkipDialogue = true;
        }
    }

    void PlayDialogue()
    {
        if (currentConversation.actors[stepNum] == DialogueActors.Random)
            SetActorInfo(false);
        else
            SetActorInfo(true);

        actor.text = currentSpeaker;
        portrait.sprite = currentPortrait;

        if (currentConversation.actors[stepNum] == DialogueActors.Branch)
        {
            for (int i = 0; i < currentConversation.optionText.Length; i++)
            {
                if (currentConversation.optionText[i] == null)
                    optionButton[i].SetActive(false);
                else
                {
                    optionButtonText[i].text = currentConversation.optionText[i];
                    optionButton[i].SetActive(true);
                    canContinueText = false;
                }
            }
        }

        if (currentConversation.actors[stepNum] == DialogueActors.Shop)
        {
            TurnOffDialogue();
            ActivateShopMenu();
            return;
        }

        if (typeWriterRoutine != null)
            StopCoroutine(typeWriterRoutine);

        if(stepNum < currentConversation.dialogue.Length)
            typeWriterRoutine = StartCoroutine(TypeWriterEffect(dialogueText.text = currentConversation.dialogue[stepNum]));
        else
            optionPanel.SetActive(true);
            
        dialogueCanvas.SetActive(true);
        stepNum++;
        animator.Play("DialogueEntry");
    }

    void SetActorInfo(bool recurringCharacter)
    {
        if (recurringCharacter)
        {
            for (int i = 0; i < actorSO.Length; i++)
            {
                if (actorSO[i].name == currentConversation.actors[stepNum].ToString())
                {
                    currentSpeaker = actorSO[i].actorName;
                    currentPortrait = actorSO[i].actorPortrait;
                }
            }
        }
        else
        {
            currentSpeaker = currentConversation.randomActorName;
            currentPortrait = currentConversation.randomActorPortrait;
        }
    }

    public void Option(int optionNum)
    {
        foreach (GameObject button in optionButton)
        {
            canContinueText = true;
            button.SetActive(false);
        }

        if (optionNum == 0)
            currentConversation = currentConversation.option0;
        if (optionNum == 1)
            currentConversation = currentConversation.option1;
        if (optionNum == 2)
            currentConversation = currentConversation.option2;
        if (optionNum == 3)
            currentConversation = currentConversation.option3;

        stepNum = 0;
    }

    private IEnumerator TypeWriterEffect(string line)
    {
        dialogueText.text = "";
        canContinueText = false;
        bool addingRichTextTag = false;
        yield return new WaitForSeconds(.1f);
        foreach (char letter in line.ToCharArray())
        {
            if (shouldSkipDialogue)
            {
                Debug.Log("Skip");
                dialogueText.text = line;
                shouldSkipDialogue = false;
                break;
            }

            //Rich Text
            if(letter == '<' || addingRichTextTag)
            {
                addingRichTextTag = true;
                dialogueText.text += letter;
                if (letter == '>')
                    addingRichTextTag = false;
            }
            else
            {
                dialogueText.text += letter;
                //yield return new WaitForSeconds(typingSpeed);
            }
        }
        canContinueText = true;
    }

    public void InitiateDialogue(NPCDialogue npcDialogue)
    {
        currentConversation = npcDialogue.conversation[0];
        
        dialogueActivated = true;
    }

    public void InitiateDialogue(NPCMerchant npcMerchant)
    {
        currentConversation = npcMerchant.conversation[0];

        dialogueActivated = true;
    }

    public void TurnOffDialogue()
    {
        stepNum = 0;

        dialogueActivated = false;
        optionPanel.SetActive(false);
        dialogueCanvas.SetActive(false);

        statusCanvas.SetActive(true);
        inventoryCanvas.SetActive(true);
        PartnerCanvas.SetActive(true);

        playerController.enabled = true;
    }

    public void ActivateShopMenu()
    {
        PlayerController.instance.isMenuActive = true;
        Time.timeScale = 0;
        shopMenuCanvas.SetActive(true);
    }
}
public enum DialogueActors
{
    Lunar,
    Random,
    Branch,
    Shop
};
