using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ConversationManager : MonoBehaviour
{
    public GameObject conversationBox;      // The conversation box UI element
    public TMP_Text conversationText;       // The TMP_Text component for displaying dialogue
    public Image heroFaceImage;             // The image component for displaying the hero's face

    private float displayDuration = 2f;     // Duration to display the conversation box

    private void Start()
    {
        conversationBox.SetActive(false);   // Hide the conversation box at the start
    }

    public void ShowConversation(string text, Sprite heroFace)
    {
        conversationText.text = text;
        heroFaceImage.sprite = heroFace;
        conversationBox.SetActive(true);
        StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(displayDuration);
        conversationBox.SetActive(false);
    }
}
