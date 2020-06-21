// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreenFader : MonoBehaviour {
    public float alphaMax;
    public float fadeSpeed;
    public Text loadingInformation;

    public Image rayCastableImage;

    Image[] images;
    float currentAlpha = 0;
    int fadeDirection = 0;

	// Use this for initialization
	void Start () {
        if (alphaMax > 1)
            alphaMax = 1;

        images = GetComponentsInChildren<Image>();
        setAlpha(0);
    }
	
	// Update is called once per frame
	void Update () {
        currentAlpha += fadeSpeed * Time.deltaTime * fadeDirection;

        if (currentAlpha > alphaMax)
        {
            currentAlpha = alphaMax;
            fadeDirection = 0;
        }
        else if (currentAlpha <= 0)
        {
            currentAlpha = 0;
            fadeDirection = 0;
            rayCastableImage.raycastTarget = false;
            gameObject.SetActive(false);
        }

        setAlpha(currentAlpha);
    }

    void setAlpha(float alpha)
    {
        foreach (Image image in images)
        {
            image.color = new Color(image.color.r, image.color.g, image.color.b, alpha);
        }
        loadingInformation.color = new Color(loadingInformation.color.r, loadingInformation.color.g, loadingInformation.color.b, alpha);
    }

    public void FadeIn()
    {
        gameObject.SetActive(true);
        fadeDirection = 1;
        rayCastableImage.raycastTarget = true;
    }

    public void FadeOut()
    {
        fadeDirection = -1;
    }
}
