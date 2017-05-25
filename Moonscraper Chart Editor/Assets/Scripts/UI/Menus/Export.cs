using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System;
using System.Threading;
using UnityEngine.UI;

public class Export : DisplayMenu {
    public LoadingScreenFader loadingScreen;
    public Text exportingInfo;
    public Dropdown fileTypeDropdown;
    public Toggle forcedToggle;

    const string FILE_EXT_CHART = ".chart";
    const string FILE_EXT_MIDI = ".mid";

    bool forced = true;
    string fileExportType = FILE_EXT_CHART;

    string chartInfoText = "Exports into the .chart format.";
    string midInfoText = "Exports into the .mid format. \n\n" + 
        "Warning: \n" +
        "\t-Audio will disconnect from file \n" +
        "\t-Tap and open note events will be defined by the expert chart if enabled \n" +
        "\t-Guitar co-op chart will not be exported, they are \".chart\" exclusive \n" +
        "\t-Drum charts will be empty\n";

    void Start()
    {
        setAsChartFile();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        fileTypeDropdown.value = 0;
        forcedToggle.isOn = true;
    }

    public void ExportSong()
    {
        try
        {
            string saveLocation;

            // Open up file explorer and get save location
            if (fileExportType == FILE_EXT_CHART)
            {
                saveLocation = FileExplorer.SaveFilePanel("Chart files (*.chart)\0*.chart", editor.currentSong.name, "chart");
            }
            else if (fileExportType == FILE_EXT_MIDI)
            {
                saveLocation = FileExplorer.SaveFilePanel("Midi files (*.mid)\0*.mid", editor.currentSong.name, "mid");
            }
            else
                throw new Exception("Invalid file extension");

            StartCoroutine(_ExportSong(saveLocation));
        }
        catch
        {
            // User probably canceled
        }
    }

    public IEnumerator _ExportSong(string filepath)
    {
        // Start saving
        Globals.applicationMode = Globals.ApplicationMode.Loading;
        loadingScreen.FadeIn();
        loadingScreen.loadingInformation.text = "Exporting " + fileExportType;

        Song song = editor.currentSong;

        Thread exportingThread = new Thread(() =>
        {
            if (fileExportType == FILE_EXT_CHART)
                song.Save(filepath, forced);
            else if (fileExportType == FILE_EXT_MIDI)
                MidWriter.WriteToFile(filepath, song, forced);
        });

        exportingThread.Start();

        while (exportingThread.ThreadState == ThreadState.Running)
            yield return null;

        // Stop loading animation
        Globals.applicationMode = Globals.ApplicationMode.Editor;
        loadingScreen.FadeOut();
        loadingScreen.loadingInformation.text = "Complete!";
    }

    public void SetForced(bool forced)
    {
        this.forced = forced;
    }

    public void SetFile(int value)
    {
        switch (value)
        {
            case 1:
                setAsMidFile();
                break;
            case 0:
            default:
                setAsChartFile();
                break;
        }
    }

    void setAsChartFile()
    {
        fileExportType = FILE_EXT_CHART;
        exportingInfo.text = chartInfoText;
    }

    void setAsMidFile()
    {
        fileExportType = FILE_EXT_MIDI;
        exportingInfo.text = midInfoText;
    }
}
