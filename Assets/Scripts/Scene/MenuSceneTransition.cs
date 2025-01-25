using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuSceneTransition : MonoBehaviour
{
    public GameObject canvas;
    public ScreenFader screenFader; // Assign in the inspector
    private bool heroMenuInitiated;
    private bool playerCheck;

    [SerializeField] string sceneNameToTransition;

    void Start()
    {
        
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F) && !heroMenuInitiated && playerCheck)
        {
            StartCoroutine(HandleTransition());
        }
    }

    private IEnumerator HandleTransition()
    {
        if (screenFader != null)
        {
            // Fade to black
            screenFader.FadeIn();
            yield return new WaitForSeconds(screenFader.fadeDuration);
        }

        // Perform the transition
        InitiateTransition();

        if (screenFader != null)
        {
            // Fade back to gameplay
            screenFader.FadeOut();
            yield return new WaitForSeconds(screenFader.fadeDuration);
        }
    }

    internal void InitiateTransition()
    {
        SceneManager.LoadSceneAsync(sceneNameToTransition);
        //GameSceneManager.instance.SwitchMenuScene(sceneNameToTransition);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player" && !heroMenuInitiated)
        {
            playerCheck = true;
            canvas.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            playerCheck = false;
            canvas.SetActive(false);
        }
    }
}
