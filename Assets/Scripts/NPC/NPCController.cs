using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NPCController : MonoBehaviour, Interactable
{
    [SerializeField] Dialog dialog;
    public TextMeshPro interactText;

    private void Start()
    {
        interactText.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Show the text when the player enters any NPC's collider
            interactText.enabled = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Hide the text when the player exits any NPC's collider
            interactText.enabled = false;
        }
    }

    public void Interact()
    {
        if (!DialogManager.instance.isTyping)
        {
            StartCoroutine(DialogManager.instance.ShowDialog(dialog));
        }
    }
}
