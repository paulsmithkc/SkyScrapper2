using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class HudFade : MonoBehaviour
{
    public Image fadeImage;
    public Text fadeText;
    public Image reticle;
    public float defaultFadeTime = 4;
    public Color firstColor = Color.black;

    private Color startColor;
    private Fade currentFade;
    private float fadeTimeLeft;

    public class Fade
    {
        public Color targetColor;
        public float fadeTime;
    }

    private Queue<Fade> fades = new Queue<Fade>();

    // Use this for initialization
    void Start()
    {
        startColor = firstColor;
        fadeImage.color = startColor;
        fadeImage.enabled = true;
        fadeImage.gameObject.SetActive(true);
        FadeToClear();
    }

    // Update is called once per frame
    void Update()
    {
        if (currentFade != null || fades.Count > 0)
        {
            if (currentFade == null)
            {
                currentFade = fades.Dequeue();
                fadeTimeLeft = currentFade.fadeTime;
            }

            float deltaTime = Time.deltaTime;
            if (fadeTimeLeft > deltaTime)
            {
                fadeTimeLeft -= deltaTime;
                fadeImage.color = Color.Lerp(currentFade.targetColor, startColor, fadeTimeLeft / currentFade.fadeTime);
                fadeImage.enabled = true;
            }
            else if (fades.Count > 0)
            {
                //	Debug.Log("Fade Completed");
                startColor = currentFade.targetColor;
                fadeImage.color = startColor;
                fadeImage.enabled = true;
                currentFade = fades.Dequeue();
                fadeTimeLeft = currentFade.fadeTime;
            }
            else
            {
                //	Debug.Log("Fade Completed");
                startColor = currentFade.targetColor;
                fadeImage.color = startColor;
                fadeImage.enabled = currentFade.targetColor.a > 0.0f;
                currentFade = null;
                fadeTimeLeft = 0.0f;
            }
        }
    }

    public void FadeTo(Color c, float fadeTime)
    {
        fades.Enqueue(new Fade()
        {
            targetColor = c,
            fadeTime = fadeTime
        });
    }

    public void FadeToClear()
    {
        FadeTo(Color.clear, defaultFadeTime);
    }

    public void FadeToWhite()
    {
        FadeTo(Color.white, defaultFadeTime);
    }

    public void FadeToBlack()
    {
        FadeTo(Color.black, defaultFadeTime);
    }
}