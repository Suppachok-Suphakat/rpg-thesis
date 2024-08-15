using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueCharecter
{
    public string name;
    public Sprite icon;
}

[System.Serializable]
public class DialogueLine
{
    public DialogueCharecter charecter;
    [TextArea(3, 10)]
    public string line;
}

[System.Serializable]
public class Dialog
{
    [SerializeField] List<DialogueLine> dialogueLines;

    public List<DialogueLine> DialogueLines
    {
        get { return dialogueLines; }
    }
}
