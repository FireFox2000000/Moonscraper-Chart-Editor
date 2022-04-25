// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayMenuCrashTest : MonoBehaviour {
    Export exportMenu;
    bool running = false;

	// Use this for initialization
	void Start () {
        exportMenu = ChartEditor.Instance.uiServices.GetComponentInChildren<Export>(true);
        Debug.Assert(exportMenu, "Unable to locate Export menu");
    }
	
	// Update is called once per frame
	void Update () {
        try
        {
            if (exportMenu.gameObject.activeSelf)
                exportMenu.Disable();
            else
                ChartEditor.Instance.EnableMenu(exportMenu);
        }
        catch (System.Exception e)
        {
            Logger.LogException(e, "DisplayMenuCrashTest caught error");
            enabled = false;
        }
	}
}
