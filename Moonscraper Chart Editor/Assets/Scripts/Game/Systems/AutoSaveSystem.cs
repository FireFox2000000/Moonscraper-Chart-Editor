using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoSaveSystem : SystemManagerState.System
{
    const float AUTOSAVE_RUN_INTERVAL = 60; // Once a minute
    float autosaveTimer = 0;
    Song autosaveSong = null;
    System.Threading.Thread autosave;

    public override void Update()
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
            autosaveSong.Save(Globals.autosaveLocation, editor.currentSong.defaultExportOptions);
            Debug.Log("Autosave complete!");
            autosaveTimer = 0;
        });

        autosave.Start();
    }
}
