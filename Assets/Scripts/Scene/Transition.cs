using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum Transitiontype
{
    Warp,
    Scene
}

public class Transition : MonoBehaviour
{
    [SerializeField] Transitiontype transitionType;
    [SerializeField] string sceneNameToTransition;
    [SerializeField] Vector3 transitionPosition;

    Transform destination;

    void Start()
    {
        destination = transform.GetChild(1);
    }

    internal void InitiateTransition(Transform toTransition)
    {
        switch (transitionType)
        {
            case Transitiontype.Warp:
                Debug.Log("Warp!");
                toTransition.GetComponent<PlayerController>().Warp(destination.position);
                //toTransition.position = new Vector3(destination.position.x, destination.position.y, toTransition.position.z);
                break; 
            case Transitiontype.Scene:
                GameSceneManager.instance.SwitchScene(sceneNameToTransition, transitionPosition);
                break;
        }
    }
}
