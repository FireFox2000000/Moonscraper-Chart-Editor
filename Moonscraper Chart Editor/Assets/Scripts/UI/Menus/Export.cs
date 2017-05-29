﻿using System.Collections;
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

    ExportOptions exportOptions;

    const string FILE_EXT_CHART = ".chart";
    const string FILE_EXT_MIDI = ".mid";

    string chartInfoText = "Exports into the .chart format.";
    string midInfoText = "Exports into the .mid format. \n\n" +
        "Warning: \n" +
        "\t-Audio will disconnect from file \n" +
        "\t-Starpower, taps and open note events will be defined by the expert chart if enabled \n" +
        "\t-Guitar co-op chart will not be exported, they are \".chart\" exclusive \n" +
        "\t-Drum charts will be empty\n\n" +

        "Exporting to Magma (Rock Band) notes: \n" +
        "\t-Notes cannot be within the first 2.45 seconds of a song \n" +
        "\t-Charts must be UNFORCED and contain no open notes \n" +
        "\t-Magma has reserved names for sections that must be followed for successful compilation. " +
        "For example, for Magma to read a section called \"Intro a\", the section should be labeled as \"intro_a\" in Moonscraper. " +
        "A full list of sections can be found at http://pksage.com/rbndocs/index.php?title=All_Practice_Sections \n";

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
            string defaultFileName = new string(editor.currentSong.name.ToCharArray());
            if (!exportOptions.forced)
                defaultFileName += "(UNFORCED)";

            // Open up file explorer and get save location
            if (exportOptions.format == ExportOptions.Format.Chart)
            {
                saveLocation = FileExplorer.SaveFilePanel("Chart files (*.chart)\0*.chart", defaultFileName, "chart");
            }
            else if (exportOptions.format == ExportOptions.Format.Midi)
            {
                saveLocation = FileExplorer.SaveFilePanel("Midi files (*.mid)\0*.mid", defaultFileName, "mid");
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
        loadingScreen.loadingInformation.text = "Exporting " + exportOptions.format;

        Song song = editor.currentSong;
        float timer = Time.realtimeSinceStartup;
        
        Thread exportingThread = new Thread(() =>
        {
            if (exportOptions.format == ExportOptions.Format.Chart)
                song.Save(filepath, exportOptions.forced);
            else if (exportOptions.format == ExportOptions.Format.Midi)
            {
                // TEMP
                exportOptions.targetResolution = 480;
                exportOptions.copyDownEmptyDifficulty = true;
                exportOptions.tickOffset = Song.time_to_dis(0, 2.5f, 480, 120);


                MidWriter.WriteToFile(filepath, song, exportOptions);
            }
        });

        exportingThread.Start();

        while (exportingThread.ThreadState == ThreadState.Running)
            yield return null;
        
        /*
        if (exportOptions.format == ExportOptions.Format.Chart)
            song.Save(filepath, exportOptions.forced);
        else if (exportOptions.format == ExportOptions.Format.Midi)
        {
            // TEMP
            exportOptions.targetResolution = 480;
            exportOptions.copyDownEmptyDifficulty = true;
            exportOptions.tickOffset = Song.time_to_dis(0, 2.5f, 480, 120);


            MidWriter.WriteToFile(filepath, song, exportOptions);
        }*/

        Debug.Log("Total exporting time: " + (Time.realtimeSinceStartup - timer));

        // Stop loading animation
        loadingScreen.FadeOut();
        loadingScreen.loadingInformation.text = "Complete!";
    }

    public void SetForced(bool forced)
    {
        exportOptions.forced = forced;
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
        exportOptions.format = ExportOptions.Format.Chart;
        exportingInfo.text = chartInfoText;
    }

    void setAsMidFile()
    {
        exportOptions.format = ExportOptions.Format.Midi;
        exportingInfo.text = midInfoText;
    }
}