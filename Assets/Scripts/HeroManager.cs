using System;
using System.Collections.Generic;
using UnityEngine;

public class HeroManager : MonoBehaviour
{
    public static HeroManager Instance;
    public List<int> unlockedPartners = new List<int>();

    private const string UnlockedKey = "UnlockedPartners";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SaveUnlockedPartners()
    {
        string data = string.Join(",", unlockedPartners);
        PlayerPrefs.SetString(UnlockedKey, data);
        PlayerPrefs.Save();
    }

    public void LoadUnlockedPartners()
    {
        if (PlayerPrefs.HasKey(UnlockedKey))
        {
            string data = PlayerPrefs.GetString(UnlockedKey);
            unlockedPartners = new List<int>(Array.ConvertAll(data.Split(','), int.Parse));
        }
    }
}