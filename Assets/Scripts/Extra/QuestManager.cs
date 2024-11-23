using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public string questName = "Kill 10 Goblins";
    public int targetKillCount = 10;
    private int currentKillCount = 0;

    public delegate void QuestUpdated();
    public event QuestUpdated OnQuestUpdated;

    public delegate void QuestCompleted();
    public event QuestCompleted OnQuestCompleted;

    // Call this method when a goblin is killed
    public void AddKill()
    {
        currentKillCount += 1;
        OnQuestUpdated?.Invoke();

        if (currentKillCount >= targetKillCount)
        {
            CompleteQuest();
        }
    }

    private void CompleteQuest()
    {
        Debug.Log($"Quest '{questName}' Completed!");

        // Example: Trigger something in the game
        GameObject rewardChest = GameObject.Find("RewardChest");
        if (rewardChest != null)
        {
            rewardChest.SetActive(true); // Example: Show a reward chest
        }

        OnQuestCompleted?.Invoke();
    }

    public int GetCurrentKillCount() => currentKillCount;
}

