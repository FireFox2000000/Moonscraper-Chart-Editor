// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Image))]
public class ImageFade : MonoBehaviour {
    Image image;

    public float opacity = 1;
    public bool completed
    {
        get; private set;
    }

    public bool fadeInRunning { get; private set; }
    public bool fadeOutRunning { get; private set; }

    // Use this for initialization
    void Start () {
        image = GetComponent<Image>();
        completed = true;
        fadeInRunning = false;
        fadeOutRunning = false;
    }
	
	// Update is called once per frame
	void Update () {
        Color color = image.color;
        color.a = opacity;
        image.color = color;
	}

    public IEnumerator fadeOut(float fadeSpeed = 0.5f, float delay = 0)
    {
        completed = false;
        opacity = 0;
        fadeOutRunning = true;
        fadeInRunning = false;

        yield return new WaitForSeconds(delay);
        float startTime = Time.realtimeSinceStartup;

        while (opacity < 1 && fadeOutRunning)
        {
            opacity = fadeSpeed * (Time.realtimeSinceStartup - startTime);

            yield return null;
        }
        opacity = 1;
        completed = true;

        Time.timeScale = 1;
    }

    public IEnumerator fadeIn(float fadeSpeed = 0.5f, float delay = 0)
    {
        completed = false;
        opacity = 1;
        fadeOutRunning = false;
        fadeInRunning = true;

        yield return new WaitForSeconds(delay);
        float startTime = Time.realtimeSinceStartup;
        while (opacity > 0 && fadeInRunning)
        {
            opacity = 1 - (fadeSpeed * (Time.realtimeSinceStartup - startTime));
            
            yield return null;
        }
        opacity = 0;
        completed = true;
    }
}
