// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ErrorMessage : DisplayMenu {
    public string errorMessage = "No error";
    public Text errorText;

    protected override void OnEnable()
    {
        base.OnEnable();
        errorText.text = errorMessage;
    }
}
