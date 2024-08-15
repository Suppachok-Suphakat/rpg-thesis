using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogManager : MonoBehaviour
{
    public static DialogManager instance { get; private set; }

    [SerializeField] GameObject dialogBox;

    public Image charecterPortrait;
    public TextMeshProUGUI charecterName;
    public TextMeshProUGUI dialogArea;

    [SerializeField] int letterPerSecond;

    public event Action OnShowDialog;
    public event Action OnHideDialog;

    [SerializeField] Animator animator;

    private void Awake()
    {
        instance = this;
    }

    Dialog dialog;
    int currentLine = 0;
    public bool isTyping;

    public IEnumerator ShowDialog(Dialog dialog)
    {
        yield return new WaitForEndOfFrame();
        OnShowDialog?.Invoke();

        this.dialog = dialog;

        PlayerController.instance.isMenuActive = true;
        dialogBox.SetActive(true);
        animator.Play("DialogueEntry");
        StartCoroutine(TypeDialog(dialog.DialogueLines[0]));
    }

    public void HandleUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isTyping || 
            Input.GetKeyDown(KeyCode.KeypadEnter) && !isTyping ||
            Input.GetKeyDown(KeyCode.Return) && !isTyping ||
            (Input.GetMouseButtonDown(0) && !isTyping))
        {
            ++currentLine;
            if(currentLine < dialog.DialogueLines.Count)
            {
                StartCoroutine(TypeDialog(dialog.DialogueLines[currentLine]));
            }
            else
            {
                animator.Play("DialogueExit");
                currentLine = 0;
                OnDialogueExitComplete();
                OnHideDialog?.Invoke();
            }
        }
    }

    public IEnumerator TypeDialog(DialogueLine dialogLine)
    {
        isTyping = true;
        dialogArea.text = "";
        charecterPortrait.sprite = dialogLine.charecter.icon;
        charecterName.text = dialogLine.charecter.name;
        foreach (char letter in dialogLine.line.ToCharArray())
        {
            dialogArea.text += letter;
            yield return new WaitForSeconds(1f / letterPerSecond);
        }
        isTyping = false;
    }

    public void OnDialogueExitComplete()
    {
        PlayerController.instance.isMenuActive = false;
        dialogBox.SetActive(false);
    }
}
