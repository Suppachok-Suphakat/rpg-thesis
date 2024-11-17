using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum Transitiontype
{
    Warp,
    Scene
}

public class Transition : MonoBehaviour
{
    public GameObject canvas;
    public ScreenFader screenFader; // Assign in the inspector
    private bool heroMenuInitiated;
    private bool playerCheck;

    [SerializeField] Transitiontype transitionType;
    [SerializeField] string sceneNameToTransition;
    [SerializeField] Vector3 transitionPosition;

    Transform destination;
    Transform playerTransform; // Reference to the player's Transform

    void Start()
    {
        destination = transform.GetChild(0);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F) && !heroMenuInitiated && playerCheck && playerTransform != null)
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
        InitiateTransition(playerTransform);

        if (screenFader != null)
        {
            // Fade back to gameplay
            screenFader.FadeOut();
            yield return new WaitForSeconds(screenFader.fadeDuration);
        }
    }

    internal void InitiateTransition(Transform toTransition)
    {
        switch (transitionType)
        {
            case Transitiontype.Warp:
                Debug.Log("Warp!");
                toTransition.GetComponent<PlayerController>().Warp(destination.position);
                break;
            case Transitiontype.Scene:
                GameSceneManager.instance.SwitchScene(sceneNameToTransition, transitionPosition);
                break;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player" && !heroMenuInitiated)
        {
            playerCheck = true;
            playerTransform = collision.transform; // Store the player's Transform
            canvas.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            playerCheck = false;
            playerTransform = null; // Clear the reference when the player exits
            canvas.SetActive(false);
        }
    }
}
