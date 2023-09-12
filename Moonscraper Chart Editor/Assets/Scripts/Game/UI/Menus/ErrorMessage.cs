// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ErrorMessage : DisplayMenu {
    public string errorMessage = "No error";
    public Text errorText;

    const int MAX_CHARS = 4096;

    protected override void OnEnable()
    {
        base.OnEnable();

        if (errorMessage.Length > MAX_CHARS)
        {
            errorMessage = errorMessage.Substring(0, MAX_CHARS).Trim();
            errorMessage += "...";
        }
        errorText.text = errorMessage;
    }
}
