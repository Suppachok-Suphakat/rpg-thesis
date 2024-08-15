using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public PlayerController player;

    public OnScreenMessageSystem messageSystem;

    private void Awake()
    {
        instance = this;

        player = FindObjectOfType<PlayerController>();
    }
}
