// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using System.Threading.Tasks;
using MoonscraperEngine;
using MoonscraperChartEditor.Song;
using MoonscraperChartEditor.Song.IO;

public class AutoSaveSystem : SystemManagerState.System
{
    const float AUTOSAVE_RUN_INTERVAL = 60; // Once a minute
    float autosaveTimer = 0;
    Song autosaveSong = null;
    Task currentAutosaveTask;

    public override void SystemUpdate()
    {
        if (currentAutosaveTask == null || currentAutosaveTask.Status != TaskStatus.RanToCompletion)
        {
            autosaveTimer += Time.deltaTime;

            if (autosaveTimer > AUTOSAVE_RUN_INTERVAL)
            {
                currentAutosaveTask = Autosave();
            }
        }
        else
        {
            autosaveTimer = 0;
        }
    }

    async Task Autosave()
    {
        ChartEditor editor = ChartEditor.Instance;

        autosaveSong = new Song(editor.currentSong);

        await Task.Run(() =>
        {
            autosaveTimer = 0;
            Debug.Log("Autosaving...");

            string saveErrorMessage;
            try
            {
                ChartWriter.ErrorReport errorReport;
                new ChartWriter(Globals.autosaveLocation).Write(autosaveSong, editor.currentSong.defaultExportOptions, out errorReport);

                saveErrorMessage = errorReport.errorList.ToString();

                if (saveErrorMessage != string.Empty && errorReport.hasNonErrorFileTypeRelatedErrors)
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
    }
}
