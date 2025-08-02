using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeController : MonoBehaviour
{
    [SerializeField] private GameObject imageGameObject;
    [SerializeField] private Image fadeImage;
    public float fadeDuration = 1f;
    [SerializeField] private GameObject eventSystem;

    private void Awake()
    {
        DontDestroyOnLoad(imageGameObject);
        DontDestroyOnLoad(eventSystem);
        if (fadeImage != null)
        {
            Color color = fadeImage.color;
            color.a = 0;
            fadeImage.color = color;
        }
    }

    public void FadeInNoDelay()
    {
        StartCoroutine(FadeInRoutine(0.1f));
    }

    public void FadeOutNoDelay()
    {
        StartCoroutine(FadeOutRoutine(0.1f));
    }

    public void FadeIn()
    {
        StartCoroutine(FadeInRoutine(fadeDuration));
    }

    public void FadeOut()
    {
        StartCoroutine(FadeOutRoutine(fadeDuration));
    }

    private IEnumerator FadeOutRoutine(float duration)
    {
        float elapsedTime = 0f;
        Color color = fadeImage.color;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Clamp01(elapsedTime / duration);
            fadeImage.color = color;
            yield return null;
        }
    }

    private IEnumerator FadeInRoutine(float duration)
    {
        float elapsedTime = 0f;
        Color color = fadeImage.color;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Clamp01(1 - (elapsedTime / duration));
            fadeImage.color = color;
            yield return null;
        }
    }
}
