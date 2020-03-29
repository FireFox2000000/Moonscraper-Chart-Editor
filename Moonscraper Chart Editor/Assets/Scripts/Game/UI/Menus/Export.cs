// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System;
using System.IO;
using UnityEngine.UI;
using Un4seen.Bass.Misc;
using Un4seen.Bass;

public class Export : DisplayMenu {
    public Text exportingInfo;
    public Dropdown fileTypeDropdown;
    public Toggle chPackageToggle;
    public Toggle forcedToggle;
    public InputField targetResolution;
    public InputField delayInputField;
    public Toggle copyDifficultiesToggle;
    public Toggle generateIniToggle;
    public Button magmaButton;

    ExportOptions exportOptions;
    float delayTime = 0;

    const string FILE_EXT_CHART = ".chart";
    const string FILE_EXT_MIDI = ".mid";

    string chartInfoText = "Exports into the .chart format.";
    string chPackageText = "Will export and organise all chart and audio files into the selected folder to be compatible with Clone Hero's naming structure.\n" +
        "This will also automatically re-encode audio into the .ogg format as needed.";
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

        chPackageToggle.isOn = false;
    }

    public void OnChPackageToggle(bool enabled)
    {
        if (enabled)
        {
            generateIniToggle.isOn = enabled;
            fileTypeDropdown.value = 0;
        }

        generateIniToggle.interactable = !enabled;        
        fileTypeDropdown.interactable = !enabled;
        magmaButton.interactable = !enabled;
        exportingInfo.text = chPackageToggle.isOn ? chPackageText : chartInfoText;
    }

    public void ExportSong()
    {
        if (chPackageToggle.isOn)
        {
            ExportCHPackage();
        }
        else
        {
            _ExportSong();
        }
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
            aquiredFilePath = FileExplorer.SaveFilePanel(new ExtensionFilter("Chart files", "chart"), defaultFileName, "chart", out saveLocation);
        }
        else if (exportOptions.format == ExportOptions.Format.Midi)
        {
            aquiredFilePath = FileExplorer.SaveFilePanel(new ExtensionFilter("Midi files", "mid"), defaultFileName, "mid", out saveLocation);
        }
        else
            throw new Exception("Invalid file extension");

        if (aquiredFilePath)
            StartCoroutine(_ExportSong(saveLocation));
    }

    public void ExportCHPackage()
    {
        string saveDirectory;
        if (FileExplorer.OpenFolderPanel(out saveDirectory))
        {
            Song song = editor.currentSong;
            float songLength = editor.currentSongLength;

            saveDirectory = saveDirectory.Replace('\\', '/');

            if (!saveDirectory.EndsWith("/"))
            {
                saveDirectory += '/';
            }

            saveDirectory += song.name + "/";

            StartCoroutine(ExportCHPackage(saveDirectory, song, songLength, exportOptions));
        }
    }

    public IEnumerator _ExportSong(string filepath)
    {
        LoadingTasksManager tasksManager = editor.services.loadingTasksManager;

        Song song = editor.currentSong;// new Song(editor.currentSong);
        float songLength = editor.currentSongLength;

        exportOptions.tickOffset = TickFunctions.TimeToDis(0, delayTime, exportOptions.targetResolution, 120);

        float timer = Time.realtimeSinceStartup;
        string errorMessageList = string.Empty;

        List<LoadingTask> tasks = new List<LoadingTask>()
        {
            new LoadingTask("Exporting " + exportOptions.format, () =>
            {
                if (exportOptions.format == ExportOptions.Format.Chart)
                {
                    try
                    {
                        Debug.Log("Exporting CHART file to " + filepath);
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
                        Debug.Log("Exporting MIDI file to " + filepath);
                        MidWriter.WriteToFile(filepath, song, exportOptions);
                    }
                    catch (System.Exception e)
                    {
                        Logger.LogException(e, "Error when exporting midi");
                        errorMessageList += e.Message;
                    }
                }
            })
        };

        if (generateIniToggle.isOn)
        {
            tasks.Add(new LoadingTask("Generating Song.ini", () =>
            {
                GenerateSongIni(Path.GetDirectoryName(filepath), song, songLength);
            }));
        }

        tasksManager.KickTasks(tasks);

        while (tasksManager.isRunningTask)
            yield return null;

        Debug.Log("Total exporting time: " + (Time.realtimeSinceStartup - timer));

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
        exportingInfo.text = chPackageToggle.isOn ? chPackageText : chartInfoText;
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

    static void GenerateSongIni(string path, Song song, float songLengthSeconds)
    {
        Metadata metaData = song.metaData;

        StreamWriter ofs = File.CreateText(path + "/song.ini");
        ofs.WriteLine("[Song]");
        ofs.WriteLine("name = " + song.name);
        ofs.WriteLine("artist = " + metaData.artist);
        ofs.WriteLine("album = " + metaData.album);
        ofs.WriteLine("genre = " + metaData.genre);
        ofs.WriteLine("year = " + metaData.year);
        ofs.WriteLine("song_length = " + (int)(songLengthSeconds * 1000));
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

    static readonly Dictionary<Song.AudioInstrument, string> audioInstrumentToCHNameMap = new Dictionary<Song.AudioInstrument, string>()
    {
        { Song.AudioInstrument.Song, "song" },
        { Song.AudioInstrument.Guitar, "guitar" },
        { Song.AudioInstrument.Bass, "bass" },
        { Song.AudioInstrument.Rhythm, "rhythm" },
		{ Song.AudioInstrument.Keys, "keys" },
		{ Song.AudioInstrument.Drum, "drums" },
		{ Song.AudioInstrument.Drums_2, "drums_2" },
		{ Song.AudioInstrument.Drums_3, "drums_3" },
		{ Song.AudioInstrument.Drums_4, "drums_4" },
		{ Song.AudioInstrument.Vocals, "vocals" },
		{ Song.AudioInstrument.Crowd, "crowd" },
    };

    static string GetCHOggFilename(Song.AudioInstrument audio)
    {
        const string audioFormat = ".ogg";

        string newAudioName = string.Empty;
        if (audioInstrumentToCHNameMap.TryGetValue(audio, out newAudioName))
        {
            newAudioName += audioFormat;
        }
        else
        {
            Debug.LogErrorFormat("Audio instrument {0} not set up in ch name dict. Skipping.", audio.ToString());
        }

        return newAudioName;
    }

    IEnumerator ExportCHPackage(string destFolderPath, Song song, float songLengthSeconds, ExportOptions exportOptions)
    {
        Song newSong = new Song(song);
        LoadingTasksManager tasksManager = editor.services.loadingTasksManager;

        float timer = Time.realtimeSinceStartup;
        string errorMessageList = string.Empty;

        destFolderPath = destFolderPath.Replace('\\', '/');

        if (!destFolderPath.EndsWith("/"))
        {
            destFolderPath += '/';
        }

        if (!Directory.Exists(destFolderPath))
        {
            Directory.CreateDirectory(destFolderPath);
        }

        List<LoadingTask> tasks = new List<LoadingTask>()
        {
            new LoadingTask("Re-encoding audio to .ogg format", () =>
            {
                ExportSongAudioOgg(destFolderPath, newSong);
            }),

            new LoadingTask("Exporting chart", () =>
            {
                string chartOutputFile = destFolderPath + "notes.chart";

                // Set audio location after audio files have already been created as set won't won't if the files don't exist
                foreach (Song.AudioInstrument audio in EnumX<Song.AudioInstrument>.Values)
                {
                    if (song.GetAudioLocation(audio) != string.Empty)
                    {
                        string audioFilename = GetCHOggFilename(audio);
                        string audioPath = destFolderPath + audioFilename;
                        newSong.SetAudioLocation(audio, audioPath);
                    }
                }

                new ChartWriter(chartOutputFile).Write(newSong, exportOptions, out errorMessageList);
                GenerateSongIni(destFolderPath, newSong, songLengthSeconds);
            }),
        };

        tasksManager.KickTasks(tasks);

        while (tasksManager.isRunningTask)
            yield return null;

        Debug.Log("Total exporting time: " + (Time.realtimeSinceStartup - timer));

        if (errorMessageList != string.Empty)
        {
            ChartEditor.Instance.errorManager.QueueErrorMessage("Encountered the following errors while exporting: " + Globals.LINE_ENDING + errorMessageList);
        }
    }

    static void ExportSongAudioOgg(string destFolderPath, Song song)
    {
        foreach (Song.AudioInstrument audio in EnumX<Song.AudioInstrument>.Values)
        {
            string audioLocation = song.GetAudioLocation(audio);
            if (audioLocation == string.Empty)
                continue;

            if (!File.Exists(audioLocation))
            {
                Debug.LogErrorFormat("Unable to find audio file in location {0}", audioLocation);
                continue;
            }
            {
                string newAudioName = GetCHOggFilename(audio);

                if (!string.IsNullOrEmpty(newAudioName))
                {
                    string outputFile = destFolderPath + newAudioName;

                    Debug.LogFormat("Converting ogg from {0} to {1}", audioLocation, outputFile);
                    AudioManager.ConvertToOgg(audioLocation, outputFile);
                }
                else
                {
                    Debug.LogErrorFormat("Audio instrument {0} not set up in ch name dict. Skipping.", audio.ToString());
                }
            }
        }
    }
}
