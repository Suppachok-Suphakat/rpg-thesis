using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScreenFader : MonoBehaviour
{
    public Image fadeScreen; // Assign in inspector
    public float fadeDuration = 1f;

    private void Start()
    {
        if (fadeScreen != null)
        {
            SetAlpha(0); // Start fully transparent
        }
    }

    public void FadeIn(System.Action onComplete = null)
    {
        StartCoroutine(Fade(0, 1, onComplete)); // Fade to black
    }

    public void FadeOut(System.Action onComplete = null)
    {
        StartCoroutine(Fade(1, 0, onComplete)); // Fade to transparent
    }


    private IEnumerator Fade(float startAlpha, float endAlpha, System.Action onComplete)
    {
        float timer = 0f;
        Color color = fadeScreen.color;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float t = timer / fadeDuration;
            color.a = Mathf.Lerp(startAlpha, endAlpha, t);
            fadeScreen.color = color;
            yield return null;
        }

        color.a = endAlpha;
        fadeScreen.color = color;

        onComplete?.Invoke();
    }

    private void SetAlpha(float alpha)
    {
        Color color = fadeScreen.color;
        color.a = alpha;
        fadeScreen.color = color;
    }
}
