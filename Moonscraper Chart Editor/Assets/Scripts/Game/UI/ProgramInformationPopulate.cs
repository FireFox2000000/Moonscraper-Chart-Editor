// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class ProgramInformationPopulate : MonoBehaviour {
    Text programInfoText;

	// Use this for initialization
	void Start () {
        programInfoText = GetComponent<Text>();

        programInfoText.text = string.Format("{0} v{1} \nBy Alexander \"FireFox\" Ong.\nBuilt using Unity {2}.", Application.productName, Application.version, Application.unityVersion);
    }
}
