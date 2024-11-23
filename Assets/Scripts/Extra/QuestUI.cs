using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestUI : MonoBehaviour
{
    public TextMeshProUGUI questTMPText; // Reference to TMP text component
    private QuestManager questManager;

    private void Start()
    {
        questManager = FindObjectOfType<QuestManager>();
        if (questManager != null)
        {
            questManager.OnQuestUpdated += UpdateUI;
            questManager.OnQuestCompleted += ShowCompletionMessage;
        }
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (questTMPText != null)
        {
            questTMPText.text = $"{questManager.questName}: {questManager.GetCurrentKillCount()}/{questManager.targetKillCount}";
        }
    }

    private void ShowCompletionMessage()
    {
        if (questTMPText != null)
        {
            questTMPText.text = $"Quest Completed: {questManager.questName}!";
        }
    }
}
