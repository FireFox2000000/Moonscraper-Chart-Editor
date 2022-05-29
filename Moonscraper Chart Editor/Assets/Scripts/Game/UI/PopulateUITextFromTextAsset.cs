// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
[RequireComponent(typeof(Text))]
public class PopulateUITextFromTextAsset : MonoBehaviour
{
    [SerializeField]
    TextAsset textAsset;

    Text uiText;

    // Start is called before the first frame update
    void Start()
    {
        uiText = GetComponent<Text>();
        PopulateUI();
    }

    // Update is called once per frame
    void Update()
    {
        if (Application.isEditor)
        {
            PopulateUI();
        }
    }

    void PopulateUI()
    {
        if (textAsset)
        {
            uiText.text = textAsset.text;
        }
    }
}
