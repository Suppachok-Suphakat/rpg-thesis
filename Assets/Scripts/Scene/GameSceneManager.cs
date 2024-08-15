using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneManager : MonoBehaviour
{
    public static GameSceneManager instance;

    string currentScene;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        currentScene = SceneManager.GetActiveScene().name;
    }

    public void SwitchScene(string to, Vector3 transitionPosition)
    {
        SceneManager.LoadScene(to, LoadSceneMode.Additive);
        SceneManager.UnloadSceneAsync(currentScene);
        currentScene = to;
        Transform playerTransform = GameManager.instance.player.transform;
        playerTransform.position = new Vector3(transitionPosition.x, transitionPosition.y, playerTransform.position.z);
    }
}
