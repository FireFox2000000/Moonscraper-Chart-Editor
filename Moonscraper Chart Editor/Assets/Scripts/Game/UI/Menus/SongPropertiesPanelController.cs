// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

//#undef UNITY_EDITOR

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;
using MoonscraperEngine;
using MoonscraperChartEditor.Song;

public class SongPropertiesPanelController : TabMenu
{
    public Scrollbar verticalScroll;

    [Header("General Settings")]
    public InputField songName;
    public InputField artist;
    public InputField charter;
    public InputField album;
    public InputField year;
    public InputField offset;
    public InputField difficulty;
    public InputField genre;
    public InputField mediaType;

    [Header("Audio Settings")]
    public Text musicStream;
    public Text guitarStream;
    public Text bassStream;
    public Text rhythmStream;
	public Text keysStream;
	public Text vocalStream;
    public Text drum1Stream;
	public Text drum2Stream;
	public Text drum3Stream;
	public Text drum4Stream;
    public Text crowdStream;

    [Header("Advanced Settings")]
    public MS_TMPro_InputField customIniSettings;

    bool init = false;

    TimeSpan customTime = new TimeSpan();

    static readonly string[] validAudioExtensions = { "ogg", "wav", "mp3", "opus" };
    readonly ExtensionFilter audioExFilter = new ExtensionFilter("Audio files", validAudioExtensions);

    Dictionary<Song.AudioInstrument, Text> m_audioStreamTextLookup;
    readonly string[] INI_SECTION_HEADER = { "Song", "song" };

    protected override void Start()
    {
        base.Start();
        offset.onValidateInput = LocalesManager.ValidateDecimalInput;
    }

    protected override void OnEnable()
    {
        ResetToInitialMenuItem();

        m_audioStreamTextLookup = new Dictionary<Song.AudioInstrument, Text>()
        {
            { Song.AudioInstrument.Song, musicStream },
            { Song.AudioInstrument.Guitar, guitarStream },
            { Song.AudioInstrument.Bass, bassStream },
            { Song.AudioInstrument.Rhythm, rhythmStream },
            { Song.AudioInstrument.Keys, keysStream },
            { Song.AudioInstrument.Vocals, vocalStream },
            { Song.AudioInstrument.Drum, drum1Stream },
            { Song.AudioInstrument.Drums_2, drum2Stream },
            { Song.AudioInstrument.Drums_3, drum3Stream },
            { Song.AudioInstrument.Drums_4, drum4Stream },
            { Song.AudioInstrument.Crowd, crowdStream },
        };

        bool edit = ChartEditor.isDirty;

        base.OnEnable();

        init = true;
        Song song = editor.currentSong;
        Metadata metaData = song.metaData;

        songName.text = song.name;
        artist.text = metaData.artist;
        charter.text = metaData.charter;
        album.text = metaData.album;
        year.text = metaData.year;
        offset.text = song.offset.ToString();
        difficulty.text = metaData.difficulty.ToString();
        genre.text = metaData.genre;
        mediaType.text = metaData.mediatype;

        // Init audio names
        setAudioTextLabels();
        init = false;

        customTime = TimeSpan.FromSeconds(editor.currentSongLength);

        UpdateIniTextFromSongProperties();

        ChartEditor.isDirty = edit;
        StartCoroutine(ScrollSetDelay());
    }

    protected override void OnDisable()
    {
        UpdateIni();
        base.OnDisable();
    }

    IEnumerator ScrollSetDelay()
    {
        yield return null;
        verticalScroll.value = 1;
    }

    void Apply()
    {
        editor.currentSong.name = songName.text;
        editor.currentSong.metaData.artist = artist.text;
    }

    public void setSongProperties()
    {
        if (!init)
        {
            Song song = editor.currentSong;
            Metadata metaData = song.metaData;

            if (!song.name.Equals(songName.text))
            {
                song.name = songName.text;
                editor.uiServices.editorPanels.displayProperties.UpdateSongNameText();
                editor.RepaintWindowText();
            }

            metaData.artist = artist.text;
            metaData.charter = charter.text;
            metaData.album = album.text;
            metaData.year = year.text;

            try
            {
                song.offset = float.Parse(offset.text);
            }
            catch
            {
                song.offset = 0;
            }

            try
            {
                metaData.difficulty = int.Parse(difficulty.text);
            }
            catch
            {
                metaData.difficulty = 0;
            }

            metaData.genre = genre.text;
            metaData.mediatype = mediaType.text;

            if (editor.currentSong.manualLength.HasValue)   // if we were already using the manual length
            {
                editor.currentSong.manualLength = (float)customTime.TotalSeconds;
            }

            ChartEditor.isDirty = true;
        }
    }

    public void UpdateIni()
    {
        Song song = editor.currentSong;

        INIParser newProperties = new INIParser();

        string[] seperatingTags = { Environment.NewLine.ToString(), "\n" };
        string[] customIniLines = customIniSettings.text.Split(seperatingTags, StringSplitOptions.None);

        foreach (string line in customIniLines)
        {
            string[] keyVal = line.Split('=');
            if (keyVal.Length >= 1)
            {
                string key = keyVal[0].Trim();
                string val = keyVal.Length > 1 ? keyVal[1].Trim() : string.Empty;

                if (!string.IsNullOrEmpty(key))
                    newProperties.WriteValue(INI_SECTION_HEADER[0], key + " ", " " + val);
            }
        }

        song.iniProperties = newProperties;
    }

    void UpdateIniTextFromSongProperties()
    {
        string str = editor.currentSong.iniProperties.GetSectionValues(INI_SECTION_HEADER, INIParser.Formatting.Whitespaced);
        str = str.Replace("\r\n", "\n");
        customIniSettings.text = str;
    }

    public void RefreshIniDisplay()
    {
        UpdateIni();
        UpdateIniTextFromSongProperties();
        ChartEditor.isDirty = true;
    }

    public void PopulateIniFromGeneralSettings()
    {
        RefreshIniDisplay();
        SongIniFunctions.PopulateIniWithSongMetadata(editor.currentSong, editor.currentSong.iniProperties, editor.currentSongLength);
        UpdateIniTextFromSongProperties();
        ChartEditor.isDirty = true;
    }

    public void AddCloneHeroIniTags()
    {
        RefreshIniDisplay();

        var song = editor.currentSong;
        var iniParser = song.iniProperties;

        SongIniFunctions.AddCloneHeroIniTags(song, iniParser, editor.currentSongLength);
        UpdateIniTextFromSongProperties();
        ChartEditor.isDirty = true;
    }

    public void OnIniInputValueChanged()
    {
        ChartEditor.isDirty = true;
    }

    public void OnIniInputEndEdit()
    {
        RefreshIniDisplay();
    }

    void ClipText(Text text)
    {
        float maxWidth = text.rectTransform.rect.width;
        if (text.preferredWidth > maxWidth)
        {
            int removePos = text.text.Length - 1;
            text.text += "...";

            while (removePos > 0 && text.preferredWidth > maxWidth)
            {
                text.text = text.text.Remove(removePos--, 1);
            }
        }
    }

   void setAudioTextLabels()
    {
        Song song = editor.currentSong;

        foreach (var audio in EnumX<Song.AudioInstrument>.Values)
        {
            Text audioStreamText;

            if (!m_audioStreamTextLookup.TryGetValue(audio, out audioStreamText))
            {
                Debug.Assert(false, "Audio stream UI Text not linked to an Audio Instrument for instrument " + audio.ToString());
                continue;
            }

            if (editor.currentSongAudio.GetAudioIsLoaded(audio))
            {
                audioStreamText.color = Color.white;
                audioStreamText.text = song.GetAudioName(audio);
                ClipText(audioStreamText);
            }
            else
            {
                audioStreamText.color = Color.red;
                audioStreamText.text = "No audio";
            }
        }

        ChartEditor.isDirty = true;
    }

    string GetAudioFile()
    {
        string audioFilepath = string.Empty;
        string defExt = string.Empty;
        foreach(string extention in validAudioExtensions)
        {
            if (defExt != string.Empty)
                defExt += ",";

            defExt += extention;
        }

        FileExplorer.OpenFilePanel(audioExFilter, defExt, out audioFilepath);
        return audioFilepath;
    }

    void LoadAudioStream(Song.AudioInstrument audioInstrument)
    {
        try
        {
            string filepath = GetAudioFile();
            if (editor.currentSongAudio.LoadAudio(filepath, audioInstrument))
            {
                // Record the filepath
                editor.currentSong.SetAudioLocation(audioInstrument, filepath);
                StartCoroutine(SetAudio());
            }
        }
        catch (Exception e)
        {
            Logger.LogException(e, "Could not open audio");
        }
    }

    void ClearAudioStream(Song.AudioInstrument audio)
    {
        editor.currentSongAudio.Clear(audio);
        editor.currentSong.SetAudioLocation(audio, string.Empty);

        setAudioTextLabels();
    }

    IEnumerator SetAudio()
    {
        LoadingTasksManager tasksManager = editor.services.loadingTasksManager;

        List<LoadingTask> tasks = new List<LoadingTask>()
        {
            new LoadingTask("Loading audio", () =>
            {
                while (editor.currentSongAudio.isAudioLoading) ;
            })
        };

        tasksManager.KickTasks(tasks);

        while (tasksManager.isRunningTask)
            yield return null;

        setAudioTextLabels();
    }

    // Unity doesn't support calling methods with enum parameters
    // From button click handlers, so we define load/clear audio
    // implementations for each case here.
    #region Unity button click handlers
    public void LoadMusicStream()
    {
        LoadAudioStream(Song.AudioInstrument.Song);
    }

    public void ClearMusicStream()
    {
        ClearAudioStream(Song.AudioInstrument.Song);
    }

    public void LoadGuitarStream()
    {
        LoadAudioStream(Song.AudioInstrument.Guitar);
    }

    public void ClearGuitarStream()
    {
        ClearAudioStream(Song.AudioInstrument.Guitar);
    }

    public void LoadBassStream()
    {
        LoadAudioStream(Song.AudioInstrument.Bass);
    }

    public void ClearBassStream()
    {
        ClearAudioStream(Song.AudioInstrument.Bass);
    }

    public void LoadRhythmStream()
    {
        LoadAudioStream(Song.AudioInstrument.Rhythm);
    }

    public void ClearRhythmStream()
    {
        ClearAudioStream(Song.AudioInstrument.Rhythm);
    }

    public void LoadVocalStream()
    {
        LoadAudioStream(Song.AudioInstrument.Vocals);
    }

    public void ClearVocalStream()
    {
        ClearAudioStream(Song.AudioInstrument.Vocals);
    }

	public void LoadKeysStream()
    {
        LoadAudioStream(Song.AudioInstrument.Keys);
    }

    public void ClearKeysStream()
    {
        ClearAudioStream(Song.AudioInstrument.Keys);
    }

    public void LoadDrum1Stream()
    {
        LoadAudioStream(Song.AudioInstrument.Drum);
    }

    public void ClearDrum1Stream()
    {
        ClearAudioStream(Song.AudioInstrument.Drum);
    }

	public void LoadDrum2Stream()
    {
        LoadAudioStream(Song.AudioInstrument.Drums_2);
    }

    public void ClearDrum2Stream()
    {
        ClearAudioStream(Song.AudioInstrument.Drums_2);
    }

	public void LoadDrum3Stream()
    {
        LoadAudioStream(Song.AudioInstrument.Drums_3);
    }

    public void ClearDrum3Stream()
    {
        ClearAudioStream(Song.AudioInstrument.Drums_3);
    }

	public void LoadDrum4Stream()
    {
        LoadAudioStream(Song.AudioInstrument.Drums_4);
    }

    public void ClearDrum4Stream()
    {
        ClearAudioStream(Song.AudioInstrument.Drums_4);
    }

	public void LoadCrowdStream()
    {
        LoadAudioStream(Song.AudioInstrument.Crowd);
    }

    public void ClearCrowdStream()
    {
        ClearAudioStream(Song.AudioInstrument.Crowd);
    }
    #endregion
}
