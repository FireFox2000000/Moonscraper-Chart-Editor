// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ErrorMessage : DisplayMenu {
    public static string errorMessage = "No error";
    public Text errorText;
	
	// Update is called once per frame
	protected override void Update () {
        errorText.text = errorMessage;
	}
}
