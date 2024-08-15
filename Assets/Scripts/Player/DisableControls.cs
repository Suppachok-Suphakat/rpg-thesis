using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableControls : MonoBehaviour
{
    PlayerController controller;

    private void Awake()
    {
        controller = GetComponent<PlayerController>();
    }

    public void DisableControl()
    {
        controller.enabled = false;
    }

    public void EnableControl()
    {
        controller.enabled = true;
    }
}
