using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    public Image fadePanel;
    public float fadeSpeed = 1f;

    void Start()
    {
        fadePanel.canvasRenderer.SetAlpha(1f);
        StartCoroutine(FadeIn());
    }

    public void LoadScene(string sceneName)
    {
        StartCoroutine(FadeOut(sceneName));
    }

    IEnumerator FadeIn()
    {
        fadePanel.CrossFadeAlpha(0f, fadeSpeed, false);
        yield return new WaitForSeconds(fadeSpeed);
    }

    IEnumerator FadeOut(string sceneName)
    {
        fadePanel.CrossFadeAlpha(1f, fadeSpeed, false);
        yield return new WaitForSeconds(fadeSpeed);
        SceneManager.LoadScene(sceneName);
    }
}
