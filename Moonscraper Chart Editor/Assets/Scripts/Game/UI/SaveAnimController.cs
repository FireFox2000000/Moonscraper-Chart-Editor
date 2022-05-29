// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SaveAnimController : UpdateableService {
    public Text saveText;
    ChartEditor editor;

    float alpha = 0;
    public float fadeSpeed = 5;

    delegate void UpdateFn();
    UpdateFn currentUpdateState;

    // Use this for initialization
    protected override void Start () {
        base.Start();
        editor = ChartEditor.Instance;
        editor.events.saveEvent.Register(StartFade);
        saveText.color = new Color(saveText.color.r, saveText.color.g, saveText.color.b, alpha);
    }
	
	// Update is called once per frame
	public override void OnServiceUpdate() {
        currentUpdateState?.Invoke();
    }

    void UpdateFadeIn()
    {
        if (CheckSaveConcluded())
        {
            // Don't bother displaying the save anim for super short saves
            currentUpdateState = UpdateFadeOut;
        }
        else
        {
            alpha += fadeSpeed * Time.deltaTime;
            alpha = Mathf.Clamp01(alpha);
            saveText.color = new Color(saveText.color.r, saveText.color.g, saveText.color.b, alpha);

            if (alpha >= 1.0f)
            {
                currentUpdateState = UpdateWaitingForSaveEnd;
            }
        }
    }

    void UpdateWaitingForSaveEnd()
    {
        if (CheckSaveConcluded())
        {
            currentUpdateState = UpdateFadeOut;
        }
    }

    void UpdateFadeOut()
    {
        alpha -= fadeSpeed * Time.deltaTime;
        alpha = Mathf.Clamp01(alpha);
        saveText.color = new Color(saveText.color.r, saveText.color.g, saveText.color.b, alpha);

        if (alpha <= 0)
        {
            currentUpdateState = null;
        }
    }

    bool CheckSaveConcluded()
    {
        return !editor.isSaving;
    }

    public void StartFade()
    {
        Debug.Log("Save event");
        alpha += fadeSpeed * Time.deltaTime;

        currentUpdateState = UpdateFadeIn;
    }
}
