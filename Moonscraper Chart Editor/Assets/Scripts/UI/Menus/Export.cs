using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System;

public class Export : DisplayMenu {

    bool forced = true;

    public Export()
    {
        try
        {
            string saveLocation = FileExplorer.SaveFilePanel("Chart files (*.chart)\0*.chart", editor.currentSong.name, "chart");
        }
        catch
        {
            // Cancel
        }
    }
}
