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
