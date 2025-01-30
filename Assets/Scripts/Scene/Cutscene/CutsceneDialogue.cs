using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutsceneDialogue : MonoBehaviour
{
    public AdvancedDialogueSO[] conversation;

    private AdvancedDialogueManager advancedDialogueManager;

    //private bool dialogueInitiated;

    // Start is called before the first frame update
    void Start()
    {
        advancedDialogueManager = GameObject.Find("DialogueManager").GetComponent<AdvancedDialogueManager>();
        advancedDialogueManager.InitiateDialogue(this);
        //dialogueInitiated = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
