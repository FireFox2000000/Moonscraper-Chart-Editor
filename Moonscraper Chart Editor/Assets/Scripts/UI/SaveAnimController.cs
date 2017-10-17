// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SaveAnimController : MonoBehaviour {
    public Text saveText;
    ChartEditor editor;

    bool fadein = false;
    float alpha = 0;
    public float fadeSpeed = 5;

	// Use this for initialization
	void Start () {
        editor = ChartEditor.FindCurrentEditor();
	}
	
	// Update is called once per frame
	void Update () {
        if (alpha >= 1)
            fadein = false;

        if (fadein)
            alpha += fadeSpeed * Time.deltaTime;
        else
            alpha -= fadeSpeed * Time.deltaTime;

        alpha = Mathf.Clamp01(alpha);

        saveText.color = new Color(saveText.color.r, saveText.color.g, saveText.color.b, alpha); 
	}

    public void StartFade()
    {
        fadein = true;
        alpha += fadeSpeed * Time.deltaTime;
    }
}
