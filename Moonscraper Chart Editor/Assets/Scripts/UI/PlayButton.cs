// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class PlayButton : MonoBehaviour {
    ChartEditor editor;
    Image buttonImage;

    [SerializeField]
    Sprite playSprite;
    [SerializeField]
    Sprite pauseSprite;

    void Start()
    {
        editor = ChartEditor.FindCurrentEditor();
        buttonImage = GetComponent<Image>();
    }

    void Update()
    {
        if (Globals.applicationMode == Globals.ApplicationMode.Playing)
            buttonImage.sprite = pauseSprite;
        else
            buttonImage.sprite = playSprite;
    }

	public void PlayPause()
    {
        if (Globals.applicationMode == Globals.ApplicationMode.Editor)
            editor.Play();
        else
            editor.Stop();
    }
}
