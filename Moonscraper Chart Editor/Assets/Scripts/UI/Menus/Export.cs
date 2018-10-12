// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System;
using System.Threading;
using System.IO;
using UnityEngine.UI;
using Un4seen.Bass.Misc;
using Un4seen.Bass;

public class Export : DisplayMenu {
    public LoadingScreenFader loadingScreen;
    public Text exportingInfo;
    public Dropdown fileTypeDropdown;
    public Toggle forcedToggle;
    public InputField targetResolution;
    public InputField delayInputField;
    public Toggle copyDifficultiesToggle;
    public Toggle generateIniToggle;

    ExportOptions exportOptions;
    float delayTime = 0;

    const string FILE_EXT_CHART = ".chart";
    const string FILE_EXT_MIDI = ".mid";

    string chartInfoText = "Exports into the .chart format.";
    string midInfoText = "Exports into the .mid format. \n\n" +
        "Warning: \n" +
        "\t-Audio will disconnect from file \n" +
        "\t-Starpower, taps and open note events will be defined by the expert chart if enabled \n" +

        "Exporting to Magma (Rock Band) notes: \n" +
        "\t-Resolution must be 480 \n" +
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

        exportOptions.forced = true;
        forcedToggle.isOn = exportOptions.forced;

        exportOptions.copyDownEmptyDifficulty = false;
        copyDifficultiesToggle.isOn = exportOptions.copyDownEmptyDifficulty;

        exportOptions.targetResolution = editor.currentSong.resolution;
        targetResolution.text = exportOptions.targetResolution.ToString();

        delayTime = 0;
        delayInputField.text = delayTime.ToString();
    }

    public void ExportSong()
    {
        editor.onClickEventFnList.Add(_ExportSong);
    }

    void _ExportSong()
    {
        string saveLocation;
        string defaultFileName = new string(editor.currentSong.name.ToCharArray());
        if (!exportOptions.forced)
            defaultFileName += "(UNFORCED)";

        bool aquiredFilePath = false;

        // Open up file explorer and get save location
        if (exportOptions.format == ExportOptions.Format.Chart)
        {
            aquiredFilePath = FileExplorer.SaveFilePanel("Chart files (*.chart)\0*.chart", defaultFileName, "chart", out saveLocation);
        }
        else if (exportOptions.format == ExportOptions.Format.Midi)
        {
            aquiredFilePath = FileExplorer.SaveFilePanel("Midi files (*.mid)\0*.mid", defaultFileName, "mid", out saveLocation);
        }
        else
            throw new Exception("Invalid file extension");

        if (aquiredFilePath)
            StartCoroutine(_ExportSong(saveLocation));
    }

    public IEnumerator _ExportSong(string filepath)
    {
        // Start saving
        Globals.applicationMode = Globals.ApplicationMode.Loading;
        loadingScreen.FadeIn();
        loadingScreen.loadingInformation.text = "Exporting " + exportOptions.format;

        Song song = new Song(editor.currentSong);
        exportOptions.tickOffset = TickFunctions.TimeToDis(0, delayTime, exportOptions.targetResolution, 120);

        float timer = Time.realtimeSinceStartup;
        string errorMessageList = string.Empty;
        Thread exportingThread = new Thread(() =>
        {
            if (exportOptions.format == ExportOptions.Format.Chart)
            {
                try
                {
                    new ChartWriter(filepath).Write(song, exportOptions, out errorMessageList);
                    //song.Save(filepath, exportOptions);
                }
                catch (System.Exception e)
                {
                    Logger.LogException(e, "Error when exporting chart");
                    errorMessageList += e.Message;
                }
            }
            else if (exportOptions.format == ExportOptions.Format.Midi)
            {
                try
                {
                    MidWriter.WriteToFile(filepath, song, exportOptions);
                }
                catch (System.Exception e)
                {
                    Logger.LogException(e, "Error when exporting midi");
                    errorMessageList += e.Message;
                }
            }
        });
     
        exportingThread.Start();

        while (exportingThread.ThreadState == ThreadState.Running)
            yield return null;

        if (generateIniToggle.isOn)
        {
            loadingScreen.loadingInformation.text = "Generating Song.ini";
            Thread iniThread = new Thread(() =>
            {
                GenerateSongIni(Path.GetDirectoryName(filepath));
            });

            iniThread.Start();

            while (iniThread.ThreadState == ThreadState.Running)
                yield return null;
        }

        Debug.Log("Total exporting time: " + (Time.realtimeSinceStartup - timer));

        // Stop loading animation
        loadingScreen.FadeOut();
        loadingScreen.loadingInformation.text = "Complete!";

        if (errorMessageList != string.Empty)
        {
            ChartEditor.Instance.errorManager.QueueErrorMessage("Encountered the following errors while exporting: " + Globals.LINE_ENDING + errorMessageList);
        }
    }

    public void SetForced(bool forced)
    {
        exportOptions.forced = forced;
    }

    public void SetCopyDiff(bool val)
    {
        exportOptions.copyDownEmptyDifficulty = val;
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

    public void SetResolution(string val)
    {
        int res;
        if (int.TryParse(val, out res) && res != 0)
            exportOptions.targetResolution = Mathf.Abs(res);
    }

    public void SetResolutionEnd(string val)
    {
        int res;
        if (!int.TryParse(val, out res))
            res = 192;

        if (res == 0)
            res = (int)(editor.currentSong.resolution);

        exportOptions.targetResolution = Mathf.Abs(res);
        targetResolution.text = exportOptions.targetResolution.ToString();
    }

    public void SetDelay(string val)
    {
        float delay;
        if (float.TryParse(val, out delay) && delay != 0)
            delayTime = Mathf.Abs(delay);
    }

    public void SetDelayEnd(string val)
    {
        float delay;
        if (!float.TryParse(val, out delay))
            delay = 0;

        delayTime = Mathf.Abs(delay);
        delayInputField.text = delayTime.ToString();
    }

    public void SetRBMagmaExport()
    {
        fileTypeDropdown.value = 1;
        forcedToggle.isOn = false;
        copyDifficultiesToggle.isOn = true;
        targetResolution.text = "480";
        delayInputField.text = "2.5";
    }

    void GenerateSongIni(string path)
    {
        Song song = editor.currentSong;
        Metadata metaData = song.metaData;

        StreamWriter ofs = File.CreateText(path + "/song.ini");
        ofs.WriteLine("[Song]");
        ofs.WriteLine("name = " + song.name);
        ofs.WriteLine("artist = " + metaData.artist);
        ofs.WriteLine("album = " + metaData.album);
        ofs.WriteLine("genre = " + metaData.genre);
        ofs.WriteLine("year = " + metaData.year);
        ofs.WriteLine("song_length = " + (int)(song.length * 1000));
        ofs.WriteLine("count = 0");
        ofs.WriteLine("diff_band = -1");
        ofs.WriteLine("diff_guitar = -1");
        ofs.WriteLine("diff_bass = -1");
        ofs.WriteLine("diff_drums = -1");
        ofs.WriteLine("diff_keys = -1");
        ofs.WriteLine("diff_guitarghl = -1");
        ofs.WriteLine("diff_bassghl = -1");
        ofs.WriteLine("preview_start_time = 0");
        ofs.WriteLine("frets = 0");
        ofs.WriteLine("charter = " + metaData.charter);
        ofs.WriteLine("icon = 0");

        ofs.Close();
    }

    public static void ExportWAV(string srcPath, string destPath, ExportOptions exportOptions)
    {
        Debug.Log("Exporting " + srcPath + " to " + destPath);
        int stream = Bass.BASS_StreamCreateFile(srcPath, 0, 0, BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_FLOAT);

        if (stream == 0 || Bass.BASS_ErrorGetCode() != BASSError.BASS_OK)
            throw new Exception(Bass.BASS_ErrorGetCode().ToString());

        WaveWriter ww = new WaveWriter(destPath, stream, true);

        float[] data = new float[32768];
        while (Bass.BASS_ChannelIsActive(stream) == BASSActive.BASS_ACTIVE_PLAYING)
        {
            // get the sample data as float values as well
            int length = Bass.BASS_ChannelGetData(stream, data, 32768);
            // and write the data to the wave file
            if (length > 0)
                ww.Write(data, length);
        }

        ww.Close();
        Bass.BASS_StreamFree(stream);
        /*
        const int WAV_HEADER_LENGTH = 44;

        

        FileStream ifs = null;
        BinaryReader br = null;

        FileStream ofs = null;
        BinaryWriter bw = null;
        
        try
        {
            ifs = new FileStream(srcPath, FileMode.Open, FileAccess.Read);
            br = new BinaryReader(ifs);

            ofs = new FileStream(destPath, FileMode.OpenOrCreate, FileAccess.Write);
            bw = new BinaryWriter(ofs);

            ifs.Seek(0, SeekOrigin.Begin);

            byte[] header = br.ReadBytes(WAV_HEADER_LENGTH);

            ifs.Seek(4, SeekOrigin.Begin);
            int chunkLength = br.ReadInt32(); // bytes 4 to 7

            ifs.Seek(16, SeekOrigin.Current);
            int frequency = br.ReadInt32();
            int byterate = br.ReadInt32();

            ifs.Seek(WAV_HEADER_LENGTH, SeekOrigin.Begin);
            byte[] chunk = br.ReadBytes(chunkLength); 

            
        }
        catch
        {
            Debug.LogError("Error with writing wav file");
        }
        finally
        {
            try { br.Close(); }
            catch { }

            try { ifs.Close(); }
            catch { }

            try { bw.Close(); }
            catch { }

            try { ofs.Close(); }
            catch { }
        }
        */
    }
}
