// Copyright (c) 2016-2020 Alexander Ong
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
    Sprite playSprite = null;
    [SerializeField]
    Sprite pauseSprite = null;

    void Start()
    {
        editor = ChartEditor.Instance;
        buttonImage = GetComponent<Image>();

        editor.events.editorStateChangedEvent.Register(UpdatePlayPauseSprite);
        UpdatePlayPauseSprite(editor.currentState);
    }

	public void PlayPause()
    {
        if (editor.currentState == ChartEditor.State.Editor)
            editor.Play();
        else
            editor.Stop();
    }

    void UpdatePlayPauseSprite(in ChartEditor.State editorState)
    {
        if (editorState == ChartEditor.State.Playing)
            buttonImage.sprite = pauseSprite;
        else
            buttonImage.sprite = playSprite;
    }
}
