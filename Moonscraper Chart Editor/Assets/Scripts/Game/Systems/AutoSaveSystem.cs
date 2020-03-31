﻿// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoSaveSystem : SystemManagerState.System
{
    const float AUTOSAVE_RUN_INTERVAL = 60; // Once a minute
    float autosaveTimer = 0;
    Song autosaveSong = null;
    System.Threading.Thread autosave;

    public override void SystemUpdate()
    {
        if (autosave == null || autosave.ThreadState != System.Threading.ThreadState.Running)
        {
            autosaveTimer += Time.deltaTime;

            if (autosaveTimer > AUTOSAVE_RUN_INTERVAL)
            {
                Autosave();
            }
        }
        else
        {
            autosaveTimer = 0;
        }
    }

    void Autosave()
    {
        ChartEditor editor = ChartEditor.Instance;

        autosaveSong = new Song(editor.currentSong);

        autosave = new System.Threading.Thread(() =>
        {
            autosaveTimer = 0;
            Debug.Log("Autosaving...");

            string saveErrorMessage;
            try
            {
                new ChartWriter(Globals.autosaveLocation).Write(autosaveSong, editor.currentSong.defaultExportOptions, out saveErrorMessage);

                Debug.Log("Autosave complete!");

                if (saveErrorMessage != string.Empty)
                {
                    Debug.LogError("Autosave completed with the following errors: " + Globals.LINE_ENDING + saveErrorMessage);
                }
            }
            catch (System.Exception e)
            {
                Logger.LogException(e, "Autosave failed!");
            }

            Debug.Log("Autosave complete!");
            autosaveTimer = 0;
        });

        autosave.Start();
    }
}
