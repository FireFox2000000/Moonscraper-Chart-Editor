// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventToolSwitcher : ToolObject {
    [SerializeField]
    GameObject chartEventTool = null;
    [SerializeField]
    GameObject songEventTool = null;
	// Update is called once per frame
	new void Update () {
        bool useChartTool = Globals.viewMode == Globals.ViewMode.Chart;
        chartEventTool.SetActive(useChartTool);
        songEventTool.SetActive(!useChartTool);
    }
}
