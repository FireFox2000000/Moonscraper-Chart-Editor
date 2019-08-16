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
        editor = ChartEditor.Instance;
        buttonImage = GetComponent<Image>();

        EventsManager.onApplicationModeChangedEventList.Add(UpdatePlayPauseSprite);
        UpdatePlayPauseSprite(Globals.applicationMode);
    }

	public void PlayPause()
    {
        if (Globals.applicationMode == Globals.ApplicationMode.Editor)
            editor.Play();
        else
            editor.Stop();
    }

    void UpdatePlayPauseSprite(Globals.ApplicationMode applicationMode)
    {
        if (applicationMode == Globals.ApplicationMode.Playing)
            buttonImage.sprite = pauseSprite;
        else
            buttonImage.sprite = playSprite;
    }
}
