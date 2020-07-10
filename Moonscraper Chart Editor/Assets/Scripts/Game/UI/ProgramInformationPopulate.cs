// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class ProgramInformationPopulate : MonoBehaviour {
    Text programInfoText;
    const string c_linuxFunctionalityAuthor = "bmwalters (https://github.com/bmwalters)";

	// Use this for initialization
	void Start () {
        programInfoText = GetComponent<Text>();

        programInfoText.text = string.Format("{0} v{1} {2} \nBy Alexander \"FireFox\" Ong.\n\nBuilt using Unity {3}.\n\nLinux build functionality provided by {4}", 
            Application.productName, 
            Application.version, 
            Globals.applicationBranchName, 
            Application.unityVersion,
            c_linuxFunctionalityAuthor);
    }
}
