// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

//#undef UNITY_EDITOR

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System;
using MoonscraperEngine;

public class SongPropertiesPanelController : DisplayMenu {
    public Scrollbar verticalScroll;

    public InputField songName;
    public InputField artist;
    public InputField charter;
    public InputField album;
    public InputField year;
    public InputField offset;
    public InputField difficulty;
    public InputField genre;
    public InputField mediaType;

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

    bool init = false;

    TimeSpan customTime = new TimeSpan();

    readonly ExtensionFilter audioExFilter = new ExtensionFilter("Audio files", "ogg", "mp3", "wav");

    Dictionary<Song.AudioInstrument, Text> m_audioStreamTextLookup;

    private void Start()
    {
        offset.onValidateInput = LocalesManager.ValidateDecimalInput;
    }

    protected override void OnEnable()
    {
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

        ChartEditor.isDirty = edit;
        StartCoroutine(ScrollSetDelay());
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
        FileExplorer.OpenFilePanel(audioExFilter, "mp3,ogg,wav", out audioFilepath);
        return audioFilepath;
    }

    public void LoadMusicStream()
    {
        try
        {
            LoadAndSetStream(Song.AudioInstrument.Song);
        }
        catch
        {
            Debug.LogError("Could not open audio");
        }
    }

    public void ClearMusicStream()
    {
        ClearAudioStream(Song.AudioInstrument.Song);
    }

    public void LoadGuitarStream()
    {
        try
        {
            LoadAndSetStream(Song.AudioInstrument.Guitar);
        }
        catch
        {
            Debug.LogError("Could not open audio");
        }
    }

    public void ClearGuitarStream()
    {
        ClearAudioStream(Song.AudioInstrument.Guitar);
    }

    public void LoadBassStream()
    {
        try
        {
            LoadAndSetStream(Song.AudioInstrument.Bass);
        }
        catch
        {
            Debug.LogError("Could not open audio");
        }
    }

    public void ClearBassStream()
    {
        ClearAudioStream(Song.AudioInstrument.Bass);
    }

    public void LoadRhythmStream()
    {
        try
        {
            LoadAndSetStream(Song.AudioInstrument.Rhythm);
        }
        catch
        {
            Debug.LogError("Could not open audio");
        }
    }

    public void ClearRhythmStream()
    {
        ClearAudioStream(Song.AudioInstrument.Rhythm);
    }

   public void LoadVocalStream()
    {
        try
        {
            LoadAndSetStream(Song.AudioInstrument.Vocals);
        }
        catch
        {
            Debug.LogError("Could not open audio");
        }
    }

    public void ClearVocalStream()
    {
        ClearAudioStream(Song.AudioInstrument.Vocals);
    }
	
	public void LoadKeysStream()
    {
        try
        {
            LoadAndSetStream(Song.AudioInstrument.Keys);
        }
        catch
        {
            Debug.LogError("Could not open audio");
        }
    }

    public void ClearKeysStream()
    {
        ClearAudioStream(Song.AudioInstrument.Keys);
    }
	

    public void LoadDrum1Stream()
    {
        try
        {
            LoadAndSetStream(Song.AudioInstrument.Drum);
        }
        catch
        {
            Debug.LogError("Could not open audio");
        }
    }

    public void ClearDrum1Stream()
    {
        ClearAudioStream(Song.AudioInstrument.Drum);
    }
	
	public void LoadDrum2Stream()
    {
        try
        {
            LoadAndSetStream(Song.AudioInstrument.Drums_2);
        }
        catch
        {
            Debug.LogError("Could not open audio");
        }
    }

    public void ClearDrum2Stream()
    {
        ClearAudioStream(Song.AudioInstrument.Drums_2);
    }
	
	public void LoadDrum3Stream()
    {
        try
        {
            LoadAndSetStream(Song.AudioInstrument.Drums_3);
        }
        catch
        {
            Debug.LogError("Could not open audio");
        }
    }

    public void ClearDrum3Stream()
    {
        ClearAudioStream(Song.AudioInstrument.Drums_3);
    }
	
	public void LoadDrum4Stream()
    {
        try
        {
            LoadAndSetStream(Song.AudioInstrument.Drums_4);
        }
        catch
        {
            Debug.LogError("Could not open audio");
        }
    }

    public void ClearDrum4Stream()
    {
        ClearAudioStream(Song.AudioInstrument.Drums_4);
    }
	
	public void LoadCrowdStream()
    {
        try
        {
            LoadAndSetStream(Song.AudioInstrument.Crowd);
        }
        catch
        {
            Debug.LogError("Could not open audio");
        }
    }

    void LoadAndSetStream(Song.AudioInstrument audioInstrument)
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
        catch
        {
            Debug.LogError("Could not open audio");
        }
    }

    public void ClearCrowdStream()
    {
        ClearAudioStream(Song.AudioInstrument.Crowd);
    }

    void ClearAudioStream(Song.AudioInstrument audio)
    {
        editor.currentSongAudio.GetSampleData(audio).Dispose();
        editor.currentSongAudio.SetBassAudioStream(audio, null);
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
}
