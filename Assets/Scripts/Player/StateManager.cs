using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;



public enum GameState { FreeRoam, Dialog, Menu };
public class GameController : MonoBehaviour
{
    [SerializeField] PlayerController playerController;

    GameState state;

    private void Start()
    {
        DialogManager.instance.OnShowDialog += () =>
        {
            state = GameState.Dialog;
        };
        DialogManager.instance.OnHideDialog += () =>
        {
            if (state == GameState.Dialog)
            {
                state = GameState.FreeRoam;
            }
        };
    }

    private void Update()
    {
        if (state == GameState.FreeRoam)
        {
            //playerController.HandleUpdate();
        }
        else if (state == GameState.Dialog)
        {
            DialogManager.instance.HandleUpdate();
        }
        else if (state == GameState.Menu)
        {

        }
    }
}
